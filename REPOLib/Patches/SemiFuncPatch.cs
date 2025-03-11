using System.Reflection;
using HarmonyLib;
using REPOLib.Commands;
using REPOLib.Extensions;

namespace REPOLib.Patches
{
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
                if (commandMethod.GetCustomAttribute<REPOLibCommandExecutionAttribute>()?.RequiresDeveloperMode == true && !SteamManager.instance.developerMode)
                {
                    Logger.LogWarning($"Command {command} requires developer mode to be enabled. Enable it in REPOLib.cfg");
                    return false;
                }
                commandMethod.Invoke(null, [args]);

                return false;
            }

            return true;
        }
    }
}
