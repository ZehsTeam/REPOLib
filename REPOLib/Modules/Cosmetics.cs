using REPOLib.Extensions;
using REPOLib.Objects;
using REPOLib.Objects.Sdk;
using System.Collections.Generic;
using System.Linq;
using REPOLib.Patches;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
/// The Cosmetics module of REPOLib.
/// </summary>
public static class Cosmetics
{
    /// <summary>
    /// Gets all cosmetics.
    /// </summary>
    /// <returns>The list of all cosmetics.</returns>
    public static IReadOnlyList<CosmeticAsset> AllCosmetics => MetaManager.instance?.GetCosmetics() ?? [];

    /// <summary>
    /// Gets all cosmetics registered with REPOLib.
    /// </summary>
    public static IReadOnlyList<CosmeticAsset> RegisteredCosmetics => _cosmeticsRegistered;

    private static readonly List<CosmeticAsset> _cosmeticsToRegister = [];
    private static readonly List<CosmeticAsset> _cosmeticsRegistered = [];

    private static bool _initialCosmeticsRegistered;

    // This will run once because of how the vanilla game registers cosmetics.
    internal static void RegisterCosmetics()
    {
        BundleLoader.OnAllBundlesLoaded -= RegisterCosmetics;

        Logger.LogInfo($"Adding cosmetics.");

        foreach (var cosmetic in _cosmeticsToRegister)
        {
            RegisterCosmeticWithGame(cosmetic);
        }

        MetaManagerPatch.LoadModded();

        _initialCosmeticsRegistered = true;
    }

    private static void RegisterCosmeticWithGame(CosmeticAsset item)
    {
        // Fix default color not being referenced properly
        if(item.defaultColor){
            item.defaultColor = MetaManager.instance.colors.FirstOrDefault(c => c.color == item.defaultColor.color);
        }

        #region UUID Check
        if(string.IsNullOrWhiteSpace(item.assetId) || item.assetId.StartsWith("vanilla:")){
            item.assetId = $"modded:{item.name}";
        }

        var duplicateAsset = AllCosmetics.FirstOrDefault(x => x != item && x.assetId == item.assetId);
        if(duplicateAsset != null){
            Logger.LogWarning($"Failed to add cosmetic \"{item.name}\" to MetaManager. Same assetId as \"{duplicateAsset.name}\". Asset ID: {item.assetId}", extended: true);
            return;
        }
        #endregion

        if (MetaManager.instance.AddCosmetic(item))
        {
            if (!_cosmeticsRegistered.Contains(item))
            {
                _cosmeticsRegistered.Add(item);
            }

            Logger.LogInfo($"Added cosmetic \"{item.name}\" to MetaManager.", extended: true);
        }
    }

    /// <summary>
    /// Registers an <see cref="CosmeticAsset"/>.
    /// </summary>
    /// <param name="cosmeticAsset">The cosmetic to register.</param>
    /// <returns>The registered cosmetic <see cref="PrefabRef"/> or null.</returns>
    public static PrefabRef? RegisterCosmetic(CosmeticAsset? cosmeticAsset)
    {
        if (cosmeticAsset == null)
        {
            Logger.LogError($"Failed to register cosmetic. CosmeticAsset is null.");
            return null;
        }

        if (cosmeticAsset.prefab == null)
        {
            Logger.LogError($"Failed to register cosmetic \"{cosmeticAsset.name}\". PrefabRef is null.");
            return null;
        }

        _cosmeticsToRegister.Add(cosmeticAsset);

        if (_initialCosmeticsRegistered)
        {
            RegisterCosmeticWithGame(cosmeticAsset);

            MetaManagerPatch.LoadModded();
        }

        return cosmeticAsset.prefab;
    }

    /// <summary>
    /// Registers an <see cref="CosmeticAsset"/>.
    /// </summary>
    /// <param name="cosmeticContent">The cosmetic to register.</param>
    /// <returns>The registered cosmetic <see cref="PrefabRef"/> or null.</returns>
    public static PrefabRef? RegisterCosmetic(CosmeticContent? cosmeticContent)
    {
        if (cosmeticContent == null)
        {
            Logger.LogError($"Failed to register cosmetic. CosmeticContent is null.");
            return null;
        }

        if (cosmeticContent.Asset == null)
        {
            Logger.LogError($"Failed to register cosmetic. CosmeticAsset is null.");
            return null;
        }

        PrefabRef? prefabRef = null;

        if (cosmeticContent.PrefabRef != null)
        {
            prefabRef = cosmeticContent.PrefabRef;
            if (prefabRef != null)
            {
                PrefabRefResolver.Resolve(prefabRef, cosmeticContent.Bundle);
            }
        }
        else if (cosmeticContent.Prefab != null)
        {
            GameObject prefab = cosmeticContent.Prefab;
            string prefabId = $"Cosmetics/{prefab.name}";
        
            PrefabRefResponse prefabRefResponse = NetworkPrefabs.RegisterNetworkPrefabInternal(prefabId, prefab);
            prefabRef = prefabRefResponse.PrefabRef;
        
            if (prefabRefResponse.Result == PrefabRefResult.PrefabAlreadyRegistered)
            {
                Logger.LogWarning($"Failed to register cosmetic \"{cosmeticContent.name}\". Cosmetic is already registered!");
                return null;
            }
        
            if (prefabRefResponse.Result == PrefabRefResult.DifferentPrefabAlreadyRegistered)
            {
                Logger.LogError($"Failed to register cosmetic \"{cosmeticContent.name}\". A cosmetic prefab is already registered with the same name.");
                return null;
            }
        
            if (prefabRefResponse.Result != PrefabRefResult.Success)
            {
                Logger.LogError($"Failed to register cosmetic \"{cosmeticContent.name}\". (Reason: {prefabRefResponse.Result})");
                return null;
            }
        }
        else
        {
            Logger.LogError($"Failed to register cosmetic. Prefab is null.");
            return null;
        }

        if (prefabRef == null)
        {
            Logger.LogError($"Failed to register cosmetic \"{cosmeticContent.name}\". PrefabRef is null.");
            return null;
        }

        cosmeticContent.Asset.prefab = prefabRef;

        if (!string.IsNullOrWhiteSpace(cosmeticContent.AssetId))
        {
            cosmeticContent.Asset.assetId = cosmeticContent.AssetId;
        }

        return RegisterCosmetic(cosmeticContent.Asset);
    }
}
