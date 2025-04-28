using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace REPOLib.Modules;

/// <summary>
/// The Upgrades module of REPOLib.
/// </summary>
public static class Upgrades
{
    /// <summary>
    /// Gets all <see cref="PlayerUpgrade"/>s registered with REPOLib.
    /// </summary>
    public static IReadOnlyList<PlayerUpgrade> PlayerUpgrades => _playerUpgrades.Values.ToList();

    private static readonly Dictionary<string, PlayerUpgrade> _playerUpgrades = [];

    private static NetworkedEvent? _upgradeEvent;

    internal static void Initialize()
    {
        _upgradeEvent = new NetworkedEvent("REPOLib Upgrade", HandleUpgradeEvent);
    }

    // This will run multiple times
    internal static void RegisterUpgrades()
    {
        if (StatsManager.instance == null)
        {
            Logger.LogError("Upgrades: Failed to register upgrades. StatsManager instance is null.");
            return;
        }

        foreach (var pair in _playerUpgrades)
        {
            string key = $"playerUpgrade{pair.Key}";
            var playerDictionary = pair.Value.PlayerDictionary;

            if (StatsManager.instance.dictionaryOfDictionaries.TryGetValue(key, out Dictionary<string, int> dictionary))
            {
                playerDictionary = dictionary;

                Logger.LogInfo($"Upgrades: Loaded upgrade \"{key}\" from StatsManager.", extended: true);
            }
            else
            {
                playerDictionary.Clear();
                StatsManager.instance.dictionaryOfDictionaries.Add(key, playerDictionary);

                Logger.LogInfo($"Upgrades: Added upgrade \"{key}\" to StatsManager.", extended: true);
            }
        }
    }

    internal static void InvokeStartActions(string steamId)
    {
        PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(steamId);

        if (playerAvatar == null)
        {
            return;
        }

        string username = SemiFunc.PlayerGetName(playerAvatar);

        foreach (PlayerUpgrade upgrade in PlayerUpgrades)
        {
            if (upgrade.StartAction == null)
            {
                continue;
            }

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
        if (_upgradeEvent == null)
        {
            return;
        }

        var data = new Hashtable
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
        {
            return;
        }

        string upgradeId = (string)data["UpgradeId"];
        string steamId = (string)data["SteamId"];
        int level = (int)data["Level"];

        if (TryGetUpgrade(upgradeId, out PlayerUpgrade? playerUpgrade))
        {
            playerUpgrade.ApplyUpgrade(steamId, level);
        }
    }

    /// <summary>
    /// Registers a <see cref="PlayerUpgrade"/>.
    /// </summary>
    /// <param name="upgradeId">The ID of your upgrade.</param>
    /// <param name="item">The upgrade item if it has one.</param>
    /// <param name="startAction">The method to be called during <see cref="PlayerController.LateStart"/> to initialize values for the upgrade. (<see cref="PlayerAvatar"/> is the player being refrenced.) (<see cref="int"/> is the level of this upgrade that the player has.)</param>
    /// <param name="upgradeAction">The method called whenever an instance of this upgrade is triggered to change value. (<see cref="PlayerAvatar"/> is the player being refrenced.) (<see cref="int"/> is the level of this upgrade that the player has.)</param>
    /// <returns>The newly registered <see cref="PlayerUpgrade"/> or null.</returns>
    public static PlayerUpgrade? RegisterUpgrade(string upgradeId, Item? item, Action<PlayerAvatar, int>? startAction, Action<PlayerAvatar, int>? upgradeAction)
    {
        if (_playerUpgrades.ContainsKey(upgradeId))
        {
            Logger.LogError($"Failed to register upgrade \"{upgradeId}\". An upgrade with this UpgradeId has already been registered.");
            return null;
        }

        var upgrade = new PlayerUpgrade(upgradeId, item, startAction, upgradeAction);
        _playerUpgrades.Add(upgradeId, upgrade);
        return upgrade;
    }

    /// <summary>
    /// Retrieves a registered <see cref="PlayerUpgrade"/> by it's identifier.
    /// </summary>
    /// <param name="upgradeId">The ID of the upgrade.</param>
    /// <returns>The <see cref="PlayerUpgrade"/> or null.</returns>
    public static PlayerUpgrade? GetUpgrade(string upgradeId)
    {
        if (_playerUpgrades.TryGetValue(upgradeId, out PlayerUpgrade playerUpgrade))
        {
            return playerUpgrade;
        }

        return null;
    }

    /// <summary>
    /// Tries to retrieves a registered <see cref="PlayerUpgrade"/> by it's identifier.
    /// </summary>
    /// <param name="upgradeId">The ID of the upgrade.</param>
    /// <param name="playerUpgrade">The found <see cref="PlayerUpgrade"/>.</param>
    /// <returns>Whether or not the <see cref="PlayerUpgrade"/> was found.</returns>
    public static bool TryGetUpgrade(string upgradeId, [NotNullWhen(true)] out PlayerUpgrade? playerUpgrade)
    {
        playerUpgrade = GetUpgrade(upgradeId);
        return playerUpgrade != null;
    }
}

/// <summary>
/// An instance of a registered <see cref="PlayerUpgrade"/>.
/// </summary>
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
    public Dictionary<string, int> PlayerDictionary = [];

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
    {
        if (playerAvatar == null)
        {
            return 0;
        }

        return GetLevel(playerAvatar.steamID);
    }

