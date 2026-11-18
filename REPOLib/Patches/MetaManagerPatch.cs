using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using REPOLib.Modules;
using UnityEngine;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(MetaManager))]
internal static class MetaManagerPatch
{
    private static List<string> missingCosmeticUnlocks = [];
    private static List<string> missingCosmeticHistory = [];
    private static List<string> missingCosmeticEquipped = [];
    private static List<List<string>> missingCosmeticPresets = [];

    // Mirrors MetaManager.saveAllowed for the modded save file
    private static bool moddedSaveAllowed;

    private static string SavePathTemp(string _suffix = "") => Path.Combine(Application.persistentDataPath, $"{MetaManager.instance.savePath}{_suffix}.tmp");
    private static string SavePathFull(string _suffix = "") => Path.Combine(Application.persistentDataPath, $"{MetaManager.instance.savePath}{_suffix}.es3");
    private static string SavePathBackup(string _suffix = "") => Path.Combine(Application.persistentDataPath, $"{MetaManager.instance.savePath}{_suffix}.bak");

    [HarmonyPatch(nameof(MetaManager.Awake))]
    [HarmonyPrefix]
    private static void AwakePrefix(MetaManager __instance)
    {
        __instance.presetCacheFolder += "Modded";
    }

    [HarmonyPatch(nameof(MetaManager.Awake))]
    [HarmonyPostfix]
    private static void AwakePatch(MetaManager __instance)
    {
        if(__instance != MetaManager.instance) return;

        if(BundleLoader.AllBundlesLoaded){
            Cosmetics.RegisterCosmetics();
        }else{
            BundleLoader.OnAllBundlesLoaded += Cosmetics.RegisterCosmetics;
        }

        #region Clear icon cache for modded cosmetics
        string _cosmeticIconsPath = Path.Combine(Application.persistentDataPath, "Cache", "Icons", "Cosmetics");
        if(Directory.Exists(_cosmeticIconsPath)){
            int deletedCount = 0;
            int totalCount = 0;
            List<string> _cosmeticNames = new List<string>(__instance.cosmeticAssets.Where(x => x != null && !Cosmetics.RegisteredCosmetics.Contains(x)).Select(x => x.name.ToLowerInvariant()));
            Directory.GetFiles(_cosmeticIconsPath).ToList().ForEach(f => {
                if(!_cosmeticNames.Contains(Path.GetFileNameWithoutExtension(f).ToLowerInvariant())){
                    totalCount++;

                    try{
                        File.Delete(f);
                        deletedCount++;
                    }catch(System.Exception e){
                        Logger.LogWarning($"Could not delete file {f}: {e.Message}");
                    }
                }
            });
            if(totalCount > 0){
                Logger.LogInfo($"Deleted {deletedCount}/{totalCount} files for cosmetics icon cache");
            }
        }
        #endregion
    }

    [HarmonyPatch(nameof(MetaManager.PresetsInitialize))]
    [HarmonyPostfix]
    private static void PresetsInitializePatch(MetaManager __instance)
    {
        missingCosmeticPresets.Clear();
        for(var index = 0; index < __instance.cosmeticPresets.Count; index++){
            missingCosmeticPresets.Add(new List<string>());
        }
    }

    [HarmonyPatch(nameof(MetaManager.Reset))]
    [HarmonyPrefix]
    private static void ResetPatch()
    {
        missingCosmeticUnlocks.Clear();
        missingCosmeticHistory.Clear();
        missingCosmeticEquipped.Clear();
        missingCosmeticPresets.ForEach(x => x.Clear());

        // Vanilla deletes its save files before saving, do the same for the modded save
        SemiFunc.DeleteFileSafe(SavePathTemp("Modded"));
        SemiFunc.DeleteFileSafe(SavePathFull("Modded"));
        SemiFunc.DeleteFileSafe(SavePathBackup("Modded"));
    }

    [HarmonyPatch(nameof(MetaManager.CosmeticPresetSet))]
    [HarmonyPrefix]
    private static void CosmeticPresetSetPatch(int _index, List<int> _cosmeticEquipped, List<int> _colorsEquipped)
    {
        if(_index < 0 || _index >= missingCosmeticPresets.Count) return;
        missingCosmeticPresets[_index].Clear();
    }

