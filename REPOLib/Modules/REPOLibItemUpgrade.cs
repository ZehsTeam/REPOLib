using JetBrains.Annotations;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
///     A component for easily triggering a <see cref="PlayerUpgrade" />.
/// </summary>
[PublicAPI]
public class REPOLibItemUpgrade : MonoBehaviour
{
    [SerializeField]
    private string _upgradeId = string.Empty;
    
    private ItemToggle _itemToggle = null!;

    /// <summary>
    ///     The ID of your PlayerUpgrade.
    /// </summary>
    public string UpgradeId
        => this._upgradeId;

    private void Start()
        => this._itemToggle = this.GetComponent<ItemToggle>();

    /// <summary>
    ///     Add the upgrade to the player that that triggered the <see cref="ItemToggle" />.
    /// </summary>
    public void Upgrade()
    {
        if (this._itemToggle == null)
        {
            Logger.LogError($"REPOLibItemUpgrade: Failed to upgrade \"{this.UpgradeId}\". ItemToggle is null.");
            return;
        }

        int playerTogglePhotonID = this._itemToggle.playerTogglePhotonID;
        PlayerAvatar? playerAvatar = SemiFunc.PlayerAvatarGetFromPhotonID(playerTogglePhotonID);

        if (playerAvatar == null)
        {
            Logger.LogError($"REPOLibItemUpgrade: Failed to upgrade \"{this.UpgradeId}\". Could not find PlayerAvatar from ItemToggle's playerTogglePhotonID {playerTogglePhotonID}.");
            return;
        }

        if (!playerAvatar.isLocal)
            return;

        if (!Upgrades.TryGetUpgrade(this.UpgradeId, out PlayerUpgrade? playerUpgrade))
        {
            Logger.LogError($"REPOLibItemUpgrade: Failed to upgrade \"{this.UpgradeId}\". Could not find PlayerUpgrade from UpgradeId.");
            return;
        }

        playerUpgrade.AddLevel(playerAvatar);
    }
}