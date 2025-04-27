using ExitGames.Client.Photon;
using REPOLib.Extensions;
using Steamworks;
using System;
using System.Collections.Generic;
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

        foreach (PlayerUpgrade upgrade in PlayerUpgrades)
        {
            if (upgrade.StartAction == null)
            {
                continue;
            }

            try
            {
                upgrade.StartAction.Invoke(playerAvatar, upgrade.GetLevel(steamId));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Upgrades: Failed to invoke start action for upgrade \"{upgrade.UpgradeId}\". {ex}");
            }
        }
    }

    internal static void RaiseUpgradeEvent(string upgradeId, string steamId, int value)
    {
        if (_upgradeEvent == null)
        {
            return;
        }

        var data = new Hashtable
        {
            { "UpgradeId", upgradeId },
            { "SteamId", steamId },
            { "Value", value }
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
        int value = (int)data["Value"];

        PlayerUpgrade? playerUpgrade = GetUpgrade(upgradeId);

        if (playerUpgrade == null)
        {
            return;
        }

        playerUpgrade.UpgradeOthers(steamId, value);
    }

    /// <summary>
    /// Registers a <see cref="PlayerUpgrade"/>.
    /// </summary>
    /// <param name="upgradeId">The ID of your upgrade.</param>
    /// <param name="item">The upgrade item if it has one.</param>
    /// <param name="startAction">The method to be called during <see cref="PlayerController.LateStart"/> to initialize values for the upgrade. (<see cref="PlayerAvatar"/> is the player being refrenced.) (<see cref="int"/> is the level of this upgrade that the player has.)</param>
    /// <param name="upgradeAction">The method called whenever an instance of this upgrade is triggered to change value. (<see cref="PlayerAvatar"/> is the player being refrenced.) (<see cref="int"/> is the level of this upgrade that the player has.)</param>
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
    public static PlayerUpgrade? GetUpgrade(string upgradeId)
    {
        if (_playerUpgrades.TryGetValue(upgradeId, out PlayerUpgrade playerUpgrade))
        {
            return playerUpgrade;
        }

        return null;
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
    /// The dictionary of player identifiers and upgrade levels saved to the game file.
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
    /// Gets the level of this upgrade for the given player's steamId.
    /// </summary>
    public int GetLevel(string steamId)
    {
        if (PlayerDictionary.TryGetValue(steamId, out int value))
        {
            return value;
        }

        return 0;
    }

    /// <summary>
    /// Triggers using this upgrade for the given player. (Calling this will network this change to other clients)
    /// </summary>
    public int Upgrade(PlayerAvatar playerAvatar) 
    {
        if (playerAvatar == null)
        {
            return 0;
        }

        return Upgrade(playerAvatar.steamID);
    }
    
    /// <summary>
    /// Triggers using this upgrade for the given player's steamId. (Calling this will network this change to other clients)
    /// </summary>
	public int Upgrade(string steamId)
	{
        int value = GetLevel(steamId);
        value++;

        PlayerDictionary[steamId] = value;

        if (SemiFunc.IsMasterClientOrSingleplayer())
		{
            UpdateRightAway(steamId);
        }

        if (SemiFunc.IsMasterClient())
        {
            Upgrades.RaiseUpgradeEvent(UpgradeId, steamId, value);
        }

        Logger.LogInfo($"PlayerUpgrade: Upgrade \"{UpgradeId}\" for player \"{steamId}\". Level: {value}");

        return value;
    }

    internal void UpgradeOthers(string steamId, int value)
    {
        Logger.LogInfo($"PlayerUpgrade: UpgradeOthers \"{UpgradeId}\" for player \"{steamId}\". Level: {value}");

        PlayerDictionary[steamId] = value;
        UpdateRightAway(steamId);
    }

	private void UpdateRightAway(string steamId)
	{
        if (_upgradeAction == null)
        {
            return;
        }

		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(steamId);

        if (playerAvatar == null)
        {
            return;
        }

        try
        {
            _upgradeAction.Invoke(playerAvatar, GetLevel(steamId));
        }
        catch (Exception ex)
        {
            Logger.LogError($"PlayerUpgrade: Failed to invoke upgrade action for upgrade \"{UpgradeId}\". {ex}");
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

        PlayerUpgrade? upgrade = Upgrades.GetUpgrade(UpgradeId);

        if (upgrade == null)
        {
            Logger.LogError($"REPOLibItemUpgrade: Failed to upgrade \"{UpgradeId}\". Could not find PlayerUpgrade from UpgradeId.");
            return;
        }

        int playerTogglePhotonID = _itemToggle.playerTogglePhotonID;
        PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromPhotonID(playerTogglePhotonID);

        if (playerAvatar == null)
        {
            Logger.LogError($"REPOLibItemUpgrade: Failed to upgrade \"{UpgradeId}\". Could not find PlayerAvatar from ItemToggle's playerTogglePhotonID {playerTogglePhotonID}.");
            return;
        }

        string steamId = playerAvatar.steamID;

        if (!steamId.IsValidSteamId())
        {
            Logger.LogError($"REPOLibItemUpgrade: Failed to upgrade \"{UpgradeId}\". Steam ID \"{steamId}\" is invalid.");
            return;
        }

        upgrade.Upgrade(steamId);
    }
}