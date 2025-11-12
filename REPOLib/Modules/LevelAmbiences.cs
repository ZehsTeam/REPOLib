using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace REPOLib.Modules;

// This module doesn't need to be public since level ambiences are only used internally by levels.
internal static class LevelAmbiences
{
    public static IReadOnlyList<LevelAmbience> AllLevelAmbiences => AudioManager.instance?.levelAmbiences ?? [];
    public static IReadOnlyList<LevelAmbience> RegisteredLevelAmbiences => _levelAmbiencesRegistered;

    private static readonly List<LevelAmbience> _levelAmbiencesRegistered = [];

    private static readonly List<Level> _levelToProcess = [];
    private static readonly List<Level> _levelsProcessed = [];

    private static bool _initialLevelAmbiencesRegistered;

    // This will run multiple times because AudioManager is instantiated multiple times.
    internal static void RegisterLevelAmbiences()
    {
        if (_initialLevelAmbiencesRegistered)
        {
            foreach (var levelAmbience in _levelAmbiencesRegistered)
            {
                AddLevelAmbienceToAudioManager(levelAmbience);
            }

            return;
        }

        Logger.LogInfo($"Adding level ambiences.");

        foreach (var level in _levelToProcess)
        {
            RegisterLevelAmbiencesWithGame(level);
        }

        _initialLevelAmbiencesRegistered = true;
    }

    private static void AddLevelAmbienceToAudioManager(LevelAmbience levelAmbience)
    {
        if (AllLevelAmbiences.Contains(levelAmbience))
        {
            return;
        }

        AudioManager.instance.levelAmbiences.Add(levelAmbience);

        Logger.LogInfo($"Added level ambience \"{levelAmbience.name}\" to AudioManager.", extended: true);
    }

    private static void RegisterLevelAmbiencesWithGame(Level level)
    {
        if (_levelsProcessed.Contains(level))
        {
            return;
        }

        for (int i = 0; i < level.AmbiencePresets.Count; i++)
        {
            LevelAmbience levelAmbience = level.AmbiencePresets[i];

            if (AudioManager.instance.levelAmbiences.Contains(levelAmbience))
            {
                continue;
            }

            // This allows custom levels to use vanilla level ambiences, by using a proxy level ambience with the same name
            if (TryGetLevelAmbience(levelAmbience.name, out LevelAmbience? foundLevelAmbience))
            {
                level.AmbiencePresets[i] = foundLevelAmbience;
                Logger.LogInfo($"Replaced existing level ambience \"{levelAmbience.name}\" in level \"{level.name}\"", extended: true);
            }
            else
            {
                AddLevelAmbienceToAudioManager(levelAmbience);
                _levelAmbiencesRegistered.Add(levelAmbience);
                Logger.LogInfo($"Registered level ambience \"{levelAmbience.name}\" from level \"{level.name}\"", extended: true);
            }
        }

        _levelsProcessed.Add(level);
    }

    public static void RegisterLevelAmbience(Level? level)
    {
        if (level == null)
        {
            Logger.LogError($"Failed to register level ambience for level. Level is null.");
            return;
        }

        if (_levelToProcess.Contains(level))
        {
            Logger.LogWarning($"Failed to register level ambience for level \"{level.name}\". Level has already been processed!");
            return;
        }

        _levelToProcess.Add(level);

        if (_initialLevelAmbiencesRegistered)
        {
            RegisterLevelAmbiencesWithGame(level);
        }
    }

    private static bool TryGetLevelAmbience(string name, [NotNullWhen(true)] out LevelAmbience? levelAmbience)
    {
        levelAmbience = AllLevelAmbiences.FirstOrDefault(x => x.name == name);
        return levelAmbience != null;
    }
}
