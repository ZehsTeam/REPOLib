using HarmonyLib;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(SteamManager))]
public static class SteamManagerPatch
{
    [HarmonyPatch(nameof(SteamManager.Awake))]
    [HarmonyPostfix]
    public static void AwakePatch(SteamManager __instance)
    {
        Logger.LogInfo("Enabling developer mode in SteamManager");
        __instance.developerMode = true;
    }
}
