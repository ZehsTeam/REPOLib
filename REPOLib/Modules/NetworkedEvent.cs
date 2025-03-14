using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REPOLib.Modules;

public class NetworkingEvents
{
    public static IReadOnlyList<NetworkedEvent> CustomEvents => _customEvents;

    /// <summary>
    /// Reserved event codes by Photon and the base game.
    /// </summary>
    public static readonly byte[] ReservedEventCodes = [0, 1, 2];

    private static readonly List<NetworkedEvent> _customEvents = [];

    public static readonly RaiseEventOptions RaiseAll = new()
    {
        Receivers = ReceiverGroup.All
    };

    public static readonly RaiseEventOptions RaiseOthers = new()
    {
        Receivers = ReceiverGroup.Others
    };

    internal static void Initialize()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;

        Application.quitting += () =>
        {
            PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
        };
    }

    internal static void AddCustomEvent(NetworkedEvent networkedEvent)
    {
        if (!_customEvents.Contains(networkedEvent))
        {
            _customEvents.Add(networkedEvent);
        }
    }

    private static void OnEvent(EventData photonEvent)
    {
        NetworkedEvent thisEvent = _customEvents.FirstOrDefault(e => e.EventCode == photonEvent.Code);

        if (thisEvent == null)
        {
            return;
        }

        thisEvent.EventAction?.Invoke(photonEvent);
    }

    internal static bool TryGetUniqueEventCode(out byte eventCode)
    {
        eventCode = 0;

        while (IsEventCodeTaken(eventCode) && eventCode < 200)
        {
            eventCode++;
        }

        if (eventCode > 200)
        {
            eventCode = 0;
            return false;
        }

        return true;
    }

    public static bool IsEventCodeTaken(byte eventCode)
    {
        if (ReservedEventCodes.Any(x => x == eventCode))
        {
            return true;
        }

        if (_customEvents.Any(x => x.EventCode == eventCode))
        {
            return true;
        }

        return false;
    }
}

public class NetworkedEvent
{
    public string Name { get; private set; }
    public byte EventCode { get; private set; }
    public Action<EventData> EventAction { get; private set; }

    public NetworkedEvent(string name, Action<EventData> eventAction)
    {
        Name = name;
        EventAction = eventAction;

        if (NetworkingEvents.TryGetUniqueEventCode(out byte eventCode))
        {
            EventCode = eventCode;
            NetworkingEvents.AddCustomEvent(this);

            Logger.LogInfo($"Registered NetworkedEvent \"{Name}\" with event code: {EventCode}", extended: true);
        }
        else
        {
            Logger.LogError($"Failed to register NetworkedEvent \"{Name}\". Could not get unique event code.");
        }
    }
}