    /// <summary>
    /// Gets the level of this upgrade for the given player's Steam ID.
    /// </summary>
    public int GetLevel(string steamId)
    {
        if (PlayerDictionary.TryGetValue(steamId, out int value))
        {
            return value;
        }

        return 0;
    }
    #endregion

    #region AddLevel
    /// <summary>
    /// Adds the upgrade level(s) for the given player. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="playerAvatar">The player you want to add the upgrade level(s) to.</param>
    /// <param name="amount">The amount of upgrade levels to add.</param>
    /// <returns>The new upgrade level.</returns>
    public int AddLevel(PlayerAvatar playerAvatar, int amount = 1)
    {
        if (playerAvatar == null)
        {
            return 0;
        }

        return AddLevel(playerAvatar.steamID, amount);
    }

    /// <summary>
    /// Adds the upgrade level(s) for the given player's Steam ID. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="steamId">The player's Steam ID you want to add the upgrade level(s) to.</param>
    /// <param name="amount">The amount of upgrade levels to add.</param>
    /// <returns>The new upgrade level.</returns>
    public int AddLevel(string steamId, int amount = 1)
    {
        return ChangeLevelBy(steamId, amount);
    }
    #endregion

    #region RemoveLevel
    /// <summary>
    /// Removes the upgrade level(s) for the given player. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="playerAvatar">The player you want to remove the upgrade level(s) from.</param>
    /// <param name="amount">The amount of upgrade levels to remove.</param>
    /// <returns>The new upgrade level.</returns>
    public int RemoveLevel(PlayerAvatar playerAvatar, int amount = 1)
    {
        if (playerAvatar == null)
        {
            return 0;
        }

        return RemoveLevel(playerAvatar.steamID, amount);
    }

    /// <summary>
    /// Removes the upgrade level(s) for the given player's Steam ID. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="steamId">The player's Steam ID you want to remove the upgrade level(s) from.</param>
    /// <param name="amount">The amount of upgrade levels to remove.</param>
    /// <returns>The new upgrade level.</returns>
    public int RemoveLevel(string steamId, int amount = 1)
    {
        return ChangeLevelBy(steamId, -amount);
    }
    #endregion

    #region SetLevel
    /// <summary>
    /// Sets the upgrade level for the given player. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="playerAvatar">The player you want to set the upgrade level on.</param>
    /// <param name="level">The new level you want to set.</param>
    /// <returns>The new upgrade level.</returns>
    public int SetLevel(PlayerAvatar playerAvatar, int level)
    {
        if (playerAvatar == null)
        {
            return 0;
        }

        return SetLevel(playerAvatar.steamID, level);
    }

    /// <summary>
    /// Sets the upgrade level for the given player's Steam ID. (Calling this will network this change to other clients)
    /// </summary>
    /// <param name="steamId">The player's Steam ID you want to set the upgrade level on.</param>
    /// <param name="level">The new level you want to set.</param>
    /// <returns>The new upgrade level.</returns>
    public int SetLevel(string steamId, int level)
    {
        level = Mathf.Max(level, 0);

        PlayerDictionary[steamId] = level;

        ApplyUpgrade(steamId, level);

        Upgrades.RaiseUpgradeEvent(UpgradeId, steamId, level);

        return level;
    }
    #endregion

    private int ChangeLevelBy(string steamId, int amount)
    {
        int level = GetLevel(steamId);
        level += amount;

        return SetLevel(steamId, level);
    }

    internal void ApplyUpgrade(string steamId, int level)
    {
        PlayerDictionary[steamId] = level;

        if (_upgradeAction == null)
        {
            return;
        }

        PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(steamId);

        if (playerAvatar == null)
        {
            return;
        }

        string username = SemiFunc.PlayerGetName(playerAvatar);

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

/// <summary>
/// A component for easily triggering a <see cref="PlayerUpgrade"/>.
/// </summary>
public class REPOLibItemUpgrade : MonoBehaviour
{
    #pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [SerializeField]
    private string _upgradeId = null!;
    #pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

    /// <summary>
    /// The ID of your PlayerUpgrade.
    /// </summary>
    public string UpgradeId => _upgradeId;

    private ItemToggle _itemToggle = null!;

    private void Start()
    {
        _itemToggle = GetComponent<ItemToggle>();
    }

    /// <summary>
    /// Add the upgrade to the player that that triggered the <see cref="ItemToggle"/>.
    /// </summary>
    public void Upgrade()
    {
        if (_itemToggle == null)
        {
            Logger.LogError($"REPOLibItemUpgrade: Failed to upgrade \"{UpgradeId}\". ItemToggle is null.");
            return;
        }

        int playerTogglePhotonID = _itemToggle.playerTogglePhotonID;
        PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromPhotonID(playerTogglePhotonID);

        if (playerAvatar == null)
        {
            Logger.LogError($"REPOLibItemUpgrade: Failed to upgrade \"{UpgradeId}\". Could not find PlayerAvatar from ItemToggle's playerTogglePhotonID {playerTogglePhotonID}.");
            return;
        }

        if (!playerAvatar.isLocal)
        {
            return;
        }

        if (!Upgrades.TryGetUpgrade(UpgradeId, out PlayerUpgrade? playerUpgrade))
        {
            Logger.LogError($"REPOLibItemUpgrade: Failed to upgrade \"{UpgradeId}\". Could not find PlayerUpgrade from UpgradeId.");
            return;
        }

        playerUpgrade.AddLevel(playerAvatar);
    }
}