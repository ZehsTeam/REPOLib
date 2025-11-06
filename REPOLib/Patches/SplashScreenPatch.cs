using HarmonyLib;
using UnityEngine;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(SplashScreen))]
internal static class SplashScreenPatch
{
    private static bool _showedBundleLoaderLoadingText;

    [HarmonyPatch(nameof(SplashScreen.StateWarning))]
    [HarmonyPostfix]
    private static void StateWarningPatch(SplashScreen __instance)
    {
        if (__instance.state == SplashScreen.State.Done)
        {
            SplashScreenUI.instance.warningTransform.gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(nameof(SplashScreen.StateDone))]
    [HarmonyPrefix]
    private static bool StateDonePatch()
    {
        if (BundleLoader.AllBundlesLoaded)
        {
            return true;
        }

        if (_showedBundleLoaderLoadingText)
        {
            return false;
        }

        _showedBundleLoaderLoadingText = true;

        int count = SplashScreenUI.instance.transform.childCount;

        for (int i = 1; i < count; i++)
        {
            Transform child = SplashScreenUI.instance.transform.GetChild(i);
            child.gameObject.SetActive(false);
        }

        BundleLoaderLoadingText.Show();

        return false;
    }
}
