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
        if (ConfigManager.DeveloperMode == null)
        {
            return;
        }

        if (SteamManager.instance == null)
        {
            return;
        }

        bool value = ConfigManager.DeveloperMode.Value;

        if (SteamManager.instance.developerMode != value)
        {
            if (value)
            {
                Logger.LogInfo("Enabling developer mode in SteamManager.");
            }
            else
            {
                Logger.LogInfo("Disabling developer mode in SteamManager.");
            }
        }

        SteamManager.instance.developerMode = value;
    }
}
