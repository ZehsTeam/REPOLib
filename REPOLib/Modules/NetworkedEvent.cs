using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using REPOLib.Patches;
using System;
using System.Collections.Generic;
using System.Linq;

namespace REPOLib.Modules
{
    public class NetworkingEvents
    {
        public static List<NetworkedEvent> AllCustomEvents = [];
        internal static NetworkedEvent ExampleEvent = null!;

        public static RaiseEventOptions RaiseAll = new()
        {
            Receivers = ReceiverGroup.All
        };

        public static RaiseEventOptions RaiseOthers = new()
        {
            Receivers = ReceiverGroup.Others
        };

        internal static void Init()
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;

            InitExample();
        }

        internal static void InitExample()
        {
            ExampleEvent ??= new("REPOLib Example", RoomVolumeExample.NetworkAnnouncement);
        }

        private static void OnEvent(EventData photonEvent)
        {
            NetworkedEvent thisEvent = AllCustomEvents.FirstOrDefault(e => e.EventByte == photonEvent.Code);
            if (thisEvent == null)
                return;

            thisEvent.EventAction.Invoke(photonEvent.CustomData);
        }
    }

    public class NetworkedEvent
    {
        internal Action<object> EventAction;
        internal byte EventByte;
        internal string Name;

        public NetworkedEvent(string name, Action<object> eventAction)
        {
            Name = name;
            EventAction = eventAction;
            EventByte = GetUniqueID();
            NetworkingEvents.AllCustomEvents.Add(this);
        }

        private static byte GetUniqueID()
        {
            byte id = 1;
            do
            {
                id++;
            } while (NetworkingEvents.AllCustomEvents.Any(u => u.EventByte == id));

            return id;
        }
    }
}
