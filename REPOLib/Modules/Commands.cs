using System;
using System.Collections.Generic;
using System.Linq;
using ChatCommand = DebugCommandHandler.ChatCommand;

namespace REPOLib.Modules;

/// <summary>
/// The Commands module of REPOLib.
/// </summary>
public static class Commands
{
    /// <summary>
    /// Gets all <see cref="ChatCommand"/>s.
    /// </summary>
    /// <returns>A list of all <see cref="ChatCommand"/>s.</returns>
    public static IReadOnlyList<ChatCommand> AllCommands => GetCommands();

    /// <summary>
    /// Gets all commands registered with REPOLib.
    /// </summary>
    public static IReadOnlyList<ChatCommand> RegisteredCommands => _commandsRegistered;

    private static readonly List<ChatCommand> _commandsToRegister = [];
    private static readonly List<ChatCommand> _commandsRegistered = [];

    // This will run multiple times because of how the vanilla game registers commands.
    internal static void RegisterCommands()
    {
        if (DebugCommandHandler.instance == null)
        {
            Logger.LogError("Failed to register commands. DebugCommandHandler instance is null.");
            return;
        }

        Logger.LogInfo($"Adding commands.");

        _commandsRegistered.Clear();

        foreach (var command in _commandsToRegister)
        {
            DebugCommandHandler.instance.Register(command);
            _commandsRegistered.Add(command);
        }
    }

    /// <summary>
    /// Registers a <see cref="ChatCommand"/>.
    /// </summary>
    /// <param name="chatCommand">The <see cref="ChatCommand"/> to register.</param>
    /// <exception cref="ArgumentException"></exception>
    public static void RegisterCommand(ChatCommand chatCommand)
    {
        if (chatCommand == null)
        {
            throw new ArgumentException("Failed to register command. ChatCommand is null.");
        }

        if (chatCommand.Execute == null)
        {
            Logger.LogError($"Failed to register command \"{chatCommand.Name}\". Execute method is null.");
            return;
        }

        if (_commandsToRegister.Contains(chatCommand))
        {
            Logger.LogError($"Failed to register command \"{chatCommand.Name}\". Command is already registered!");
            return;
        }

        if (_commandsToRegister.Any(x => x.Name == chatCommand.Name))
        {
            Logger.LogError($"Failed to register command \"{chatCommand.Name}\". Command already exists with the same name.");
            return;
        }

        _commandsToRegister.Add(chatCommand);
    }

    private static IReadOnlyList<ChatCommand> GetCommands()
    {
        if (DebugCommandHandler.instance == null)
        {
            return [];
        }

        return DebugCommandHandler.instance._commands.Values.ToList();
    }
}
