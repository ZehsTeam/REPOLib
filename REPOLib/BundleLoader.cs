using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using REPOLib.Extensions;
using REPOLib.Modules;
using REPOLib.Objects.Sdk;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REPOLib;

/// <summary>
///     Loads AssetBundles asynchronously.
/// </summary>
[PublicAPI]
public static class BundleLoader
{
    private static readonly List<LoadOperation> _operations = [];

    /// <summary>
    ///     An event that runs when all AssetBundles are loaded by REPOLib.
    /// </summary>
    public static event Action? OnAllBundlesLoaded;

    internal static void LoadAllBundles(string root, string withExtension)
    {
        Logger.LogInfo($"Loading all bundles with extension {withExtension} from root {root}", true);
        string[] files = Directory.GetFiles(root, "*" + withExtension, SearchOption.AllDirectories);
        files.ForEach(LoadBundleAndContent);
    }

    /// <summary>
    ///     Load an AssetBundle async and register its content. Use
    ///     <see cref="LoadBundle(string, Func{AssetBundle, IEnumerator}?, bool)" /> or<br />
    ///     <see cref="LoadBundle(string, Action{AssetBundle}, bool)" /> for a callback for when the AssetBundle is loaded.
    /// </summary>
    /// <param name="path">The absolute path to the AssetBundle.</param>
    public static void LoadBundleAndContent(string path) 
        => LoadBundle(path, null, true);

    /// <param name="onLoaded">Callback for when the AssetBundle is loaded.</param>
    /// <inheritdoc cref="LoadBundle(string, Func{AssetBundle, IEnumerator}?, bool)" />
    /// <param name="path"></param>
    /// <param name="loadContents"></param>
    public static void LoadBundle(string path, Action<AssetBundle> onLoaded, bool loadContents = false) 
        => LoadBundle(path, onLoaded.ToEnumerator(), loadContents);

    /// <summary>
    ///     Load an AssetBundle async and get a callback for when it's loaded. Optionally also register its contents
    ///     automatically.
    /// </summary>
    /// <param name="path">The absolute path to the AssetBundle.</param>
    /// <param name="onLoaded">Callback for when the AssetBundle is loaded. Supports waiting as an IEnumerator coroutine.</param>
    /// <param name="loadContents">Whether REPOLib should register the AssetBundle's contents automatically.</param>
    public static void LoadBundle(string path, Func<AssetBundle, IEnumerator>? onLoaded = null,
        bool loadContents = false)
    {
        Logger.LogInfo($"Loading bundle at {path}...");
        _operations.Add(new LoadOperation(path, onLoaded, loadContents));
    }

    internal static void FinishLoadOperations(MonoBehaviour behaviour) 
        => behaviour.StartCoroutine(FinishLoadOperationsRoutine(behaviour));

    private static IEnumerator FinishLoadOperationsRoutine(MonoBehaviour behaviour)
    {
        yield return null;

        foreach (LoadOperation? loadOperation in _operations.ToArray()) // collection might change
            behaviour.StartCoroutine(FinishLoadOperation(loadOperation));

        (TMP_Text text, Action disableLoadingUI) = SetupLoadingUI();
        float lastUpdate = Time.time;

        while (_operations.Count > 0)
        {
            if (Time.time - lastUpdate <= 1) 
                continue;

            lastUpdate = Time.time;
            string bundlesWord = _operations.Count == 1 ? "bundle" : "bundles";
            text.text = $"REPOLib: Waiting for {_operations.Count} {bundlesWord} to load...";

            if (ConfigManager.ExtendedLogging.Value != true) 
                continue;

            foreach (LoadOperation? operation in _operations)
            {
                string msg = $"Loading {operation.FileName}: {operation.CurrentState}";
                float? progress = operation.CurrentState switch
                { 
                    LoadOperation.State.LoadingBundle => operation.BundleRequest.progress,
                    _ => null 
                };

                if (progress.HasValue) 
                    msg += $" {progress.Value:P0}";
                Logger.LogDebug(msg);
            }


            yield return null;
        }

        Logger.LogInfo("Finished loading bundles.");

        disableLoadingUI();
        Utilities.SafeInvokeEvent(OnAllBundlesLoaded);
    }

    private static (TMP_Text, Action) SetupLoadingUI()
    {
        GameObject? hudCanvas = GameObject.Find("HUD Canvas");
        Transform? hud = hudCanvas.transform.Find("HUD");
        hud.gameObject.SetActive(false);

        TMP_Text? buttonText = Object.FindObjectOfType<TMP_Text>();
        TMP_Text? text = Object.Instantiate(buttonText, hudCanvas.transform);
        text.gameObject.name = "REPOLibText";
        text.gameObject.SetActive(true);
        text.text = "REPOLib is loading bundles... Hang tight!";
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform? rectTransform = text.GetComponent<RectTransform>();
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
        AssetBundle? bundle = operation.BundleRequest.assetBundle;

        if (bundle == null)
        {
            Logger.LogError($"Failed to load bundle {operation.FileName}!");
            Finish();
            yield break;
        }

        if (operation.LoadContents) 
            yield return LoadBundleContent(operation, bundle);

        if (operation.OnBundleLoaded != null) 
            yield return operation.OnBundleLoaded(bundle);

        if (ConfigManager.ExtendedLogging?.Value == true)
            Logger.LogInfo($"Loaded bundle {operation.FileName} in {operation.ElapsedTime.TotalSeconds:N1}s");

        Finish();
        yield break;

        void Finish() => _operations.Remove(operation);
    }

    private static IEnumerator LoadBundleContent(LoadOperation operation, AssetBundle bundle)
    {
        operation.CurrentState = LoadOperation.State.LoadingContent;
        AssetBundleRequest? assetRequest = bundle.LoadAllAssetsAsync<ScriptableObject>();
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

        Mod mod = mods[0];
        assets.OfType<Content>().ForEach(content =>
        {
            try
            {
                content.Initialize(mod);
            }
            catch (Exception e)
            {
                Logger.LogError(
                    $"Failed to load {content.Name} ({content.GetType().Name}) from bundle {content.name} ({mod.Identifier}): {e}");
            }
        });
    }

    #region Obsolete

    /// <summary>Deprecated.</summary>
    [Obsolete("Use LoadBundleAndContent instead")]
    public static void LoadBundle(string path, string relativePath) 
        => LoadBundleAndContent(path);

    #endregion

    [PublicAPI]
    private class LoadOperation(
        string path,
        Func<AssetBundle, IEnumerator>? onBundleLoaded = null,
        bool loadContents = true)
    {
        public enum State
        {
            LoadingBundle,
            LoadingContent
        }

        public string Path { get; } = path;
        public DateTime StartTime { get; } = DateTime.Now;
        public State CurrentState { get; set; } = State.LoadingBundle;
        public bool LoadContents { get; } = loadContents;
        public Func<AssetBundle, IEnumerator>? OnBundleLoaded { get; } = onBundleLoaded;

        public AssetBundleCreateRequest BundleRequest { get; } = AssetBundle.LoadFromFileAsync(path);

        public TimeSpan ElapsedTime => DateTime.Now - this.StartTime;
        public string FileName => System.IO.Path.GetFileNameWithoutExtension(this.Path);
    }
}