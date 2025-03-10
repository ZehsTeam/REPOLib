using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace REPOLib.Commands
{
    public static class CommandManager
    {
        public static Dictionary<string, MethodInfo> CommandExecutionMethods { get; private set; } = [];
        public static List<MethodInfo> CommandInitializerMethods { get; private set; } = [];

        public static void Initialize()
        {
            FindAllCommandMethods();

            foreach (var command in CommandInitializerMethods)
            {
                try
                {
                    if (!command.IsStatic)
                    {
                        Logger.LogWarning($"Command initializer {command.Name} is not static!");
                    }
                    command.Invoke(null, null);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Failed to initialize command: {e}");
                }
            }
            foreach (var command in CommandExecutionMethods)
            {
                if (!command.Value.IsStatic)
                {
                    Logger.LogWarning($"Command execution method for command \"{command.Key}\" is not static!");
                }
            }
            Logger.LogInfo("Finished initializing custom commands.");
        }

        public static void FindAllCommandMethods()
        {
            var executionMethods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetMethods())
                .Where(method => method.GetCustomAttribute<CommandExecutionAttribute>() != null)
                .ToList();

            foreach (var method in executionMethods)
            {
                var aliasAttributes = method.GetCustomAttributes<CommandAliasAttribute>();
                if (aliasAttributes == null || aliasAttributes.Count() == 0)
                {
                    Logger.LogWarning($"Command {method.Name} has no alias attributes!");
                    continue;
                }
                foreach (var aliasAttribute in aliasAttributes)
                {
                    CommandExecutionMethods.Add(aliasAttribute.Alias, method);
                }
            }

            CommandInitializerMethods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetMethods())
                .Where(method => method.GetCustomAttribute<CommandInitializerAttribute>() != null)
                .ToList();
        }
    }
}
