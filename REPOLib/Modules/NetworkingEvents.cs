using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
///     // TODO: Document this.
/// </summary>
[PublicAPI]
public static class NetworkingEvents
{
    /// <summary>
    ///     Reserved event codes by Photon and the base game.
    /// </summary>
    public static readonly byte[] ReservedEventCodes = [ 0, 1, 2 ];

    /// <summary>
    ///     // TODO: Document this.
    /// </summary>
    public static readonly RaiseEventOptions RaiseAll = new() { Receivers = ReceiverGroup.All };

    /// <summary>
    ///     // TODO: Document this.
    /// </summary>
    public static readonly RaiseEventOptions RaiseOthers = new() { Receivers = ReceiverGroup.Others };

    /// <summary>
    ///     // TODO: Document this.
    /// </summary>
    public static readonly RaiseEventOptions RaiseMasterClient = new() { Receivers = ReceiverGroup.MasterClient };

    private static readonly List<NetworkedEvent> _customEvents = [];

    /// <summary>
    ///     // TODO: Document this.
    /// </summary>
    public static IReadOnlyList<NetworkedEvent> GetCustomEvents()
        => _customEvents;
    internal static void Initialize()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        Application.quitting += () => PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    internal static void AddCustomEvent(NetworkedEvent networkedEvent)
    {
        if (!_customEvents.Contains(networkedEvent))
            _customEvents.Add(networkedEvent);
    }

    private static void OnEvent(EventData photonEvent)
    {
        _customEvents.FirstOrDefault(e => e.EventCode == photonEvent.Code)?.EventAction.Invoke(photonEvent);
    }

    internal static bool TryGetUniqueEventCode(out byte eventCode)
    {
        eventCode = 0;
        while (IsEventCodeTaken(eventCode) && eventCode < 200)
            eventCode++;

        if (eventCode <= 200 && (eventCode != 200 || !IsEventCodeTaken(eventCode)))
            return true;

        eventCode = 0;
        return false;
    }

    /// <summary>
    ///     // TODO: Document this.
    /// </summary>
    /// <param name="eventCode"></param>
    /// <returns></returns>
    public static bool IsEventCodeTaken(byte eventCode)
        => ReservedEventCodes.Any(x => x == eventCode) || _customEvents.Any(x => x.EventCode == eventCode);
}