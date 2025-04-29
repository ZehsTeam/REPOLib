using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace REPOLib.Modules;

/// <summary>
/// A component for easily triggering a <see cref="PlayerUpgrade"/>.
/// </summary>
[PublicAPI]
public class REPOLibItemUpgrade : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("_upgradeId")] 
    private string upgradeId = string.Empty;

    /// <summary>
    /// The ID of your PlayerUpgrade.
    /// </summary>
    public string UpgradeId => upgradeId;

    private ItemToggle _itemToggle = null!;
    private void Start() => _itemToggle = GetComponent<ItemToggle>();

    /// <summary>
    /// Add the upgrade to the player that that triggered the <see cref="ItemToggle"/>.
    /// </summary>
    public void Upgrade()
    {
        if (_itemToggle == null)
        {
            Logger.LogError($"REPOLibItemUpgrade: Failed to upgrade \"{UpgradeId}\". ItemToggle is null.");
            return;
        }

        var playerTogglePhotonID = _itemToggle.playerTogglePhotonID;
        var playerAvatar = SemiFunc.PlayerAvatarGetFromPhotonID(playerTogglePhotonID);

        if (playerAvatar == null)
        {
            Logger.LogError($"REPOLibItemUpgrade: Failed to upgrade \"{UpgradeId}\". Could not find PlayerAvatar from ItemToggle's playerTogglePhotonID {playerTogglePhotonID}.");
            return;
        }

        if (!playerAvatar.isLocal) return;
        if (!Upgrades.TryGetUpgrade(UpgradeId, out var playerUpgrade))
        {
            Logger.LogError($"REPOLibItemUpgrade: Failed to upgrade \"{UpgradeId}\". Could not find PlayerUpgrade from UpgradeId.");
            return;
        }

        playerUpgrade.AddLevel(playerAvatar);
    }
}