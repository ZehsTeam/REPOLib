using HarmonyLib;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(SteamManager))]
internal static class SteamManagerPatch
{
    [HarmonyPatch(nameof(SteamManager.Awake))]
    [HarmonyPostfix]
    public static void AwakePatch(SteamManager __instance)
    {
        UpdateDeveloperMode();
    }

    public static void UpdateDeveloperMode()
    {
        if (ConfigManager.VanillaDeveloperMode == null)
        {
            return;
        }

        if (SteamManager.instance == null)
        {
            return;
        }

        bool value = ConfigManager.VanillaDeveloperMode.Value;

        if (SteamManager.instance.developerMode != value)
        {
            if (value)
            {
                Logger.LogInfo("Enabling vanilla developer mode.");
            }
            else
            {
                Logger.LogInfo("Disabling vanilla developer mode.");
            }
        }

        SteamManager.instance.developerMode = value;
    }
}
