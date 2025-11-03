using HarmonyLib;
using REPOLib.Modules;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(DebugCommandHandler))]
internal static class DebugCommandHandlerPatch
{
    [HarmonyPatch(nameof(DebugCommandHandler.Start))]
    [HarmonyPostfix]
    private static void StartPatch()
    {
        Commands.RegisterCommands();
    }
}
