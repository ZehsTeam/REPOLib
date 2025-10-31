using HarmonyLib;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(DebugCommandHandler))]
internal static class DebugCommandHandlerPatch
{
    [HarmonyPatch(nameof(DebugCommandHandler.Start))]
    [HarmonyPostfix]
    private static void StartPatch()
    {
        Modules.Commands.RegisterCommands();
    }
}
