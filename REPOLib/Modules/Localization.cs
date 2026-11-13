using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Object = UnityEngine.Object;

namespace REPOLib.Modules;

/// <summary>
/// The Localization module of REPOLib.
/// </summary>
/// <remarks>
/// String tables in a mod's bundle are registered automatically. They're made available to the game's
/// localization system the same way the game's own tables are, so <see cref="LocalizedAsset"/>s and
/// <see cref="LocalizedString"/>s referencing them work like any vanilla one.
/// </remarks>
public static class Localization
{
    private const string LocatorId = "REPOLib.Localization";

    /// <summary>
    /// Gets all <see cref="StringTable"/>s registered with REPOLib.
    /// </summary>
    public static IReadOnlyList<StringTable> RegisteredStringTables => _stringTables;

    private static readonly List<StringTable> _stringTables = [];
    private static readonly HashSet<LocalizedAsset> _checkedLocalizedAssets = [];
    private static readonly LocalizationProvider _provider = new();

    private static ResourceLocationMap? _locator;
    private static bool _rebuildQueued;
    private static Dictionary<string, (Guid TableGuid, string TableName, long KeyId)>? _gameKeys;

    /// <summary>
    /// Registers a <see cref="StringTable"/> (one locale of a table collection).
    /// </summary>
    /// <param name="stringTable">The <see cref="StringTable"/> to register.</param>
    public static void RegisterStringTable(StringTable stringTable)
    {
        if (stringTable == null)
        {
            Logger.LogError("Failed to register string table. StringTable is null.");
            return;
        }

        if (stringTable.SharedData == null)
        {
            Logger.LogError($"Failed to register string table \"{stringTable.name}\". It has no SharedTableData.");
            return;
        }

        if (_stringTables.Contains(stringTable))
        {
            return;
        }

        string collectionName = stringTable.TableCollectionName;
        string address = GetTableAddress(collectionName, stringTable.LocaleIdentifier.Code);

        if (IsProvidedByGame(address) || IsProvidedByGame(GuidKey(stringTable.SharedData)))
        {
            Logger.LogError($"Failed to register string table \"{stringTable.name}\". A table collection named \"{collectionName}\" already exists in the game. Use a unique name for your table collection.");
            return;
        }

        if (_stringTables.Any(x => x.SharedData != stringTable.SharedData && x.TableCollectionName == collectionName))
        {
            Logger.LogError($"Failed to register string table \"{stringTable.name}\". A table collection named \"{collectionName}\" was already registered by another mod.");
            return;
        }

        _stringTables.Add(stringTable);
        QueueRebuild();

        Logger.LogDebug($"Registered string table \"{stringTable.name}\"", extended: true);
    }

    internal static IEnumerator RegisterBundleStringTables(AssetBundle bundle)
    {
        if (bundle.isStreamedSceneAssetBundle) yield break;

        var request = bundle.LoadAllAssetsAsync<StringTable>();
        yield return request;

        foreach (var stringTable in request.allAssets.OfType<StringTable>())
        {
            RegisterStringTable(stringTable);
        }
    }

    internal static IEnumerator FixLocalizedAssets()
    {
        var toCheck = Resources.FindObjectsOfTypeAll<LocalizedAsset>()
            .Where(x => x != null && !_checkedLocalizedAssets.Contains(x))
            .ToList();

        _checkedLocalizedAssets.UnionWith(toCheck);

        var invalid = toCheck.Where(x => !x.IsValid()).ToList();
        if (invalid.Count == 0) yield break;

        if (_gameKeys == null)
        {
            yield return LoadGameKeys();
        }

        foreach (var localizedAsset in invalid)
        {
            string key = GetEntryKey(localizedAsset.name);
            if (_gameKeys != null && _gameKeys.TryGetValue(key, out var entry))
            {
                localizedAsset.stringReference.SetReference(entry.TableGuid, entry.KeyId);
                Logger.LogDebug($"LocalizedAsset \"{localizedAsset.name}\" uses base-game entry \"{key}\" in table \"{entry.TableName}\"", extended: true);
            }
            else
            {
                Logger.LogWarning($"LocalizedAsset \"{localizedAsset.name}\" has no string reference and doesn't match any vanilla entry.");
            }
        }
    }

    private static string GetEntryKey(string localizedAssetName)
    {
        const string LegacyPrefix = "LocalizedAsset - ";

        return localizedAssetName.StartsWith(LegacyPrefix, StringComparison.Ordinal)
            ? localizedAssetName.Substring(LegacyPrefix.Length)
            : localizedAssetName;
    }