    [HarmonyPatch(nameof(MetaManager.Save))]
    [HarmonyPrefix]
    private static bool SavePatch(MetaManager __instance)
    {
        if(!SemiFunc.IsMainEditor() || !__instance.saveAllowed) return false;

        bool IsValidCosmetic(int x) => x >= 0 && x < __instance.cosmeticAssets.Count && __instance.cosmeticAssets[x] != null;

        #region Vanilla
        try{
            bool IsValidVanillaCosmetic(int x) => IsValidCosmetic(x) && !Cosmetics.RegisteredCosmetics.Contains(__instance.cosmeticAssets[x]);

            string _pathTemp = SavePathTemp();
            string _pathFull = SavePathFull();
            string _pathBackup = SavePathBackup();

            // Start from the existing vanilla save so values we don't write here are preserved
            if(File.Exists(_pathFull)){
                File.Copy(_pathFull, _pathTemp, overwrite: true);
            }
            var _settings = __instance.MakeSettings(_pathTemp);

            // Values that are in-sync with vanilla save file
            ES3.Save("cosmeticTokens", __instance.cosmeticTokens, _settings);
            ES3.Save("cosmeticUnlocks", __instance.cosmeticUnlocks.Where(IsValidVanillaCosmetic).ToList(), _settings);
            ES3.Save("cosmeticHistory", __instance.cosmeticHistory.Where(IsValidVanillaCosmetic).ToList(), _settings);

            if(!__instance.VerifySaveFile(_pathTemp)){
                Logger.LogError($"[MetaSave] Failed to verify {Path.GetFileName(_pathTemp)}");
                SemiFunc.DeleteFileSafe(_pathTemp);
            }else{
                CommitSaveFile(_pathTemp, _pathFull, _pathBackup, "MetaSave");
            }

        }catch(System.Exception e){
            Logger.LogError($"[MetaSave] Save failed: {e}");
        }
        #endregion

        #region Modded
        if(!moddedSaveAllowed) return false;

        try{
            bool IsValidModdedCosmetic(int x) => IsValidCosmetic(x) && Cosmetics.RegisteredCosmetics.Contains(__instance.cosmeticAssets[x]);

            string _pathTemp = SavePathTemp("Modded");
            string _pathFull = SavePathFull("Modded");
            string _pathBackup = SavePathBackup("Modded");

            var _settings = __instance.MakeSettings(_pathTemp);

            ES3.Save("cosmeticUnlocks", __instance.cosmeticUnlocks.Where(IsValidModdedCosmetic).Select(x => __instance.cosmeticAssets[x].assetId)
                .Concat(missingCosmeticUnlocks).ToList(), _settings);
            ES3.Save("cosmeticHistory", __instance.cosmeticHistory.Where(IsValidModdedCosmetic).Select(x => __instance.cosmeticAssets[x].assetId)
                .Concat(missingCosmeticHistory).ToList(), _settings);

            ES3.Save("cosmeticEquipped", __instance.cosmeticEquipped.Where(IsValidCosmetic).Select(x => __instance.cosmeticAssets[x].assetId)
                .Concat(missingCosmeticEquipped).ToList(), _settings);

            ES3.Save("cosmeticPresets", __instance.cosmeticPresets.Select((preset, i) => preset.Where(IsValidCosmetic).Select(x => __instance.cosmeticAssets[x].assetId)
                .Concat(i < missingCosmeticPresets.Count ? missingCosmeticPresets[i] : new List<string>()).ToList()
            ).ToList(), _settings);

            ES3.Save("colorPresets", __instance.colorPresets, _settings);
            ES3.Save("colorsEquipped", __instance.colorsEquipped, _settings);

            if(!VerifyModdedSaveFile(_pathTemp)){
                Logger.LogError($"[MetaSaveModded] Failed to verify {Path.GetFileName(_pathTemp)}");
                SemiFunc.DeleteFileSafe(_pathTemp);
            }else{
                CommitSaveFile(_pathTemp, _pathFull, _pathBackup, "MetaSaveModded");
            }

        }catch(System.Exception e){
            Logger.LogError($"[MetaSaveModded] Save failed: {e}");
        }
        #endregion

        return false;
    }

    private static void CommitSaveFile(string _pathTemp, string _pathFull, string _pathBackup, string _logTag){
        SemiFunc.DeleteFileSafe(_pathBackup);
        try{
            if(File.Exists(_pathFull)) File.Move(_pathFull, _pathBackup);
        }catch(System.Exception e){
            Logger.LogWarning($"[{_logTag}] Could not back up {Path.GetFileName(_pathFull)} - saving without backup: {e.Message}");
        }

        if(File.Exists(_pathFull)){
            File.Copy(_pathTemp, _pathFull, true);
            SemiFunc.DeleteFileSafe(_pathTemp);
        }else{
            File.Move(_pathTemp, _pathFull);
        }
    }

    private static bool VerifyModdedSaveFile(string _path){
        try{
            var _settings = MetaManager.instance.MakeSettings(_path);
            return ES3.FileExists(_settings) && ES3.KeyExists("cosmeticPresets", _settings) && ES3.KeyExists("cosmeticUnlocks", _settings);
        }catch{
            return false;
        }
    }

    [HarmonyPatch(nameof(MetaManager.Load))]
    [HarmonyPrefix]
    private static void LoadPrefix()
    {
        moddedSaveAllowed = false;
    }

    [HarmonyPatch(nameof(MetaManager.Load))]
    [HarmonyPostfix]
    private static void LoadPatch(MetaManager __instance)
    {
        LoadModded();
    }

