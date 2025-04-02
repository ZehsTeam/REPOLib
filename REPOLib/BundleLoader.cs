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
    public static event Action? OnAllBundlesLoaded;
    
    private static readonly List<LoadOperation> _operations = [];
    
    internal static void LoadAllBundles(string root, string withExtension)
    {
        Logger.LogInfo($"Loading all bundles with extension {withExtension} from root {root}", extended: true);
        
        string[] files = Directory.GetFiles(root, "*" + withExtension, SearchOption.AllDirectories);

        foreach (string path in files)
        {
            LoadBundleAndContent(path);
        }
    }
    
    public static void LoadBundleAndContent(string path)
    {
        LoadBundle(path, onLoaded: null, loadContents: true);
    }
    
    public static void LoadBundle(string path, Action<AssetBundle> onLoaded, bool loadContents = false)
    {
        LoadBundle(path, OnLoaded, loadContents);
        return;

        IEnumerator OnLoaded(AssetBundle bundle) 
        {
            onLoaded(bundle);
            yield break;
        }
    }
    
    public static void LoadBundle(string path, Func<AssetBundle, IEnumerator>? onLoaded = null, bool loadContents = false)
    {
        Logger.LogInfo($"Loading bundle at {path}...");
        _operations.Add(new LoadOperation(path, onLoaded, loadContents));
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
                    
                    Logger.LogDebug(msg, extended: true);
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
        
        if (operation.LoadContents)
        {
            yield return LoadBundleContent(operation, bundle);
        }
        
        if (operation.OnBundleLoaded != null)
        {
            yield return operation.OnBundleLoaded(bundle);
        }

        if (ConfigManager.ExtendedLogging.Value)
        {
            Logger.LogInfo($"Loaded bundle {operation.FileName} in {operation.ElapsedTime.TotalSeconds:N1}s");
        }
        
        Finish();
        yield break;

        void Finish()
        {
            _operations.Remove(operation);
        }
    }

    private static IEnumerator LoadBundleContent(LoadOperation operation, AssetBundle bundle)
    {
        operation.CurrentState = LoadOperation.State.LoadingContent;
        var assetRequest = bundle.LoadAllAssetsAsync<ScriptableObject>();
        yield return assetRequest;

        Object[] assets = assetRequest.allAssets;
        Mod[] mods = assets.OfType<Mod>().ToArray();

        switch (mods.Length)
        {
            case 0:
                Logger.LogError($"Bundle {operation.FileName} contains no mods!");
                yield break;
            case > 1:
                Logger.LogError($"Bundle {operation.FileName} contains more than one mod!");
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
    }

    private class LoadOperation(string path, Func<AssetBundle, IEnumerator>? onBundleLoaded = null, bool loadContents = true)
    {
        public string Path { get; } = path;
        public DateTime StartTime { get; } = DateTime.Now;
        public State CurrentState { get; set; } = State.LoadingBundle;
        public bool LoadContents { get; } = loadContents;
        public Func<AssetBundle, IEnumerator>? OnBundleLoaded { get; } = onBundleLoaded;

        public AssetBundleCreateRequest BundleRequest { get; } = AssetBundle.LoadFromFileAsync(path);
        
        public TimeSpan ElapsedTime => DateTime.Now - StartTime;
        public string FileName => System.IO.Path.GetFileNameWithoutExtension(Path);

        public enum State
        {
            LoadingBundle,
            LoadingContent
        }
    }
    
    #region Obsolete
    
    [Obsolete("Use LoadBundleAndContent instead")]
    public static void LoadBundle(string path, string relativePath) 
    {
        LoadBundleAndContent(path);
    }
    
    #endregion
}
