using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using REPOLib.Modules;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(RoomVolumeCheck))]
internal static class RoomVolumeExample
{
    private static bool inTheTruck = false;

    [HarmonyPatch(nameof(RoomVolumeCheck.CheckSet))]
    [HarmonyPostfix]
    private static void CheckSetPatch(RoomVolumeCheck __instance)
    {
        if (!__instance.player)
            return;

        if(__instance.inTruck != inTheTruck)
        {
            inTheTruck = __instance.inTruck;

            //this is what we will send as our object
            //type must be something that can be serialized
            string message = $"{PlayerAvatar.instance.playerName} is detected in the truck!";
            string message2 = $"All clients besides me will see this message! - {PlayerAvatar.instance.playerName}";

            //multiplayer check needed to not throw errors in singleplayer
            if (SemiFunc.IsMultiplayer())
            {
                PhotonNetwork.RaiseEvent(NetworkingEvents.ExampleEvent.EventCode, message, NetworkingEvents.RaiseAll, SendOptions.SendReliable);
                //The same eventbyte can be used if targeting the same method
                PhotonNetwork.RaiseEvent(NetworkingEvents.ExampleEvent.EventCode, message2, NetworkingEvents.RaiseOthers, SendOptions.SendReliable);
            }

            //Below could be run for singleplayer compatibility, or always alongside the RaiseOthers property
            //NetworkingEvents.ExampleEvent.EventAction.Invoke(message);

        }

    }

    //This is our method that will be called by clients who receive the RaiseEvent
    //The object is our serialized object, this can be a single string, int, etc. Or a collection
    internal static void NetworkAnnouncement(object content)
    {
        //for example's sake, I will convert our serialized object back to a string
        string message = (string)content;

        Logger.Log(BepInEx.Logging.LogLevel.Message, $"{message}", true);
    }
}
