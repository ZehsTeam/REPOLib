using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using REPOLib.Modules;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(PlayerController))]
internal static class PlayerControllerPatch
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PlayerController.LateStart), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> LateStartTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        FieldInfo? playerAvatarScript =
            AccessTools.Field(typeof(PlayerController), nameof(PlayerController.playerAvatarScript));
        MethodInfo? getPlayerSteamIdCall =
            AccessTools.Method(typeof(SemiFunc), nameof(SemiFunc.PlayerGetSteamID), [ typeof(PlayerAvatar) ]);
        MethodInfo? initUpgradesMethod =
            AccessTools.Method(typeof(Upgrades), nameof(Upgrades.InvokeStartActions), [ typeof(string) ]);

        int place = 0;
        bool found = false;
        List<CodeInstruction> newInstructions = [];

        foreach (CodeInstruction? instruction in instructions)
        {
            newInstructions.Add(instruction);

            if (place switch
                { 0 => instruction.opcode == OpCodes.Ldloc_1,
                  1 => instruction.opcode == OpCodes.Ldfld && instruction.LoadsField(playerAvatarScript),
                  2 => instruction.opcode == OpCodes.Call && instruction.Calls(getPlayerSteamIdCall),
                  3 => instruction.opcode == OpCodes.Stloc_2,
                  _ => false })
            {
                place++;
                if (place <= 3) continue;

                place = 0;
                found = true;
                newInstructions.Add(new CodeInstruction(OpCodes.Ldloc_2));
                newInstructions.Add(new CodeInstruction(OpCodes.Call, initUpgradesMethod));
            }
            else
                place = 0;
        }

        foreach (CodeInstruction? instruction in newInstructions)
            yield return instruction;

        if (!found) Logger.LogWarning("Failed to patch PlayerController.LateStart!");
    }
}