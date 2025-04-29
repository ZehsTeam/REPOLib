using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using JetBrains.Annotations;

namespace REPOLib.Modules;

/// <summary>
/// // TODO: Document this.
/// </summary>
[PublicAPI]
public class NetworkedEvent
{
    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    public byte EventCode { get; private set; }
    
    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    public Action<EventData> EventAction { get; private set; }

    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="eventAction"></param>
    public NetworkedEvent(string name, Action<EventData> eventAction)
    {
        Name = name;
        EventAction = eventAction;

        if (NetworkingEvents.TryGetUniqueEventCode(out var eventCode))
        {
            EventCode = eventCode;
            NetworkingEvents.AddCustomEvent(this);
            Logger.LogInfo($"Registered NetworkedEvent \"{Name}\" with event code: {EventCode}", extended: true);
            return;
        }

        Logger.LogError($"Failed to register NetworkedEvent \"{Name}\". Could not get unique event code.");
    }

    /// <summary>
    /// This method works in multiplayer and singleplayer.
    /// </summary>
    public void RaiseEvent(object eventContent, RaiseEventOptions raiseEventOptions, SendOptions sendOptions)
    {
        if (SemiFunc.IsMultiplayer())
            PhotonNetwork.RaiseEvent(EventCode, eventContent, raiseEventOptions, sendOptions);
        else if (raiseEventOptions.Receivers != ReceiverGroup.Others)
            RaiseEventSingleplayer(eventContent);
    }

    private void RaiseEventSingleplayer(object eventContent)
    {
        if (SemiFunc.IsMultiplayer())
            return;
        
        var eventData = new EventData { Code = EventCode };
        eventData.Parameters[eventData.CustomDataKey] = eventContent;
        eventData.Parameters[eventData.SenderKey] = 1;
        EventAction?.Invoke(eventData);
    }
}
