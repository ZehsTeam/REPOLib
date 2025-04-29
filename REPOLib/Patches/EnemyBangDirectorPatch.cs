using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(EnemyBangDirector))]
internal static class EnemyBangDirectorPatch
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EnemyBangDirector.Awake))]
    private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var originalMethod = AccessTools.Method(typeof(EnemyBangDirector), nameof(EnemyBangDirector.Setup));
        var replacementMethod = AccessTools.Method(typeof(EnemyBangDirectorPatch), nameof(PreSetup));

        if (originalMethod is null || replacementMethod is null)
        {
            Logger.LogError("EnemyBangDirectorPatch: failed to find required methods for AwakeTranspiler.");
            return instructions;
        }

        var modifiedInstructions = new List<CodeInstruction>();

        foreach (var instruction in instructions)
        {
            var isMethodCall = instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt;
            if (isMethodCall && instruction.operand is MethodInfo methodInfo && methodInfo == originalMethod)
            {
                // Replace original method call with replacement method call
                modifiedInstructions.Add(new CodeInstruction(OpCodes.Call, replacementMethod));
                Logger.LogDebug($"EnemyBangDirectorPatch: AwakeTranspiler replaced {originalMethod.Name} call with {replacementMethod.Name}.");
                continue;
            }

            modifiedInstructions.Add(instruction); // Add unmodified instruction
        }

        return modifiedInstructions.AsEnumerable();
    }

    private static IEnumerator PreSetup(EnemyBangDirector instance)
    {
        if (LevelGenerator.Instance.Generated)
            yield return new WaitForSeconds(0.1f);

        yield return instance.Setup();
    }
}
