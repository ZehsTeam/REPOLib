using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REPOLib.Patches;

[HarmonyPatch]
internal static class FontAssetMaterialPatch
{
    private static readonly HashSet<Object> _legacyAssets = [];
    private static readonly Dictionary<string, Object?> _gameAssetCache = [];
    private static readonly HashSet<TMP_FontAsset> _checkedFontAssets = [];
    private static readonly int _mainTexId = Shader.PropertyToID("_MainTex");

    private static readonly AccessTools.FieldRef<TMP_Text, TMP_FontAsset> _fontAssetRef = AccessTools.FieldRefAccess<TMP_Text, TMP_FontAsset>("m_fontAsset");
    private static readonly AccessTools.FieldRef<TMP_Text, Material> _sharedMaterialRef = AccessTools.FieldRefAccess<TMP_Text, Material>("m_sharedMaterial");

    private static readonly Regex _duplicateSuffixRegex = new(@"\s+(\d+|\(\d+\))$");

    internal static HashSet<Object>? BeginLegacyBundle(string path, AssetBundle bundle)
    {
        if (bundle.isStreamedSceneAssetBundle) return null;

        string? unityVersion = ReadBundleUnityVersion(path);

        if (ConfigManager.ExtendedLogging.Value)
        {
            Logger.LogInfo($"Bundle \"{Path.GetFileName(path)}\" was built with Unity {unityVersion ?? "unknown"}.");
        }

        if (unityVersion == null || !IsLegacyUnityVersion(unityVersion)) return null;

        return [.. GetLoadedTextAssets()];
    }

