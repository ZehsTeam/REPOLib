using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using UnityEngine;

namespace REPOLib.Modules;

public static class Upgrades
{
    public static IReadOnlyList<PlayerUpgrade> PlayerUpgrades => _playerUpgrades.Values.ToList();
    private static readonly Dictionary<string, PlayerUpgrade> _playerUpgrades = [];

    public static PlayerUpgrade GetUpgrade(string upgradeId)
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
                pair.Value.playerDictionary = [];
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

    public static PlayerUpgrade RegisterUpgrade(string name, Action<PlayerController, int> startAction, Action<PlayerController, int> upgradeAction)
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
}

public class PlayerUpgrade
{
    public readonly string name;
    public Dictionary<string, int> playerDictionary {get; internal set;}
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

    public int GetLevel(PlayerAvatar player) 
    {
        string steamId = SemiFunc.PlayerGetSteamID(player);
        if (player == null || steamId == null) return 0;
        return GetLevel(steamId);
    }
    public int GetLevel(string steamId)
    {
        if (playerDictionary.TryGetValue(steamId, out int value)) return value;
        return 0;
    }

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

public class REPOLibUpgrade : MonoBehaviour
{
    private ItemToggle itemToggle;

    void Start()
    {
        itemToggle = GetComponent<ItemToggle>();
    }

    public void Upgrade(string upgradeId)
    {
        PlayerUpgrade upgrade = Upgrades.GetUpgrade(upgradeId);
        upgrade?.Upgrade(SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarGetFromPhotonID(itemToggle.playerTogglePhotonID)));
    }
}