using REPOLib.Extensions;
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

/// <summary>
/// Loads AssetBundles asynchronously.
/// </summary>
public static class BundleLoader
{
    /// <summary>
    /// An event that runs when all AssetBundles are loaded by REPOLib.
    /// </summary>
    public static event Action? OnAllBundlesLoaded;

    private static readonly List<LoadOperation> _operations = [];

    internal static bool AllBundlesLoaded { get; private set; }

    internal static void LoadAllBundles(string root, string withExtension)
    {
        Logger.LogInfo($"Loading all bundles with extension {withExtension} from root {root}", extended: true);

        string[] files = Directory.GetFiles(root, "*" + withExtension, SearchOption.AllDirectories);

        foreach (string path in files)
        {
            LoadBundleAndContent(path);
        }
    }

    /// <summary>
    /// Load an AssetBundle async and register its content. Use <see cref="LoadBundle(string, Func{AssetBundle, IEnumerator}?, bool)"/> or<br/>
    /// <see cref="LoadBundle(string, Action{AssetBundle}, bool)"/> for a callback for when the AssetBundle is loaded.
    /// </summary>
    /// <param name="path">The absolute path to the AssetBundle.</param>
    public static void LoadBundleAndContent(string path)
    {
        LoadBundle(path, onLoaded: null, loadContents: true);
    }

    /// <param name="onLoaded">Callback for when the AssetBundle is loaded.</param>
    /// <inheritdoc cref="LoadBundle(string, Func{AssetBundle, IEnumerator}?, bool)"/>
    /// <param name="path"></param>
    /// <param name="loadContents"></param>
    public static void LoadBundle(string path, Action<AssetBundle> onLoaded, bool loadContents = false)
    {
        LoadBundle(path, OnLoaded, loadContents);
        return;

        IEnumerator OnLoaded(AssetBundle bundle)
        {
            try
            {
                onLoaded(bundle);
            }
            catch (Exception ex)
            {
                Logger.LogError($"BundleLoader failed to invoke onLoaded method for AssetBundle at \"{path}\" {ex}");
            }
            
            yield break;
        }
    }

    /// <summary>
    /// Load an AssetBundle async and get a callback for when it's loaded. Optionally also register its contents automatically.
    /// </summary>
    /// <param name="path">The absolute path to the AssetBundle.</param>
    /// <param name="onLoaded">Callback for when the AssetBundle is loaded. Supports waiting as an IEnumerator coroutine.</param>
    /// <param name="loadContents">Whether or not REPOLib should register the AssetBundle's contents automatically.</param>
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

        AllBundlesLoaded = false;

        foreach (var loadOperation in _operations.ToArray()) // collection might change
        {
            behaviour.StartCoroutine(FinishLoadOperation(loadOperation));
        }

        BundleLoaderLoadingText.Create();

        float lastUpdate = Time.time;
        while (_operations.Count > 0)
        {
            if (Time.time - lastUpdate > 1)
            {
                lastUpdate = Time.time;

                string bundlesWord = _operations.Count == 1 ? "bundle" : "bundles";

                BundleLoaderLoadingText.SetText($"REPOLib: Waiting for {_operations.Count} {bundlesWord} to load...");

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

                    Logger.LogDebug(msg);
                }
            }

            yield return null;
        }

        Logger.LogInfo("Finished loading bundles.");

        BundleLoaderLoadingText.Hide();

        AllBundlesLoaded = true;

        Utilities.SafeInvokeEvent(OnAllBundlesLoaded);
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

        foreach (var content in assets.OfType<Content>().OrderByTypeFirst<Content, LevelContent>())
        {
            try
            {
                content.Initialize(mod);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to load {content.Name} ({content.GetType().Name}) from bundle {operation.FileName} ({mod.Identifier}): {ex}");
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

    /// <summary>Deprecated.</summary>
    [Obsolete("Use LoadBundleAndContent instead")]
    public static void LoadBundle(string path, string relativePath)
    {
        LoadBundleAndContent(path);
    }

    #endregion
}

internal static class BundleLoaderLoadingText
{
    private static TMP_Text? _text;

    public static void Create()
    {
        var hudCanvas = GameObject.Find("HUD Canvas");

        var buttonText = Object.FindObjectOfType<TMP_Text>();
        _text = Object.Instantiate(buttonText, hudCanvas.transform);
        _text.gameObject.name = "REPOLibText";
        _text.color = Color.white;
        _text.fontStyle = FontStyles.Bold;
        _text.alignment = TextAlignmentOptions.Center;

        var rectTransform = _text.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        // Hide the text now so we can show it later after the vanilla splash screens
        _text.gameObject.SetActive(false);

        SetText("REPOLib is loading bundles... Hang tight!");
    }
    
    public static void Show()
    {
        VideoOverlay.Instance.Override(999f, 0.02f, 2f);
        _text?.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        VideoOverlay.Instance.Override(0f, 0.02f, 2f);
        _text?.gameObject.SetActive(false);
    }

    public static void SetText(string value)
    {
        if (_text == null) return;

        _text.text = value;
    }
}
