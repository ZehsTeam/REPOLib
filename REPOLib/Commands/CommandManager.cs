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
            foreach (var command in CommandInitializerMethods)
            {
                try
                {
                    if (!command.IsStatic)
                    {
                        Logger.LogWarning($"Command initializer {command.Name} is not static!");
                    }
                    Logger.LogInfo($"Initializing command {command.Name}", extended: true);
                    command.Invoke(null, null);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Failed to initialize command: {e}");
                }
            }

            FindAllCommandMethods();
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
                .Where(method => method.GetCustomAttribute<REPOLibCommandExecutionAttribute>() != null)
                .ToList();

            foreach (var method in executionMethods)
            {
                var aliasAttributes = method.GetCustomAttributes<REPOLibCommandAliasAttribute>();
                bool added = false;
                if (aliasAttributes == null || aliasAttributes.Count() == 0)
                {
                    Logger.LogWarning($"Command {method.Name} has no alias attributes!");
                    continue;
                }
                foreach (var aliasAttribute in aliasAttributes)
                {
                    if (CommandExecutionMethods.TryAdd(aliasAttribute.Alias, method))
                    {
                        Logger.LogWarning($"Skipped duplicate command alias \"{aliasAttribute.Alias}\" on method \"{method.Name}\".");
                    }
                    else
                        added = true;
                }
                if (!added)
                {
                    Logger.LogWarning($"Failed to add any command aliases for method \"{method.Name}\".");
                }
            }
        }
    }
}
