using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
///     An instance of a registered <see cref="PlayerUpgrade" />.
/// </summary>
[PublicAPI]
public class PlayerUpgrade
{
    /// <summary>
    ///     The upgrade item.
    /// </summary>
    public readonly Item? Item;

    /// <summary>
    ///     The dictionary of player Steam IDs and upgrade levels saved to the game file.
    /// </summary>
    public readonly Dictionary<string, int> PlayerDictionary = [];

    /// <summary>
    ///     The ID for this upgrade.
    /// </summary>
    public readonly string UpgradeId;
    
    private readonly Action<PlayerAvatar, int>? _upgradeAction;
    internal readonly Action<PlayerAvatar, int>? StartAction;
    internal PlayerUpgrade(string upgradeId, Item? item, Action<PlayerAvatar, int>? startAction,
        Action<PlayerAvatar, int>? upgradeAction)
    {
        this.UpgradeId = upgradeId;
        this.Item = item;
        this.StartAction = startAction;
        this._upgradeAction = upgradeAction;
    }

    private int ChangeLevelBy(string steamId, int amount)
        => this.SetLevel(steamId, this.GetLevel(steamId) + amount);

    internal void ApplyUpgrade(string steamId, int level)
    {
        this.PlayerDictionary[steamId] = level;
        if (this._upgradeAction == null)
            return;

        PlayerAvatar? playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(steamId);
        if (playerAvatar == null)
            return;

        string? username = SemiFunc.PlayerGetName(playerAvatar);

        try
        {
            this._upgradeAction.Invoke(playerAvatar, this.GetLevel(steamId));
            Logger.LogDebug(
                $"PlayerUpgrade: Invoked upgrade action for upgrade \"{this.UpgradeId}\" on player \"{username}\" at level {level}");
        }
        catch (Exception ex)
        {
            Logger.LogError(
                $"PlayerUpgrade: Failed to invoke upgrade action for upgrade \"{this.UpgradeId}\" on player \"{username}\". {ex}");
        }
    }

    #region GetLevel

    /// <summary>
    ///     Gets the level of this upgrade for the given player.
    /// </summary>
    public int GetLevel(PlayerAvatar playerAvatar)
        => playerAvatar == null ? 0 : this.GetLevel(playerAvatar.steamID);

    /// <summary>
    ///     Gets the level of this upgrade for the given player's Steam ID.
    /// </summary>
    public int GetLevel(string steamId)
        => this.PlayerDictionary.GetValueOrDefault(steamId, 0);

    #endregion

    #region AddLevel

    /// <summary>
    ///     Adds the upgrade level(s) for the given player. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="playerAvatar">The player you want to add the upgrade level(s) to.</param>
    /// <param name="amount">The amount of upgrade levels to add.</param>
    /// <returns>The new upgrade level.</returns>
    public int AddLevel(PlayerAvatar playerAvatar, int amount = 1)
        => playerAvatar == null
            ? 0
            : this.AddLevel(playerAvatar.steamID, amount);

    /// <summary>
    ///     Adds the upgrade level(s) for the given player's Steam ID. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="steamId">The player's Steam ID you want to add the upgrade level(s) to.</param>
    /// <param name="amount">The amount of upgrade levels to add.</param>
    /// <returns>The new upgrade level.</returns>
    public int AddLevel(string steamId, int amount = 1)
        => this.ChangeLevelBy(steamId, amount);

    #endregion

    #region RemoveLevel

    /// <summary>
    ///     Removes the upgrade level(s) for the given player. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="playerAvatar">The player you want to remove the upgrade level(s) from.</param>
    /// <param name="amount">The amount of upgrade levels to remove.</param>
    /// <returns>The new upgrade level.</returns>
    public int RemoveLevel(PlayerAvatar playerAvatar, int amount = 1)
        => playerAvatar == null ? 0 : this.RemoveLevel(playerAvatar.steamID, amount);

    /// <summary>
    ///     Removes the upgrade level(s) for the given player's Steam ID. (Calling this will network this change to other
    ///     clients)
    /// </summary>
    /// <param name="steamId">The player's Steam ID you want to remove the upgrade level(s) from.</param>
    /// <param name="amount">The amount of upgrade levels to remove.</param>
    /// <returns>The new upgrade level.</returns>
    public int RemoveLevel(string steamId, int amount = 1)
        => this.ChangeLevelBy(steamId, -amount);

    #endregion

    #region SetLevel

    /// <summary>
    ///     Sets the upgrade level for the given player. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="playerAvatar">The player you want to set the upgrade level on.</param>
    /// <param name="level">The new level you want to set.</param>
    /// <returns>The new upgrade level.</returns>
    public int SetLevel(PlayerAvatar playerAvatar, int level)
        => playerAvatar == null ? 0 : this.SetLevel(playerAvatar.steamID, level);

    /// <summary>
    ///     Sets the upgrade level for the given player's Steam ID. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="steamId">The player's Steam ID you want to set the upgrade level on.</param>
    /// <param name="level">The new level you want to set.</param>
    /// <returns>The new upgrade level.</returns>
    public int SetLevel(string steamId, int level)
    {
        level = Mathf.Max(level, 0);
        this.PlayerDictionary[steamId] = level;
        this.ApplyUpgrade(steamId, level);
        Upgrades.RaiseUpgradeEvent(this.UpgradeId, steamId, level);
        return level;
    }

    #endregion
}