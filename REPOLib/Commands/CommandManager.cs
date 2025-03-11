using BepInEx;
using BepInEx.Configuration;
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
        private static List<MethodInfo> commandExecutionMethodCache = [];
        public static List<MethodInfo> CommandInitializerMethods { get; private set; } = [];
        public static Dictionary<string, bool> CommandsEnabled { get; private set; } = [];

        public static void Initialize()
        {
            Logger.LogInfo($"CommandManager initializing.", extended: true);
            CommandInitializerMethods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetMethods())
                .Where(method => method.GetCustomAttribute<CommandInitializerAttribute>() != null)
                .ToList();
            foreach (var command in CommandInitializerMethods)
            {
                try
                {
                    Logger.LogInfo($"Initializing command initializer on method {command.DeclaringType}.{command.Name}", extended:true);
                    if (!command.IsStatic)
                    {
                        Logger.LogWarning($"Command initializer {command.DeclaringType}.{command.Name} is not static!");
                    }
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

            BindConfigs();

            Logger.LogInfo("Finished initializing custom commands.");
        }

        public static void FindAllCommandMethods()
        {
            commandExecutionMethodCache = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetMethods())
                .Where(method => method.GetCustomAttribute<CommandExecutionAttribute>() != null)
                .ToList();

            foreach (var method in commandExecutionMethodCache)
            {
                var methodParams = method.GetParameters();
                if (methodParams.Length > 1)
                {
                    Logger.LogError($"Command \"{method.GetCustomAttribute<CommandExecutionAttribute>().Name}\" execution method \"{method.Name}\" has too many parameters! Should only have 1 string parameter or none.");
                    return;
                }
                if (methodParams.Length == 1 && methodParams[0].ParameterType != typeof(string))
                {
                    Logger.LogError($"Command \"{method.GetCustomAttribute<CommandExecutionAttribute>().Name}\" execution method \"{method.Name}\" has parameter of the wrong type! Should be string.");
                    return;
                }
                var aliasAttributes = method.GetCustomAttributes<CommandAliasAttribute>();
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
                        Logger.LogInfo($"Registered command alias \"{aliasAttribute.Alias}\" for method \"{method.DeclaringType}.{method.Name}\".", extended:true);
                        added = true;
                    }
                }
                if (!added)
                {
                    Logger.LogWarning($"Failed to add any command aliases for method \"{method.Name}\".");
                }
            }
        }

        public static void BindConfigs()
        {
            foreach (var method in commandExecutionMethodCache)
            {
                var execAttribute = method.GetCustomAttribute<CommandExecutionAttribute>();

                var bepinPluginClass = method.Module.Assembly.GetTypes()
                    .Where(type => type.GetCustomAttribute<BepInPlugin>() != null)
                    .ToList()[0].GetCustomAttribute<BepInPlugin>();
                string sourceModGUID = bepinPluginClass?.GUID ?? "Unknown";
                string sourceClass = method.DeclaringType.ToString() ?? "Unknown";

                List<string> aliases = [];
                foreach (var aliasAttribute in method.GetCustomAttributes<CommandAliasAttribute>())
                {
                    aliases.Add(aliasAttribute.Alias);
                }
                string commandName = execAttribute.Name ?? "Unknown";
                string commandDescription = $"(Alias(es): [{string.Join(", ", aliases)}])" + "\n\n" + (execAttribute.Description ?? "Unknown");
                bool enabled = execAttribute.EnabledByDefault;

                CommandsEnabled[commandName] = ConfigManager.ConfigFile.Bind("Commands."+sourceModGUID, commandName, defaultValue:enabled, commandDescription).Value;
            }
        }
    }
}
