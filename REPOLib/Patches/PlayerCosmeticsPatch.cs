using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Photon.Pun;
using REPOLib.Modules;
using REPOLib.Objects;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(PlayerCosmetics))]
internal static class PlayerCosmeticsPatch
{
    [HarmonyPatch(nameof(PlayerCosmetics.Awake))]
    [HarmonyPrefix]
    private static void AwakePatch(PlayerCosmetics __instance)
    {
        if (!__instance.gameObject.GetComponent<PlayerCosmeticsModded>())
        {
            __instance.gameObject.AddComponent<PlayerCosmeticsModded>();
        }
    }

    // Sync the list of cosmetics as a list of names
    [HarmonyPatch(nameof(PlayerCosmetics.SetupCosmetics))]
    [HarmonyPrefix]
    private static bool SetupCosmeticsPatch(PlayerCosmetics __instance, bool _synced, bool _forced, List<int> _cosmetics)
    {
        PhotonView _photonView = __instance.deathHead && __instance.deathHead.setup && __instance.deathHead.playerAvatar ? __instance.deathHead.playerAvatar.photonView : __instance.photonView;
        
        if(_synced && SemiFunc.IsMultiplayer() && !_photonView.IsMine) return true;

        PlayerCosmeticsModded playerCosmeticsModded = __instance.gameObject.GetComponent<PlayerCosmeticsModded>();
        if(!playerCosmeticsModded) return true;

        playerCosmeticsModded.cosmeticEquipped.Clear();

        if(!SemiFunc.IsMultiplayer() || !_synced) return true;

        var _cosmeticEquipped = _cosmetics ?? MetaManager.instance.cosmeticEquipped;

        #region Local Player
        __instance.SetupCosmeticsLogic(_cosmeticEquipped.ToArray(), _forced);
        #endregion

        #region Modded
        List<string> _cosmeticEquippedModded = new();
        foreach(var _cosmetic in _cosmeticEquipped){
            if(_cosmetic < 0 || _cosmetic >= MetaManager.instance.cosmeticAssets.Count) continue;

            var _cosmeticAsset = MetaManager.instance.cosmeticAssets[_cosmetic];
            if(!_cosmeticAsset) continue;

            _cosmeticEquippedModded.Add(_cosmeticAsset.assetId);
        }

        PhotonNetwork.RemoveBufferedRPCs(playerCosmeticsModded.photonView.ViewID, nameof(playerCosmeticsModded.SetupCosmeticsModdedRPC));
        playerCosmeticsModded.photonView.RPC(nameof(playerCosmeticsModded.SetupCosmeticsModdedRPC), RpcTarget.OthersBuffered, string.Join("\x1F", _cosmeticEquippedModded));
        #endregion

        #region Vanilla
        bool IsValidVanillaCosmetic(int x) =>
            x >= 0 && x < MetaManager.instance.cosmeticAssets.Count &&
            MetaManager.instance.cosmeticAssets[x] != null &&
            !Cosmetics.RegisteredCosmetics.Contains(MetaManager.instance.cosmeticAssets[x]);

        PhotonNetwork.RemoveBufferedRPCs(__instance.photonView.ViewID, nameof(__instance.SetupCosmeticsRPC));
        __instance.photonView.RPC(nameof(__instance.SetupCosmeticsRPC), RpcTarget.OthersBuffered, _cosmeticEquipped.Where(IsValidVanillaCosmetic).ToArray(), _forced);
        #endregion

        return false;

    }

    // Overwrite the values of _cosmeticEquipped to use the string version
    [HarmonyPatch(nameof(PlayerCosmetics.SetupCosmeticsLogic))]
    [HarmonyPrefix]
    private static void SetupCosmeticsLogicPatch(PlayerCosmetics __instance, ref int[] _cosmeticEquipped, bool _forced)
    {
        PlayerCosmeticsModded playerCosmeticsModded = __instance.gameObject.GetComponent<PlayerCosmeticsModded>();
        if(!playerCosmeticsModded || playerCosmeticsModded.cosmeticEquipped.Count == 0) return;

        List<int> _cosmeticEquippedInts = new();
        foreach(var _cosmetic in playerCosmeticsModded.cosmeticEquipped.ToList()){
            var _cosmeticIndex = MetaManager.instance.cosmeticAssets.FindIndex(x => x.assetId == _cosmetic);
            if(_cosmeticIndex >= 0) _cosmeticEquippedInts.Add(_cosmeticIndex);
        }
        _cosmeticEquipped = _cosmeticEquippedInts.ToArray();
    }
}