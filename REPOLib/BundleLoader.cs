using BepInEx;
using REPOLib.Objects.Sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REPOLib;

public static class BundleLoader
{
    private static readonly List<LoadOperation> _operations = [];
    
    public static void LoadAllBundles(string root, string withExtension)
    {
        Logger.LogInfo($"Loading all bundles with extension {withExtension} from root {root}", extended: true);
        
        string[] files = Directory.GetFiles(root, "*" + withExtension, SearchOption.AllDirectories);

        foreach (string path in files)
        {
            LoadBundle(path, path.Replace(root, ""));
        }
    }

    public static void LoadBundle(string path)
    {
        LoadBundle(path, path.Replace(Paths.PluginPath, ""));
    }

    public static void LoadBundle(string path, string relativePath)
    {
        Logger.LogInfo($"Loading bundle at {relativePath}...", extended: true);

        var start = DateTime.Now;
        var request = AssetBundle.LoadFromFileAsync(path);
        
        var operation = new LoadOperation(start, request, relativePath);
        _operations.Add(operation);
    }

    internal static void FinishLoadOperations(MonoBehaviour behaviour, Action onFinished)
    {
        behaviour.StartCoroutine(FinishLoadOperationsRoutine(behaviour, onFinished));
    }

    private static IEnumerator FinishLoadOperationsRoutine(MonoBehaviour behaviour, Action onFinished)
    {
        yield return null;
        
        foreach (var loadOperation in _operations.ToArray()) // collection might change
        {
            behaviour.StartCoroutine(FinishLoadOperation(loadOperation));
        }
        
        var (text, disableLoadingUI) = SetupLoadingUI();

        float lastUpdate = Time.time;
        while (_operations.Count > 0)
        {
            if (Time.time - lastUpdate > 1)
            {
                lastUpdate = Time.time;

                text.text = $"REPOLib: Waiting for {_operations.Count} bundle(s) to load...";

                if (!ConfigManager.ExtendedLogging.Value) continue;
                    
                foreach (var operation in _operations)
                {
                    string msg = $"{operation.RelativePath}: {operation.CurrentState}";
                    if (operation.CurrentState == LoadOperation.State.LoadingBundle)
                    {
                        msg += $" {operation.Request.progress:P0}";
                    }
                    
                    Logger.LogInfo(msg, extended: true);
                }
            }
            
            yield return null;
        }

        onFinished();
        disableLoadingUI();
    }

    private static (TMP_Text, Action) SetupLoadingUI()
    {
        var hudCanvas = GameObject.Find("HUD Canvas");
        var hud = hudCanvas.transform.Find("HUD");
        hud.gameObject.SetActive(false);

        var buttonText = Object.FindObjectOfType<TMP_Text>();
        var text = Object.Instantiate(buttonText, hudCanvas.transform);
        text.gameObject.name = "REPOLibText";
        text.gameObject.SetActive(true);
        text.text = $"REPOLib is loading bundles...";
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        
        var rectTransform = text.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        return (text, () =>
        {
            text.gameObject.SetActive(false);
            hud.gameObject.SetActive(true);
        });
    }

    private static IEnumerator FinishLoadOperation(LoadOperation operation)
    {
        yield return operation.Request;
        var bundle = operation.Request.assetBundle;
        
        if (bundle == null)
        {
            Logger.LogError($"Failed to load bundle at {operation.RelativePath}!");
            Finish();
            yield break;
        }

        operation.CurrentState = LoadOperation.State.LoadingContent;

        var modsRequest = bundle.LoadAllAssetsAsync<Mod>();
        yield return modsRequest;
        Object[] mods = modsRequest.allAssets;
        
        switch (mods.Length)
        {
            case 0:
                Logger.LogError($"Bundle at {operation.RelativePath} contains no mods.");
                Finish();
                yield break;
            case > 1:
                Logger.LogError($"Bundle at {operation.RelativePath} contains more than one mod.");
                Finish();
                yield break;
        }
        
        var mod = (Mod)mods[0];
        
        var contentRequest = bundle.LoadAllAssetsAsync<Content>();
        yield return contentRequest;

        operation.CurrentState = LoadOperation.State.RegisteringContent;

        foreach (var obj in contentRequest.allAssets)
        {
            var content = (Content)obj;
            try
            {
                content.Initialize(mod);
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to load {content.Name} ({content.GetType().Name}) from {mod.Identifier} (at {operation.RelativePath}): {e}");
            }
        }
        
        Logger.LogInfo($"Loaded bundled mod {mod.Identifier} (at {operation.RelativePath}) in {operation.ElapsedTime.TotalSeconds:N1}s");

        Finish();
        yield break;
        
        void Finish()
        {
            _operations.Remove(operation);
        }
    }
    
    private class LoadOperation(DateTime startTime, AssetBundleCreateRequest request, string relativePath)
    {
        public string RelativePath { get; } = relativePath;
        public DateTime StartTime { get; } = startTime;
        public AssetBundleCreateRequest Request { get; } = request;
        public State CurrentState { get; set; } = State.LoadingBundle;
        
        public TimeSpan ElapsedTime => DateTime.Now - StartTime;

        public enum State
        {
            LoadingBundle,
            LoadingContent,
            RegisteringContent
        }
    }
}