    internal static void EndLegacyBundle(string path, AssetBundle bundle, HashSet<Object> snapshot)
    {
        var bundleAssets = GetLoadedTextAssets()
            .Concat(bundle.LoadAllAssets<TMP_FontAsset>())
            .Concat(bundle.LoadAllAssets<TMP_SpriteAsset>())
            .Where(asset => !snapshot.Contains(asset))
            .ToList();

        _legacyAssets.UnionWith(bundleAssets);

        Logger.LogInfo($"Bundle \"{Path.GetFileName(path)}\" was built with Unity {ReadBundleUnityVersion(path)}, replacing its TMP assets with the game's: {(bundleAssets.Count > 0 ? string.Join(", ", bundleAssets.Select(asset => $"\"{asset.name}\"").Distinct()) : "none found")}");
    }

    private static IEnumerable<Object> GetLoadedTextAssets()
    {
        return Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
            .Concat<Object>(Resources.FindObjectsOfTypeAll<TMP_SpriteAsset>());
    }

    [HarmonyPatch(typeof(TextMeshPro), "LoadFontAsset")]
    [HarmonyPatch(typeof(TextMeshProUGUI), "LoadFontAsset")]
    [HarmonyPrefix]
    private static void LoadFontAssetPrefix(TMP_Text __instance)
    {
        ReplaceLegacyAssets(__instance);
        RestoreMaterial(__instance.font);
    }

    private static void ReplaceLegacyAssets(TMP_Text text)
    {
        if (_legacyAssets.Count == 0) return;

        var fontAsset = text.font;
        if (fontAsset != null && _legacyAssets.Contains(fontAsset))
        {
            var gameFontAsset = GetGameAsset<TMP_FontAsset>(fontAsset.name);
            if (gameFontAsset != null)
            {
                _fontAssetRef(text) = gameFontAsset;
                _sharedMaterialRef(text) = gameFontAsset.material;
            }
        }

        var spriteAsset = text.spriteAsset;
        if (spriteAsset != null && _legacyAssets.Contains(spriteAsset))
        {
            text.spriteAsset = GetGameAsset<TMP_SpriteAsset>(spriteAsset.name);
        }
    }

    private static T? GetGameAsset<T>(string name) where T : Object
    {
        string normalizedName = NormalizeName(name);
        string cacheKey = $"{typeof(T).Name}:{normalizedName}";

        if (_gameAssetCache.TryGetValue(cacheKey, out var cached)) return cached as T;

        var asset = FindGameAsset(Resources.FindObjectsOfTypeAll<T>(), normalizedName)
            ?? FindGameAsset(Resources.LoadAll<T>(string.Empty), normalizedName);

        if (asset != null)
        {
            Logger.LogInfo($"Replaced legacy {typeof(T).Name} \"{name}\" with \"{asset.name}\".");
        }
        else
        {
            Logger.LogWarning($"Could not find a game {typeof(T).Name} matching legacy \"{name}\".");
        }

        _gameAssetCache[cacheKey] = asset;
        return asset;
    }

    private static T? FindGameAsset<T>(T[] assets, string normalizedName) where T : Object
    {
        return assets
            .Where(asset => asset != null && !_legacyAssets.Contains(asset))
            .FirstOrDefault(asset => NormalizeName(asset.name) == normalizedName);
    }

    private static string NormalizeName(string name)
    {
        return _duplicateSuffixRegex.Replace(name, string.Empty);
    }

    private static bool IsLegacyUnityVersion(string unityVersion)
    {
        string major = unityVersion.Split('.')[0];
        return int.TryParse(major, out int majorVersion) && majorVersion < 6000;
    }

    private static string? ReadBundleUnityVersion(string path)
    {
        try
        {
            using var reader = new BinaryReader(File.OpenRead(path));

            string signature = ReadNullTerminatedString(reader);
            if (!signature.StartsWith("Unity")) return null;

            reader.ReadUInt32();
            ReadNullTerminatedString(reader);
            return ReadNullTerminatedString(reader);
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Failed to read Unity version of bundle \"{path}\": {ex.Message}");
            return null;
        }
    }

    private static string ReadNullTerminatedString(BinaryReader reader)
    {
        var builder = new StringBuilder();

        for (int i = 0; i < 64; i++)
        {
            byte value = reader.ReadByte();
            if (value == 0) break;

            builder.Append((char)value);
        }

        return builder.ToString();
    }

    private static void RestoreMaterial(TMP_FontAsset? fontAsset)
    {
        if (fontAsset == null || !_checkedFontAssets.Add(fontAsset)) return;

        if (fontAsset.material == null)
        {
            var material = FindMaterial(fontAsset);
            if (material != null)
            {
                fontAsset.material = material;
                Logger.LogInfo($"Restored missing material \"{material.name}\" on font asset \"{fontAsset.name}\".");
            }
            else
            {
                Logger.LogWarning($"Font asset \"{fontAsset.name}\" has no material and no matching material could be found.");
            }
        }

        if (fontAsset.fallbackFontAssetTable == null) return;

        foreach (var fallbackFontAsset in fontAsset.fallbackFontAssetTable)
        {
            RestoreMaterial(fallbackFontAsset);
        }
    }

    private static Material? FindMaterial(TMP_FontAsset fontAsset)
    {
        var material = FindMaterial(fontAsset, Resources.FindObjectsOfTypeAll<Material>());
        if (material != null) return material;

        foreach (var bundle in AssetBundle.GetAllLoadedAssetBundles())
        {
            if (bundle.isStreamedSceneAssetBundle) continue;

            material = FindMaterial(fontAsset, bundle.LoadAllAssets<Material>());
            if (material != null) return material;
        }

        return null;
    }

    private static Material? FindMaterial(TMP_FontAsset fontAsset, Material[] materials)
    {
        var atlasTexture = fontAsset.atlasTextures is { Length: > 0 } ? fontAsset.atlasTexture : null;
        string expectedName = $"{fontAsset.name} Material";

        Material? atlasMatch = null;
        Material? nameMatch = null;

        foreach (var material in materials)
        {
            if (material == null) continue;

            bool hasName = material.name == expectedName;
            bool usesAtlas = atlasTexture != null
                && material.HasProperty(_mainTexId)
                && material.GetTexture(_mainTexId) == atlasTexture;

            if (hasName && usesAtlas) return material;
            if (usesAtlas && atlasMatch == null) atlasMatch = material;
            if (hasName && nameMatch == null) nameMatch = material;
        }

        return atlasMatch != null ? atlasMatch : nameMatch;
    }
}
