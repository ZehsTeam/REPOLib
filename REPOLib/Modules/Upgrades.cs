using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace REPOLib.Modules;

/// <summary>
///     The Upgrades module of REPOLib.
/// </summary>
[PublicAPI]
public static class Upgrades
{
    private static readonly Dictionary<string, PlayerUpgrade> _playerUpgrades = [];
    private static readonly NetworkedEvent? _upgradeEvent = new("REPOLib Upgrade", HandleUpgradeEvent);

    /// <summary>
    ///     Gets all <see cref="PlayerUpgrade" />s registered with REPOLib.
    /// </summary>
    public static IEnumerable<PlayerUpgrade> GetPlayerUpgrades()
        => _playerUpgrades.Values;

    // This will run multiple times
    internal static void RegisterUpgrades(StatsManager instance)
    {
        if (instance == null)
        {
            Logger.LogError("Upgrades: Failed to register upgrades. StatsManager instance is null.");
            return;
        }

        foreach (KeyValuePair<string, PlayerUpgrade> pair in _playerUpgrades)
        {
            string key = $"playerUpgrade{pair.Key}";
            Dictionary<string, int> playerDictionary = pair.Value.PlayerDictionary;

            if (!instance.dictionaryOfDictionaries.TryGetValue(key, out _))
            {
                playerDictionary.Clear();
                instance.dictionaryOfDictionaries.Add(key, playerDictionary);

                Logger.LogInfo($"Upgrades: Added upgrade \"{key}\" to StatsManager.", true);
                continue;
            }

            Logger.LogInfo($"Upgrades: Loaded upgrade \"{key}\" from StatsManager.", true);
        }
    }

    internal static void InvokeStartActions(string steamId)
    {
        PlayerAvatar? playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(steamId);
        if (playerAvatar == null)
            return;

        string? username = SemiFunc.PlayerGetName(playerAvatar);
        foreach (PlayerUpgrade? upgrade in GetPlayerUpgrades())
        {
            if (upgrade.StartAction is null)
                continue;

            try
            {
                int level = upgrade.GetLevel(steamId);
                upgrade.StartAction.Invoke(playerAvatar, level);
                Logger.LogDebug($"Upgrades: Invoked start action for upgrade \"{upgrade.UpgradeId}\" on player \"{username}\" at level {level}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Upgrades: Failed to invoke start action for upgrade \"{upgrade.UpgradeId}\" on player \"{username}\". {ex}");
            }
        }
    }

    internal static void RaiseUpgradeEvent(string upgradeId, string steamId, int level)
    {
        if (_upgradeEvent is null)
            return;

        Hashtable data = new()
        { 
            { "UpgradeId", upgradeId },
            { "SteamId", steamId },
            { "Level", level } 
        };

        _upgradeEvent.RaiseEvent(data, NetworkingEvents.RaiseOthers, SendOptions.SendReliable);
    }

    private static void HandleUpgradeEvent(EventData eventData)
    {
        if (eventData.CustomData is not Hashtable data)
            return;

        string? upgradeId = (string)data["UpgradeId"];
        string? steamId = (string)data["SteamId"];
        int level = (int)data["Level"];

        if (TryGetUpgrade(upgradeId, out PlayerUpgrade? playerUpgrade))
            playerUpgrade.ApplyUpgrade(steamId, level);
    }

    /// <summary>
    ///     Registers a <see cref="PlayerUpgrade" />.
    /// </summary>
    /// <param name="upgradeId">The ID of your upgrade.</param>
    /// <param name="item">The upgrade item if it has one.</param>
    /// <param name="startAction">
    ///     The method to be called during <see cref="PlayerController.LateStart" /> to initialize values
    ///     for the upgrade. (<see cref="PlayerAvatar" /> is the player being refrenced.) (<see cref="int" /> is the level of
    ///     this upgrade that the player has.)
    /// </param>
    /// <param name="upgradeAction">
    ///     The method called whenever an instance of this upgrade is triggered to change value. (
    ///     <see cref="PlayerAvatar" /> is the player being refrenced.) (<see cref="int" /> is the level of this upgrade that
    ///     the player has.)
    /// </param>
    /// <returns>The newly registered <see cref="PlayerUpgrade" /> or null.</returns>
    public static PlayerUpgrade? RegisterUpgrade(string upgradeId, Item? item, Action<PlayerAvatar, int>? startAction,
        Action<PlayerAvatar, int>? upgradeAction)
    {
        if (_playerUpgrades.ContainsKey(upgradeId))
        {
            Logger.LogError(
                $"Failed to register upgrade \"{upgradeId}\". An upgrade with this UpgradeId has already been registered.");
            return null;
        }

        PlayerUpgrade upgrade = new(upgradeId, item, startAction, upgradeAction);
        _playerUpgrades.Add(upgradeId, upgrade);
        return upgrade;
    }

    /// <summary>
    ///     Retrieves a registered <see cref="PlayerUpgrade" /> by it's identifier.
    /// </summary>
    /// <param name="upgradeId">The ID of the upgrade.</param>
    /// <returns>The <see cref="PlayerUpgrade" /> or null.</returns>
    public static PlayerUpgrade? GetUpgrade(string upgradeId)
        => _playerUpgrades.GetValueOrDefault(upgradeId);

    /// <summary>
    ///     Tries to retrieve a registered <see cref="PlayerUpgrade" /> by it's identifier.
    /// </summary>
    /// <param name="upgradeId">The ID of the upgrade.</param>
    /// <param name="playerUpgrade">The found <see cref="PlayerUpgrade" />.</param>
    /// <returns>Whether the <see cref="PlayerUpgrade" /> was found.</returns>
    public static bool TryGetUpgrade(string upgradeId, [NotNullWhen(true)] out PlayerUpgrade? playerUpgrade)
        => (playerUpgrade = GetUpgrade(upgradeId)) != null;
}