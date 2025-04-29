using System;
using System.Reflection;
using HarmonyLib;
using REPOLib.Commands;
using REPOLib.Modules;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(SemiFunc))]
internal static class SemiFuncPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SemiFunc.Command))]
    private static bool CommandPatch(string _command)
        => !_command.StartsWith("/") || Command(_command);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SemiFunc.EnemySpawn))]
    private static bool EnemySpawnPatch(ref bool __result)
    {
        if (Enemies.SpawnNextEnemiesNotDespawned <= 0)
            return true;

        Enemies.SpawnNextEnemiesNotDespawned--;
        __result = true;
        return false;
    }

    private static bool Command(string message)
    {
        string args = "";
        string text = message.ToLower();
        string? command = text.Split(' ')[0][1..];

        if (text.Length > command.Length)
            args = text[(command.Length + 1)..].Trim();

        CommandManager.CommandExecutionMethods.TryGetValue(command, out MethodInfo? commandMethod);
        if (commandMethod == null)
            return true;

        CommandExecutionAttribute? execAttribute = commandMethod.GetCustomAttribute<CommandExecutionAttribute>();
        if (CommandManager.CommandsEnabled.TryGetValue(execAttribute.Name, out bool enabled) && !enabled)
            return false;

        if (execAttribute.RequiresDeveloperMode && !ConfigManager.DeveloperMode.Value)
        {
            Logger.LogWarning($"Command {command} requires developer mode to be enabled. Enable it in REPOLib.cfg");
            return false;
        }

        try
        {
            if (commandMethod.GetParameters().Length <= 0)
                commandMethod.Invoke(null, null);
            else
                commandMethod.Invoke(null, [ args ]);
        }
        catch (Exception e)
        {
            Logger.LogError($"Error executing command: {e}");
        }

        return false;
    }
}