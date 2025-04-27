using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using UnityEngine;

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

    /// <summary>
    /// Retrieves a registered <see cref="PlayerUpgrade"/> by it's identifier.
    /// </summary>
    public static PlayerUpgrade? GetUpgrade(string upgradeId)
    {
        if (_playerUpgrades.TryGetValue(upgradeId, out PlayerUpgrade upgrade)) return upgrade;
        return null;
    }
    
    internal static void RegisterUpgrades()
    {
        if (StatsManager.instance == null)
        {
            Logger.LogError("Failed to register upgrades. StatsManager instance is null.");
            return;
        }

        foreach (KeyValuePair<string, PlayerUpgrade> pair in _playerUpgrades)
        {
            string key = $"playerUpgrade{pair.Key}";

            if (StatsManager.instance.dictionaryOfDictionaries.ContainsKey(key))
            {
                Logger.LogWarning($"Failed to add upgrade \"{key}\"", extended: true);
                continue;
            }
            else
            {
                pair.Value.playerDictionary.Clear();
                StatsManager.instance.dictionaryOfDictionaries.Add(key, pair.Value.playerDictionary);
                Logger.LogInfo($"Added upgrade \"{key}\"", extended: true);
            }
        }
    }

    internal static void LateStartInitUpgrades(PlayerController playerController, string steamId)
    {
        foreach (PlayerUpgrade upgrade in _playerUpgrades.Values)
        {
            if (upgrade.startAction == null) continue;
            // Added safety since this code is inserted in the middle of vanilla coroutine via transpiler and I don't want to break too much if one mod throws an error
            try
            {
                upgrade.startAction.Invoke(playerController, upgrade.GetLevel(steamId));
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }

    /// <summary>
    /// Registers a <see cref="PlayerUpgrade"/>.
    /// </summary>
    /// <param name="name">The name of the upgrade you want to create.</param>
    /// <param name="startAction">The method to be called during <see cref="PlayerController.LateStart"/> to initialize values for the upgrade. (<see cref="PlayerController"/> is the player being refrenced.) (<see cref="int"/> is the level of this upgrade that the player has.)</param>
    /// <param name="upgradeAction">The method called whenever an instance of this upgrade is triggered to change value. (<see cref="PlayerController"/> is the player being refrenced.) (<see cref="int"/> is the level of this upgrade that the player has.)</param>
    public static PlayerUpgrade? RegisterUpgrade(string name, Action<PlayerController, int> startAction, Action<PlayerController, int> upgradeAction)
    {
        if (_playerUpgrades.ContainsKey(name))
        {
            Logger.LogError($"Failed to register upgrade \"{name}\". An upgrade with this key has already been registered.");
            return null;
        }

        PlayerUpgrade upgrade = new(name, startAction, upgradeAction);

        _playerUpgrades.Add(name, upgrade);

        return upgrade;
    }

    /// <summary>
    /// Registers a <see cref="PlayerUpgrade"/>.
    /// </summary>
    /// <param name="name">The name of the upgrade you want to create.</param>
    /// <param name="upgradeAction">The method called whenever an instance of this upgrade is triggered to change value. (<see cref="PlayerController"/> is the player being refrenced.) (<see cref="int"/> is the level of this upgrade that the player has.)</param>
    public static PlayerUpgrade? RegisterUpgrade(string name, Action<PlayerController, int> upgradeAction)
    => RegisterUpgrade(name, upgradeAction, upgradeAction);
}

/// <summary>
/// An instance of a registered <see cref="PlayerUpgrade"/>.
/// </summary>
public class PlayerUpgrade
{
    /// <summary>
    /// The name used to register this upgrade.
    /// This is used as the upgrade's key.
    /// </summary>
    public readonly string name;
    /// <summary>
    /// The dictionary of player identifiers and upgrade levels saved to the game file.
    /// </summary>
    public readonly Dictionary<string, int> playerDictionary = [];
    private readonly NetworkedEvent upgradeEvent;
    internal readonly Action<PlayerController, int> startAction;
    private readonly Action<PlayerController, int> upgradeAction;

    internal PlayerUpgrade(string name, Action<PlayerController, int> startAction, Action<PlayerController, int> upgradeAction)
    {
        this.name = name;
        this.startAction = startAction;
        this.upgradeAction = upgradeAction;
        upgradeEvent = new NetworkedEvent($"ModdedPlayerUpgrade{name}", UpgradeHandler);
    }

    /// <summary>
    /// Gets the level of this upgrade for the given player.
    /// </summary>
    public int GetLevel(PlayerAvatar player) 
    {
        string steamId = SemiFunc.PlayerGetSteamID(player);
        if (player == null || steamId == null) return 0;
        return GetLevel(steamId);
    }
    /// <summary>
    /// Gets the level of this upgrade for the given player's steamId.
    /// </summary>
    public int GetLevel(string steamId)
    {
        if (playerDictionary.TryGetValue(steamId, out int value)) return value;
        return 0;
    }

    /// <summary>
    /// Triggers using this upgrade for the given player. (Calling this will network this change to other clients)
    /// </summary>
    public void Upgrade(PlayerAvatar player) 
    {
        string steamId = SemiFunc.PlayerGetSteamID(player);
        if (player == null || steamId == null) return;
        Upgrade(steamId);
    }
    /// <summary>
    /// Triggers using this upgrade for the given player's steamId. (Calling this will network this change to other clients)
    /// </summary>
	public int Upgrade(string playerId)
	{
        if (!playerDictionary.TryGetValue(playerId, out int num))
        {
            playerDictionary.Add(playerId, num = 0);
        }
        num++;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			UpdateRightAway(playerId);
		}
		if (SemiFunc.IsMasterClient())
		{
            upgradeEvent.RaiseEvent(string.Concat(playerId, '\n', num), NetworkingEvents.RaiseOthers, SendOptions.SendReliable);
		}
		return playerDictionary[playerId] = num;
	}

	private void UpgradeHandler(EventData data)
	{
        string[] rawData = ((string)data.CustomData).Split('\n');
        if (rawData.Length == 2 && int.TryParse(rawData[1], out int value))
        {
            string steamId = rawData[0];
            if (playerDictionary.ContainsKey(steamId)) playerDictionary[steamId] = value;
            else playerDictionary.Add(steamId, value);
            UpdateRightAway(steamId);
        }
	}

	private void UpdateRightAway(string playerId)
	{
        if (upgradeAction == null) return;
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(playerId);
		if (playerAvatar == SemiFunc.PlayerAvatarLocal() && playerAvatar.playerTransform.gameObject.TryGetComponent(out PlayerController controller))
		{
            upgradeAction.Invoke(controller, GetLevel(playerId));
		}
	}
}

/// <summary>
/// A component for easily triggering a <see cref="PlayerUpgrade"/>.
/// </summary>
public class REPOLibUpgrade : MonoBehaviour
{
    #pragma warning disable CS8618
    private ItemToggle itemToggle;
    #pragma warning restore CS8618

    void Start()
    {
        itemToggle = GetComponent<ItemToggle>();
    }

    /// <summary>
    /// Gets the level of this upgrade for the given player's steamId.
    /// </summary>
    public void Upgrade(string upgradeId)
    {
        PlayerUpgrade? upgrade = Upgrades.GetUpgrade(upgradeId);
        upgrade?.Upgrade(SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarGetFromPhotonID(itemToggle.playerTogglePhotonID)));
    }
}