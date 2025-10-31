﻿using BepInEx;
using HarmonyLib;
using REPOLib.Commands;
using REPOLib.Modules;
using System.Linq;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(RunManager))]
internal static class RunManagerPatch
{
    private static bool _patchedAwake = false;

    [HarmonyPatch(nameof(RunManager.Awake))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void AwakePatch(RunManager __instance)
    {
        if (_patchedAwake) return;
        _patchedAwake = true;

        NetworkPrefabs.Initialize();
        NetworkingEvents.Initialize();
        Levels.RegisterInitialLevels();
        Modules.Modules.RegisterInitialModules();
        Valuables.RegisterInitialValuables();

        BundleLoader.OnAllBundlesLoaded += CommandManager.Initialize;
        BundleLoader.FinishLoadOperations(__instance);
    }
}
