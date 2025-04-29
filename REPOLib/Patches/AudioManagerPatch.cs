using HarmonyLib;
using REPOLib.Modules;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(AudioManager))]
internal static class AudioManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(AudioManager.Start))]
    private static void StartPatch()
        => Utilities.FixAudioMixerGroupsOnPrefabs();
}