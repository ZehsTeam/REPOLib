using HarmonyLib;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(SteamManager))]
internal static class SteamManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(SteamManager.Awake))]
    public static void AwakePatch(SteamManager __instance)
        => UpdateDeveloperMode(__instance);

    public static void UpdateDeveloperMode(SteamManager instance)
    {
        if (instance == null)
            return;

        bool value = ConfigManager.VanillaDeveloperMode.Value;
        if (instance.developerMode != value)
            Logger.LogInfo(value ? "Enabling vanilla developer mode." : "Disabling vanilla developer mode.");

        instance.developerMode = value;
    }
}