using HarmonyLib;
using Platforms;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(PlatformManager))]
internal static class PlatformManagerPatch
{
    [HarmonyPatch(nameof(PlatformManager.Awake))]
    [HarmonyPostfix]
    public static void AwakePatch(PlatformManager __instance)
    {
        if(__instance != PlatformManager.instance) return;
        if(ConfigManager.VanillaDeveloperMode == null || !ConfigManager.VanillaDeveloperMode.Value) return;

        UpdateDeveloperMode();
    }

    public static void UpdateDeveloperMode()
    {
        if (ConfigManager.VanillaDeveloperMode == null)
        {
            return;
        }

        if (PlatformManager.instance == null)
        {
            return;
        }

        bool value = ConfigManager.VanillaDeveloperMode.Value;

        if (PlatformManager.instance.developerFlags.debug_console != value)
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

        PlatformManager.instance.developerFlags.debug_console = value;
    }
}
