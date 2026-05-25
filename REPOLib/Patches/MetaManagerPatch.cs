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
        bool IsValidCosmetic(int x) => x >= 0 && x < MetaManager.instance.cosmeticAssets.Count && MetaManager.instance.cosmeticAssets[x] != null;

        #region Vanilla
        try{
            bool IsValidVanillaCosmetic(int x) => IsValidCosmetic(x) && !Cosmetics.RegisteredCosmetics.Contains(MetaManager.instance.cosmeticAssets[x]);

            string SavePathTemp = Path.Combine(Application.persistentDataPath, $"{MetaManager.instance.savePath}.tmp"); 
            string SavePathFull = Path.Combine(Application.persistentDataPath, $"{MetaManager.instance.savePath}.es3");
            string SavePathBackup = Path.Combine(Application.persistentDataPath, $"{MetaManager.instance.savePath}.bak");

            if(File.Exists(SavePathFull)){
                File.Copy(SavePathFull, SavePathTemp, overwrite: true);
            }
            var _settings = MetaManager.instance.MakeSettings(SavePathTemp);
 
            // Values that are in-sync with vanilla save file
            ES3.Save("cosmeticTokens", MetaManager.instance.cosmeticTokens, _settings);
            ES3.Save("cosmeticUnlocks", MetaManager.instance.cosmeticUnlocks.Where(IsValidVanillaCosmetic).ToList(), _settings);
            ES3.Save("cosmeticHistory", MetaManager.instance.cosmeticHistory.Where(IsValidVanillaCosmetic).ToList(), _settings);
 
            if(!MetaManager.instance.VerifySaveFile(SavePathTemp)){
                Logger.LogError($"[MetaSaveModded] Failed to verify {Path.GetFileName(SavePathTemp)}");
                MetaManager.instance.DeleteSaveFile(SavePathTemp);
            }else{
                MetaManager.instance.DeleteSaveFile(SavePathBackup);
                if(File.Exists(SavePathFull)){
                    File.Move(SavePathFull, SavePathBackup);
                }

                File.Move(SavePathTemp, SavePathFull);
            }
 
        }catch(System.Exception e){
            Logger.LogError($"[MetaSaveModded] Save failed: {e}");
        }
        #endregion

        #region Modded
        try{
            bool IsValidModdedCosmetic(int x) => IsValidCosmetic(x) && Cosmetics.RegisteredCosmetics.Contains(MetaManager.instance.cosmeticAssets[x]);

            string SavePathTemp = Path.Combine(Application.persistentDataPath, $"{MetaManager.instance.savePath}Modded.tmp"); 
            string SavePathFull = Path.Combine(Application.persistentDataPath, $"{MetaManager.instance.savePath}Modded.es3");
            string SavePathBackup = Path.Combine(Application.persistentDataPath, $"{MetaManager.instance.savePath}Modded.bak");

            var _settings = MetaManager.instance.MakeSettings(SavePathTemp);
 
            ES3.Save("cosmeticUnlocks", MetaManager.instance.cosmeticUnlocks.Where(IsValidModdedCosmetic).Select(x => MetaManager.instance.cosmeticAssets[x].assetId)
                .Concat(missingCosmeticUnlocks).ToList(), _settings);
            ES3.Save("cosmeticHistory", MetaManager.instance.cosmeticHistory.Where(IsValidModdedCosmetic).Select(x => MetaManager.instance.cosmeticAssets[x].assetId)
                .Concat(missingCosmeticHistory).ToList(), _settings);

            ES3.Save("cosmeticEquipped", MetaManager.instance.cosmeticEquipped.Where(IsValidCosmetic).Select(x => MetaManager.instance.cosmeticAssets[x].assetId)
                .Concat(missingCosmeticEquipped).ToList(), _settings);

            ES3.Save("cosmeticPresets", MetaManager.instance.cosmeticPresets.Select((preset, i) => preset.Where(IsValidCosmetic).Select(x => MetaManager.instance.cosmeticAssets[x].assetId)
                .Concat(missingCosmeticPresets[i]).ToList()
            ).ToList(), _settings);

            ES3.Save("colorPresets", MetaManager.instance.colorPresets, _settings);
            ES3.Save("colorsEquipped", MetaManager.instance.colorsEquipped, _settings);
 
            if(!VerifySaveFile(SavePathTemp)){
                Logger.LogError($"[MetaSaveModded] Failed to verify {Path.GetFileName(SavePathTemp)}");
                MetaManager.instance.DeleteSaveFile(SavePathTemp);
            }else{
                MetaManager.instance.DeleteSaveFile(SavePathBackup);
                if(File.Exists(SavePathFull)){
                    File.Move(SavePathFull, SavePathBackup);
                }

                File.Move(SavePathTemp, SavePathFull);
            }
 
        }catch(System.Exception e){
            Logger.LogError($"[MetaSaveModded] Save failed: {e}");
        }
        #endregion

        return false;
    }

    private static bool VerifySaveFile(string _path){
        try{
            var _settings = MetaManager.instance.MakeSettings(_path);
            return ES3.FileExists(_settings) && ES3.KeyExists("cosmeticPresets", _settings) && ES3.KeyExists("cosmeticUnlocks", _settings);
        }catch{
            return false;
        }
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
            bool IsValidCosmetic(string x) => MetaManager.instance.cosmeticAssets.FirstOrDefault(a => a.assetId == x);

            var _saveFile = MetaManager.instance.MakeSettings(_path);

            if(!ES3.FileExists(_saveFile)) return false;

            #region Combine
            if(ES3.KeyExists("cosmeticUnlocks", _saveFile)){
                List<string> _cosmeticUnlocks = ES3.Load<List<string>>("cosmeticUnlocks", _saveFile);
                MetaManager.instance.cosmeticUnlocks = MetaManager.instance.cosmeticUnlocks
                    .Concat(_cosmeticUnlocks.Where(IsValidCosmetic).Select(e => MetaManager.instance.cosmeticAssets.FindIndex(a => a.assetId == e)))
                    .Distinct().ToList();
                missingCosmeticUnlocks = _cosmeticUnlocks.Where(x => !IsValidCosmetic(x)).ToList();
            }
            if(ES3.KeyExists("cosmeticHistory", _saveFile)){
                List<string> _cosmeticHistory = ES3.Load<List<string>>("cosmeticHistory", _saveFile);
                MetaManager.instance.cosmeticHistory = MetaManager.instance.cosmeticHistory
                    .Concat(_cosmeticHistory.Where(IsValidCosmetic).Select(e => MetaManager.instance.cosmeticAssets.FindIndex(a => a.assetId == e)))
                    .Distinct().ToList();
                missingCosmeticHistory = _cosmeticHistory.Where(x => !IsValidCosmetic(x)).ToList();
            }
            #endregion

            #region Overwrite
            if(ES3.KeyExists("cosmeticEquipped", _saveFile)){
                List<string> _cosmeticEquipped = ES3.Load<List<string>>("cosmeticEquipped", _saveFile);
                MetaManager.instance.cosmeticEquipped = _cosmeticEquipped.Where(IsValidCosmetic).Select(e => MetaManager.instance.cosmeticAssets.FindIndex(a => a.assetId == e)).ToList();
                missingCosmeticEquipped = _cosmeticEquipped.Where(x => !IsValidCosmetic(x)).ToList();
            }

            if(ES3.KeyExists("cosmeticPresets", _saveFile)){
                var _cosmeticPresets = ES3.Load<List<List<string>>>("cosmeticPresets", _saveFile);
                if(_cosmeticPresets != null){
                    int _minLength = Mathf.Min(MetaManager.instance.cosmeticPresets.Count, _cosmeticPresets.Count);
                    for(int i = 0; i < _minLength; i++){
                        MetaManager.instance.cosmeticPresets[i] = _cosmeticPresets[i].Where(IsValidCosmetic).Select(e => MetaManager.instance.cosmeticAssets.FindIndex(a => a.assetId == e)).ToList();
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
            Logger.LogError($"[MetaSave] Failed to load {Path.GetFileName(_path)}: {e}");
            return false;
        }
    }
    
    public static void LoadModded()
    {
        if(!MetaManager.instance) return;

        string SavePathTemp = Path.Combine(Application.persistentDataPath, $"{MetaManager.instance.savePath}Modded.tmp"); 
        string SavePathFull = Path.Combine(Application.persistentDataPath, $"{MetaManager.instance.savePath}Modded.es3");
        string SavePathBackup = Path.Combine(Application.persistentDataPath, $"{MetaManager.instance.savePath}Modded.bak");

        MetaManager.instance.DeleteSaveFile(SavePathTemp);

        if(TryLoadModded(SavePathFull)){
            Logger.LogInfo($"[MetaSaveModded] {Path.GetFileName(SavePathFull)} found");
            return;
        }
 
        if(TryLoadModded(SavePathBackup)){
            Logger.LogWarning($"[MetaSaveModded] {Path.GetFileName(SavePathFull)} restored from {Path.GetFileName(SavePathBackup)}");
            MetaManager.instance.DeleteSaveFile(SavePathFull);
            MetaManager.instance.Save();
            return;
        }
 
        Logger.LogWarning($"[MetaSaveModded] {Path.GetFileName(SavePathFull)} not found - creating new");
        MetaManager.instance.Save();
    }
}