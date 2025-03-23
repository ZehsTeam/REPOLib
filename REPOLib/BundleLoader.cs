using BepInEx;
using REPOLib.Modules;
using REPOLib.Objects.Sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REPOLib;

public static class BundleLoader
{
    public static event Action OnAllBundlesLoaded;
    
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
        Logger.LogInfo($"Loading bundle at {relativePath}...");

        var start = DateTime.Now;
        var request = AssetBundle.LoadFromFileAsync(path);
        
        var operation = new LoadOperation(start, request, relativePath);
        _operations.Add(operation);
    }

    internal static void FinishLoadOperations(MonoBehaviour behaviour)
    {
        behaviour.StartCoroutine(FinishLoadOperationsRoutine(behaviour));
    }

    private static IEnumerator FinishLoadOperationsRoutine(MonoBehaviour behaviour)
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

                string bundlesWord = _operations.Count == 1 ? "bundle": "bundles" ;
                text.text = $"REPOLib: Waiting for {_operations.Count} {bundlesWord} to load...";

                if (!ConfigManager.ExtendedLogging.Value) continue;
                    
                foreach (var operation in _operations)
                {
                    string msg = $"Loading {operation.FileName}: {operation.CurrentState}";
                    float? progress = operation.CurrentState switch
                    {
                        LoadOperation.State.LoadingBundle => operation.BundleRequest.progress,
                        _ => null
                    };
                    
                    if (progress.HasValue) msg += $" {progress.Value:P0}";
                    
                    Logger.LogInfo(msg, extended: true);
                }
            }
            
            yield return null;
        }

        Logger.LogInfo("Finished loading bundles.");
        
        disableLoadingUI();
        Utilities.SafeInvokeEvent(OnAllBundlesLoaded);
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
        text.text = "REPOLib is loading bundles... Hang tight!";
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
        yield return operation.BundleRequest;
        var bundle = operation.BundleRequest.assetBundle;
        
        if (bundle == null)
        {
            Logger.LogError($"Failed to load bundle {operation.FileName}!");
            Finish();
            yield break;
        }

        operation.CurrentState = LoadOperation.State.LoadingAssets;
        operation.AssetRequest = bundle.LoadAllAssetsAsync<ScriptableObject>();
        yield return operation.AssetRequest;
        
        Object[] assets = operation.AssetRequest.allAssets;
        Mod[] mods = assets.OfType<Mod>().ToArray();
        
        switch (mods.Length)
        {
            case 0:
                Logger.LogError($"Bundle {operation.FileName} contains no mods!");
                Finish();
                yield break;
            case > 1:
                Logger.LogError($"Bundle {operation.FileName} contains more than one mod!");
                Finish();
                yield break;
        }

        var mod = mods[0];
        
        foreach (var content in assets.OfType<Content>())
        {
            try
            {
                content.Initialize(mod);
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to load {content.Name} ({content.GetType().Name}) from bundle {operation.FileName} ({mod.Identifier}): {e}");
            }
        }

        if (ConfigManager.ExtendedLogging.Value)
        {
            Logger.LogInfo($"Loaded bundle {operation.FileName} ({mod.Identifier}) in {operation.ElapsedTime.TotalSeconds:N1}s");
        }
        
        Finish();
        yield break;
        
        void Finish()
        {
            _operations.Remove(operation);
            
            Logger.LogInfo($"Unloading bundle {operation.FileName}...", extended: true);
            bundle.UnloadAsync(unloadAllLoadedObjects: false);
        }
    }
    
    private class LoadOperation(DateTime startTime, AssetBundleCreateRequest bundleRequest, string relativePath)
    {
        public string RelativePath { get; } = relativePath;
        public DateTime StartTime { get; } = startTime;
        public State CurrentState { get; set; } = State.LoadingBundle;
        
        public AssetBundleCreateRequest BundleRequest { get; } = bundleRequest;
        public AssetBundleRequest AssetRequest { get; set; }
        
        public TimeSpan ElapsedTime => DateTime.Now - StartTime;

        public string FileName => Path.GetFileNameWithoutExtension(RelativePath);
        
        public enum State
        {
            LoadingBundle,
            LoadingAssets,
        }
    }
}
