using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
/// An instance of a registered <see cref="PlayerUpgrade"/>.
/// </summary>
[PublicAPI]
public class PlayerUpgrade
{
    /// <summary>
    /// The ID for this upgrade.
    /// </summary>
    public readonly string UpgradeId;

    /// <summary>
    /// The upgrade item.
    /// </summary>
    public readonly Item? Item;

    /// <summary>
    /// The dictionary of player Steam IDs and upgrade levels saved to the game file.
    /// </summary>
    public readonly Dictionary<string, int> PlayerDictionary = [];

    internal readonly Action<PlayerAvatar, int>? StartAction;
    private readonly Action<PlayerAvatar, int>? _upgradeAction;

    internal PlayerUpgrade(string upgradeId, Item? item, Action<PlayerAvatar, int>? startAction, Action<PlayerAvatar, int>? upgradeAction)
    {
        UpgradeId = upgradeId;
        Item = item;
        StartAction = startAction;
        _upgradeAction = upgradeAction;
    }

    #region GetLevel
    /// <summary>
    /// Gets the level of this upgrade for the given player.
    /// </summary>
    public int GetLevel(PlayerAvatar playerAvatar) 
        => playerAvatar == null ? 0 : GetLevel(playerAvatar.steamID);

    /// <summary>
    /// Gets the level of this upgrade for the given player's Steam ID.
    /// </summary>
    public int GetLevel(string steamId) 
        => PlayerDictionary.GetValueOrDefault(steamId, 0);
    #endregion

    #region AddLevel
    /// <summary>
    /// Adds the upgrade level(s) for the given player. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="playerAvatar">The player you want to add the upgrade level(s) to.</param>
    /// <param name="amount">The amount of upgrade levels to add.</param>
    /// <returns>The new upgrade level.</returns>
    public int AddLevel(PlayerAvatar playerAvatar, int amount = 1) 
        => playerAvatar == null ? 0 : AddLevel(playerAvatar.steamID, amount);

    /// <summary>
    /// Adds the upgrade level(s) for the given player's Steam ID. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="steamId">The player's Steam ID you want to add the upgrade level(s) to.</param>
    /// <param name="amount">The amount of upgrade levels to add.</param>
    /// <returns>The new upgrade level.</returns>
    public int AddLevel(string steamId, int amount = 1) 
        => ChangeLevelBy(steamId, amount);
    #endregion

    #region RemoveLevel
    /// <summary>
    /// Removes the upgrade level(s) for the given player. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="playerAvatar">The player you want to remove the upgrade level(s) from.</param>
    /// <param name="amount">The amount of upgrade levels to remove.</param>
    /// <returns>The new upgrade level.</returns>
    public int RemoveLevel(PlayerAvatar playerAvatar, int amount = 1) 
        => playerAvatar == null ? 0 : RemoveLevel(playerAvatar.steamID, amount);

    /// <summary>
    /// Removes the upgrade level(s) for the given player's Steam ID. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="steamId">The player's Steam ID you want to remove the upgrade level(s) from.</param>
    /// <param name="amount">The amount of upgrade levels to remove.</param>
    /// <returns>The new upgrade level.</returns>
    public int RemoveLevel(string steamId, int amount = 1) 
        => ChangeLevelBy(steamId, -amount);
    #endregion

    #region SetLevel
    /// <summary>
    /// Sets the upgrade level for the given player. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="playerAvatar">The player you want to set the upgrade level on.</param>
    /// <param name="level">The new level you want to set.</param>
    /// <returns>The new upgrade level.</returns>
    public int SetLevel(PlayerAvatar playerAvatar, int level) 
        => playerAvatar == null ? 0 : SetLevel(playerAvatar.steamID, level);

    /// <summary>
    /// Sets the upgrade level for the given player's Steam ID. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="steamId">The player's Steam ID you want to set the upgrade level on.</param>
    /// <param name="level">The new level you want to set.</param>
    /// <returns>The new upgrade level.</returns>
    public int SetLevel(string steamId, int level)
    {
        level = Mathf.Max(level, 0);
        this.PlayerDictionary[steamId] = level;
        this.ApplyUpgrade(steamId, level);
        Upgrades.RaiseUpgradeEvent(UpgradeId, steamId, level);
        return level;
    }
    #endregion

    private int ChangeLevelBy(string steamId, int amount)
        => SetLevel(steamId, GetLevel(steamId) + amount);

    internal void ApplyUpgrade(string steamId, int level)
    {
        PlayerDictionary[steamId] = level;
        if (_upgradeAction == null) return;

        var playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(steamId);
        if (playerAvatar == null) return;

        var username = SemiFunc.PlayerGetName(playerAvatar);
        
        try
        {
            _upgradeAction.Invoke(playerAvatar, GetLevel(steamId));
            Logger.LogDebug($"PlayerUpgrade: Invoked upgrade action for upgrade \"{UpgradeId}\" on player \"{username}\" at level {level}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"PlayerUpgrade: Failed to invoke upgrade action for upgrade \"{UpgradeId}\" on player \"{username}\". {ex}");
        }
    }
}