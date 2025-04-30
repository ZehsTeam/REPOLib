using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using REPOLib.Extensions;

namespace REPOLib.Commands;

internal static class CommandManager
{
    private static List<MethodInfo> _commandExecutionMethodCache = [];
    public static Dictionary<string, MethodInfo> CommandExecutionMethods { get; private set; } = [];
    private static List<MethodInfo> CommandInitializerMethods { get; set; } = [];
    public static Dictionary<string, bool> CommandsEnabled { get; private set; } = [];
    public static void Initialize()
    {
        Logger.LogInfo("CommandManager initializing.", true);

        CommandInitializerMethods = AccessTools.AllTypes()
                                               .SelectMany(type => type.SafeGetMethods())
                                               .Where(method => method.HasCustomAttribute<CommandInitializerAttribute>())
                                               .ToList();

        foreach (MethodInfo? command in CommandInitializerMethods)
        {
            try
            {
                Logger.LogDebug($"Initializing command initializer on method {command.DeclaringType}.{command.Name}");
                if (!command.IsStatic)
                    Logger.LogWarning($"Command initializer {command.DeclaringType}.{command.Name} is not static!");

                command.Invoke(null, null);
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to initialize command: {e}");
            }
        }

        FindAllCommandMethods();

        foreach (KeyValuePair<string, MethodInfo> command in CommandExecutionMethods.Where(command => !command.Value.IsStatic))
            Logger.LogWarning($"Command execution method for command \"{command.Key}\" is not static!");

        BindConfigs();
        Logger.LogInfo("Finished initializing custom commands.");
    }

    private static void FindAllCommandMethods()
    {
        _commandExecutionMethodCache = AccessTools.AllTypes()
                                                  .SelectMany(type => type.SafeGetMethods())
                                                  .Where(method => method.HasCustomAttribute<CommandExecutionAttribute>())
                                                  .ToList();

        foreach (MethodInfo? method in _commandExecutionMethodCache)
        {
            ParameterInfo[] methodParams = method.GetParameters();
            switch (methodParams.Length)
            {
                case > 1:
                    Logger.LogError(
                        $"Command \"{method.GetCustomAttribute<CommandExecutionAttribute>().Name}\" execution method \"{method.Name}\" has too many parameters! Should only have 1 string parameter or none.");
                    return;
                case 1 when methodParams[0].ParameterType != typeof(string):
                    Logger.LogError(
                        $"Command \"{method.GetCustomAttribute<CommandExecutionAttribute>().Name}\" execution method \"{method.Name}\" has parameter of the wrong type! Should be string.");
                    return;
            }

            bool added = false;
            CommandAliasAttribute[] aliasAttributes = method.GetCustomAttributes<CommandAliasAttribute>().ToArray();
            if (!aliasAttributes.Any())
            {
                Logger.LogWarning($"Command {method.Name} has no alias attributes!");
                continue;
            }

            foreach (CommandAliasAttribute aliasAttribute in aliasAttributes)
            {
                if (!CommandExecutionMethods.TryAdd(aliasAttribute.Alias, method))
                    continue;

                Logger.LogDebug($"Registered command alias \"{aliasAttribute.Alias}\" for method \"{method.DeclaringType}.{method.Name}\".");
                added = true;
            }

            if (!added)
                Logger.LogWarning($"Failed to add any command aliases for method \"{method.Name}\".");
        }
    }

    private static void BindConfigs()
    {
        foreach (MethodInfo? method in _commandExecutionMethodCache)
        {
            List<string> aliases = method.GetCustomAttributes<CommandAliasAttribute>()
                                         .Select(aliasAttribute => aliasAttribute.Alias)
                                         .ToList();

            CommandExecutionAttribute? execAttribute = method.GetCustomAttribute<CommandExecutionAttribute>();
            BepInPlugin? bepInPluginClass = AccessTools.GetTypesFromAssembly(method.Module.Assembly)
                                                       .Where(type => type.GetCustomAttribute<BepInPlugin>() is not null)
                                                       .ToList()[0].GetCustomAttribute<BepInPlugin>();

            string sourceModGuid = bepInPluginClass?.GUID ?? "Unknown";
            string commandName = execAttribute.Name;
            string commandDescription = $"(Alias(es): [{string.Join(", ", aliases)}])" + "\n\n" + execAttribute.Description;
            bool enabled = execAttribute.EnabledByDefault;

            CommandsEnabled[commandName] = ConfigManager.ConfigFile.Bind(
                "Commands." + sourceModGuid,
                commandName,
                enabled,
                commandDescription).Value;
        }
    }
}