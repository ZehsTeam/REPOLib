using HarmonyLib;
using REPOLib.Commands;
using System;
using System.Reflection;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(SemiFunc))]
internal static class SemiFuncPatch
{
    [HarmonyPatch(nameof(SemiFunc.Command))]
    [HarmonyPrefix]
    private static bool CommandPatch(string _command)
    {
        if (_command.StartsWith("/"))
        {
            return Command(_command);
        }

        return true;
    }

    private static bool Command(string message)
    {
        var text = message.ToLower();

        var command = text.Split(' ')[0].Substring(1);
        string args = "";
        if (text.Length > command.Length)
        {
            args = text.Substring(command.Length + 1).Trim();
        }

        MethodInfo commandMethod;
        CommandManager.CommandExecutionMethods.TryGetValue(command, out commandMethod);
        if (commandMethod != null)
        {
            var execAttribute = commandMethod.GetCustomAttribute<CommandExecutionAttribute>();
            if (CommandManager.CommandsEnabled.TryGetValue(execAttribute.Name, out bool enabled))
            {
                if (!enabled)
                {
                    return false;
                }
            }
            if (execAttribute != null &&  execAttribute.RequiresDeveloperMode == true && !SteamManager.instance.developerMode)
            {
                Logger.LogWarning($"Command {command} requires developer mode to be enabled. Enable it in REPOLib.cfg");
                return false;
            }
            try
            {
                var methodParams = commandMethod.GetParameters();
                if (methodParams.Length == 0)
                {
                    commandMethod.Invoke(null, null);
                }
                else
                {
                    commandMethod.Invoke(null, [args]);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Error executing command: {e}");
            }

            return false;
        }

        return true;
    }
}