    private static IEnumerator LoadGameKeys()
    {
        var initialization = LocalizationSettings.InitializationOperation;
        if (!initialization.IsDone) yield return initialization;

        var locale = LocalizationSettings.ProjectLocale ?? LocalizationSettings.SelectedLocale;
        if (locale == null)
        {
            Logger.LogError("Failed to load vanilla localization keys. No locale is available.");
            yield break;
        }

        var tables = LocalizationSettings.StringDatabase.GetAllTables(locale);
        yield return tables;

        _gameKeys = [];

        if (tables.Result == null) yield break;

        foreach (var table in tables.Result)
        {
            if (table == null || table.SharedData == null || _stringTables.Contains(table)) continue;

            var tableGuid = table.SharedData.TableCollectionNameGuid;
            string tableName = table.SharedData.TableCollectionName;
            foreach (var entry in table.SharedData.Entries)
            {
                _gameKeys.TryAdd(entry.Key, (tableGuid, tableName, entry.Id));
            }
        }
    }

    private static void QueueRebuild()
    {
        var initialization = LocalizationSettings.InitializationOperation;

        if (initialization.IsDone)
        {
            RebuildLocator();
            return;
        }

        if (_rebuildQueued) return;
        _rebuildQueued = true;

        initialization.Completed += _ =>
        {
            _rebuildQueued = false;
            RebuildLocator();
        };
    }

    private static void RebuildLocator()
    {
        if (!Addressables.ResourceManager.ResourceProviders.Contains(_provider))
        {
            Addressables.ResourceManager.ResourceProviders.Add(_provider);
        }

        _provider.Clear();
        var locator = new ResourceLocationMap(LocatorId);

        string? projectLocaleCode = LocalizationSettings.ProjectLocale?.Identifier.Code;
        var localeCodes = LocalizationSettings.AvailableLocales?.Locales?
            .Where(x => x != null)
            .Select(x => x.Identifier.Code)
            .ToList() ?? [];

        foreach (var collection in _stringTables.Where(x => x != null).GroupBy(x => x.SharedData))
        {
            var sharedData = collection.Key;
            string collectionName = sharedData.TableCollectionName;

            locator.Add(GuidKey(sharedData), _provider.CreateLocation(sharedData, typeof(SharedTableData)));

            var tablesByCode = new Dictionary<string, StringTable>();
            foreach (var table in collection)
            {
                tablesByCode[table.LocaleIdentifier.Code] = table;
                locator.Add(GetTableAddress(collectionName, table.LocaleIdentifier.Code), _provider.CreateLocation(table, typeof(StringTable)));
            }

            var fallback = projectLocaleCode != null && tablesByCode.TryGetValue(projectLocaleCode, out var projectTable)
                ? projectTable
                : collection.First();

            foreach (string code in localeCodes.Where(x => !tablesByCode.ContainsKey(x)))
            {
                locator.Add(GetTableAddress(collectionName, code), _provider.CreateLocation(fallback, typeof(StringTable)));
            }
        }

        if (_locator != null)
        {
            Addressables.RemoveResourceLocator(_locator);
        }

        _locator = locator;
        Addressables.AddResourceLocator(_locator);
    }

    private static bool IsProvidedByGame(string key)
    {
        return Addressables.ResourceLocators
            .Where(x => x != _locator)
            .Any(x => x.Locate(key, null, out _));
    }

    private static string GuidKey(SharedTableData sharedData)
    {
        return sharedData.TableCollectionNameGuid.ToString("N");
    }

    private static string GetTableAddress(string collectionName, string localeCode)
    {
        return $"{collectionName}_{localeCode}";
    }

    private class LocalizationProvider : ResourceProviderBase
    {
        private readonly Dictionary<string, Object> _objects = [];

        public IResourceLocation CreateLocation(Object obj, Type type)
        {
            string internalId = $"{LocatorId}/{obj.GetInstanceID()}";
            _objects[internalId] = obj;
            return new ResourceLocationBase(obj.name, internalId, ProviderId, type);
        }

        public void Clear()
        {
            _objects.Clear();
        }

        public override void Provide(ProvideHandle provideHandle)
        {
            if (_objects.TryGetValue(provideHandle.Location.InternalId, out var obj) && obj != null)
            {
                provideHandle.Complete(obj, true, null);
                return;
            }

            provideHandle.Complete<Object?>(null, false, new Exception($"REPOLib has no localization asset for {provideHandle.Location.InternalId}"));
        }
    }
}
