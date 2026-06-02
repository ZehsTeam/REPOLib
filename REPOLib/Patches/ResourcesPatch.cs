using System;
using HarmonyLib;
using REPOLib.Modules;
using UnityEngine;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(Resources))]
internal static class ResourcesPatch
{
    [HarmonyPatch(nameof(Resources.Load), typeof(string), typeof(Type))]
    [HarmonyPrefix]
    private static bool PrefabPatch(string path, Type systemTypeInstance, ref UnityEngine.Object __result)
    {
        if (systemTypeInstance == typeof(GameObject) && NetworkPrefabs.TryGetNetworkPrefab(path, out GameObject? prefab))
        {
            __result = prefab;
            return false;
        }

        return true;
    }
}