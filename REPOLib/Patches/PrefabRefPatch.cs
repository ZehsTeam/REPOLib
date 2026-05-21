using HarmonyLib;
using REPOLib.Modules;
using UnityEngine;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(Resources))]
internal static class PrefabRefPatch
{
    [HarmonyPatch(nameof(Resources.Load))]
    [HarmonyPrefix]
    private static bool PrefabPatch(string path, ref GameObject __result)
    {
        if (NetworkPrefabs.TryGetNetworkPrefab(path, out GameObject? prefab))
        {
            __result = prefab;
            return false;
        }

        return true;
    }
}