    private static bool TryLoadModded(string _path){

        if(!File.Exists(_path)) return false;

        try{
            bool IsValidCosmetic(string x) => MetaManager.instance.cosmeticAssets.FirstOrDefault(a => a && a.assetId == x);
            int CosmeticIndex(string x) => MetaManager.instance.cosmeticAssets.FindIndex(a => a && a.assetId == x);

            var _saveFile = MetaManager.instance.MakeSettings(_path);

            if(!ES3.FileExists(_saveFile)) return false;

            #region Combine
            if(ES3.KeyExists("cosmeticUnlocks", _saveFile)){
                List<string> _cosmeticUnlocks = ES3.Load<List<string>>("cosmeticUnlocks", _saveFile);
                MetaManager.instance.cosmeticUnlocks = MetaManager.instance.cosmeticUnlocks
                    .Concat(_cosmeticUnlocks.Where(IsValidCosmetic).Select(CosmeticIndex))
                    .Distinct().ToList();
                missingCosmeticUnlocks = _cosmeticUnlocks.Where(x => !IsValidCosmetic(x)).ToList();
            }
            if(ES3.KeyExists("cosmeticHistory", _saveFile)){
                List<string> _cosmeticHistory = ES3.Load<List<string>>("cosmeticHistory", _saveFile);
                MetaManager.instance.cosmeticHistory = MetaManager.instance.cosmeticHistory
                    .Concat(_cosmeticHistory.Where(IsValidCosmetic).Select(CosmeticIndex))
                    .Distinct().ToList();
                missingCosmeticHistory = _cosmeticHistory.Where(x => !IsValidCosmetic(x)).ToList();
            }
            #endregion

            #region Overwrite
            if(ES3.KeyExists("cosmeticEquipped", _saveFile)){
                List<string> _cosmeticEquipped = ES3.Load<List<string>>("cosmeticEquipped", _saveFile);
                MetaManager.instance.cosmeticEquipped = _cosmeticEquipped.Where(IsValidCosmetic).Select(CosmeticIndex).ToList();
                missingCosmeticEquipped = _cosmeticEquipped.Where(x => !IsValidCosmetic(x)).ToList();
            }

            if(ES3.KeyExists("cosmeticPresets", _saveFile)){
                var _cosmeticPresets = ES3.Load<List<List<string>>>("cosmeticPresets", _saveFile);
                if(_cosmeticPresets != null){
                    int _minLength = Mathf.Min(MetaManager.instance.cosmeticPresets.Count, _cosmeticPresets.Count, missingCosmeticPresets.Count);
                    for(int i = 0; i < _minLength; i++){
                        MetaManager.instance.cosmeticPresets[i] = _cosmeticPresets[i].Where(IsValidCosmetic).Select(CosmeticIndex).ToList();
                        missingCosmeticPresets[i] = _cosmeticPresets[i].Where(x => !IsValidCosmetic(x)).ToList();
                    }
                }
            }

            if(ES3.KeyExists("colorPresets", _saveFile)){
                var _colorPresets = ES3.Load<List<List<int>>>("colorPresets", _saveFile);
                if(_colorPresets != null){
                    int _minLength = Mathf.Min(MetaManager.instance.colorPresets.Count, _colorPresets.Count);
                    for(int i = 0; i < _minLength; i++){
                        MetaManager.instance.colorPresets[i] = _colorPresets[i];
                    }
                }
            }

            if(ES3.KeyExists("colorsEquipped", _saveFile)){
                var _colorsEquipped = ES3.Load<int[]>("colorsEquipped", _saveFile);
                if(_colorsEquipped != null){
                    int _minLength = Mathf.Min(MetaManager.instance.colorsEquipped.Length, _colorsEquipped.Length);
                    for(int i = 0; i < _minLength; i++){
                        MetaManager.instance.colorsEquipped[i] = _colorsEquipped[i];
                    }
                }
            }
            #endregion

            return true;
        }catch(System.Exception e){
            Logger.LogError($"[MetaSaveModded] Failed to load {Path.GetFileName(_path)}: {e}");
            return false;
        }
    }

    public static void LoadModded()
    {
        if(!MetaManager.instance) return;

        string _pathTemp = SavePathTemp("Modded");
        string _pathFull = SavePathFull("Modded");
        string _pathBackup = SavePathBackup("Modded");

        SemiFunc.DeleteFileSafe(_pathTemp);

        if(TryLoadModded(_pathFull)){
            Logger.LogInfo($"[MetaSaveModded] {Path.GetFileName(_pathFull)} found");
            moddedSaveAllowed = true;
            return;
        }

        if(TryLoadModded(_pathBackup)){
            Logger.LogWarning($"[MetaSaveModded] {Path.GetFileName(_pathFull)} restored from {Path.GetFileName(_pathBackup)}");
            MetaManager.instance.PreserveSaveFile(_pathFull);
            SemiFunc.DeleteFileSafe(_pathFull);
            moddedSaveAllowed = true;
            MetaManager.instance.Save();
            return;
        }

        if(File.Exists(_pathFull) || File.Exists(_pathBackup)){
            MetaManager.instance.PreserveSaveFile(_pathFull);
            Logger.LogError($"[MetaSaveModded] {Path.GetFileName(_pathFull)} exists but could not be loaded - modded saving disabled this session");
            moddedSaveAllowed = false;
            return;
        }

        Logger.LogWarning($"[MetaSaveModded] {Path.GetFileName(_pathFull)} not found - creating new");
        moddedSaveAllowed = true;
        MetaManager.instance.Save();
    }
}
