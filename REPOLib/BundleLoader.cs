using BepInEx;
using REPOLib.Objects.Sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REPOLib;

public static class BundleLoader
{
    private static readonly List<LoadOperation> _operations = [];
    
    public static void LoadAllBundles(string root, string withExtension)
    {
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

    internal static void FinishLoadOperations(MonoBehaviour routineRunner)
    {
        foreach (var loadOperation in _operations.ToArray()) // collection might change
        {
            routineRunner.StartCoroutine(FinishLoadOperation(loadOperation));
        }
    }

    private static IEnumerator FinishLoadOperation(LoadOperation operation)
    {
        while (!operation.CreateRequest.isDone)
        {
            yield return null;
        }

        var bundle = operation.CreateRequest.assetBundle;
        Mod[] mods = bundle.LoadAllAssets<Mod>();

        switch (mods.Length)
        {
            case 0:
                Logger.LogError($"Bundle at {operation.RelativePath} contains no mods.");
                _operations.Remove(operation);
                yield break;
            case > 1:
                Logger.LogError($"Bundle at {operation.RelativePath} contains more than one mod.");
                _operations.Remove(operation);
                yield break;
        }

        var mod = mods[0];
        Content[] contents = bundle.LoadAllAssets<Content>();

        foreach (var content in contents)
        {
            try
            {
                content.Initialize(mod);
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to load {content.Name} ({content.GetType().Name}) from {mod.Identifier} (at {operation.RelativePath}): {e}");
            }
        }

        double loadTime = (DateTime.Now - operation.StartTime).TotalSeconds;
        Logger.LogInfo($"Loaded bundled mod {mod.Identifier} (at {operation.RelativePath}) in {loadTime:N1}s");
        
        _operations.Remove(operation);
    }

    private class LoadOperation(DateTime startTime, AssetBundleCreateRequest request, string relativePath)
    {
        public string RelativePath { get; } = relativePath;
        public DateTime StartTime { get; } = startTime;
        public AssetBundleCreateRequest CreateRequest { get; } = request;
    }
}
