using System;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;

namespace REPOLib.Modules;

/// <summary>
///     // TODO: Document this.
/// </summary>
[PublicAPI]
public class NetworkedEvent
{
    /// <summary>
    ///     // TODO: Document this.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="eventAction"></param>
    public NetworkedEvent(string name, Action<EventData> eventAction)
    {
        this.Name = name;
        this.EventAction = eventAction;

        if (NetworkingEvents.TryGetUniqueEventCode(out byte eventCode))
        {
            this.EventCode = eventCode;
            NetworkingEvents.AddCustomEvent(this);
            Logger.LogInfo($"Registered NetworkedEvent \"{this.Name}\" with event code: {this.EventCode}", true);
            return;
        }

        Logger.LogError($"Failed to register NetworkedEvent \"{this.Name}\". Could not get unique event code.");
    }

    /// <summary>
    ///     // TODO: Document this.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    ///     // TODO: Document this.
    /// </summary>
    public byte EventCode { get; private set; }

    /// <summary>
    ///     // TODO: Document this.
    /// </summary>
    public Action<EventData> EventAction { get; private set; }

    /// <summary>
    ///     This method works in multiplayer and singleplayer.
    /// </summary>
    public void RaiseEvent(object eventContent, RaiseEventOptions raiseEventOptions, SendOptions sendOptions)
    {
        if (SemiFunc.IsMultiplayer())
            PhotonNetwork.RaiseEvent(this.EventCode, eventContent, raiseEventOptions, sendOptions);
        else if (raiseEventOptions.Receivers != ReceiverGroup.Others)
            this.RaiseEventSingleplayer(eventContent);
    }

    private void RaiseEventSingleplayer(object eventContent)
    {
        if (SemiFunc.IsMultiplayer())
            return;

        EventData eventData = new() { Code = this.EventCode };
        eventData.Parameters[eventData.CustomDataKey] = eventContent;
        eventData.Parameters[eventData.SenderKey] = 1;
        this.EventAction?.Invoke(eventData);
    }
}