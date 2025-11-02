using HarmonyLib;
using REPOLib.Modules;
using UnityEngine;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(PrefabRef))]
internal static class PrefabRefPatch
{
    [HarmonyPatch(nameof(PrefabRef.Prefab), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool PrefabPatch(PrefabRef __instance, ref bool __result)
    {
        if (NetworkPrefabs.TryGetNetworkPrefab(__instance.resourcePath, out GameObject? prefab))
        {
            __result = prefab;
            return false;
        }

        return true;
    }
}
