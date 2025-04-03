using System.Collections.Generic;
using System.Linq;

namespace REPOLib.Commands;

internal static class ListCommandsCommand
{
    [CommandExecution(
        "List Commands",
        "Lists all commands with their name, aliases, and descriptions in the log",
        requiresDeveloperMode: false
    )]
    [CommandAlias("listcommands")]
    [CommandAlias("listcom")]
    [CommandAlias("lc")]
    public static void Execute(string args)
    {
        Logger.LogInfo($"Running command lister", extended: true);
        
        List<(int,string,List<string>,string)> activeCommands = [];
        foreach (var commandExecutionInfo in CommandManager.CommandExecutionMethods)
        {
            var metadataToken = commandExecutionInfo.Value.MetadataToken;
            if (activeCommands.Exists(x => x.Item1 == metadataToken))
            {
                activeCommands.First(x => x.Item1 == metadataToken)
                    .Item3.Add(commandExecutionInfo.Key);
                continue;
            }
            
            var attributeData = commandExecutionInfo.Value.CustomAttributes.First(x =>
                x.AttributeType == typeof(CommandExecutionAttribute));
            
            var name = attributeData.ConstructorArguments[0].Value as string;
            var description = attributeData.ConstructorArguments[1].Value as string;
            
            activeCommands.Add((metadataToken,name,[commandExecutionInfo.Key],description)!);
        }
        
        Logger.LogInfo("Commands:");
        foreach (var listedCommand in activeCommands)
        {
            var aliases = string.Join(", ", listedCommand.Item3);
            Logger.LogInfo("   " + $"{listedCommand.Item2} (Aliases: {aliases}): {listedCommand.Item4}");
        }
    }
}