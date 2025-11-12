using HarmonyLib;
using REPOLib.Modules;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(AudioManager))]
internal static class AudioManagerPatch
{
    [HarmonyPatch(nameof(AudioManager.Start))]
    [HarmonyPostfix]
    private static void StartPatch()
    {
        Utilities.FixAudioMixerGroupsOnPrefabs();
        LevelAmbiences.RegisterLevelAmbiences();
    }
}
