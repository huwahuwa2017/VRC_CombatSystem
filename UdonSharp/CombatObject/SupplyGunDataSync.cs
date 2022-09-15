using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SupplyGunDataSync : UdonSharpBehaviour
{
    [SerializeField]
    private VRCPickup _vrcPickup;

    [SerializeField]
    private Gun _udonGun;

    [SerializeField]
    private Transform _playerDataArray;



    [UdonSynced, FieldChangeCallback(nameof(SetPlayerId))]
    private int _playerId;

    public int SetPlayerId
    {
        set
        {
            Debug.Log("SupplyGunDataSync.SetPlayerId");

            int targetPlayerId = value;
            PlayerData playerData = PlayerDataSearch(targetPlayerId);

            if (playerData == null)
            {
                _playerId = 0;
                _vrcPickup.Drop();
                return;
            }

            _playerId = targetPlayerId;

            DamageArraySync damageArraySync = playerData.UdonDamageArraySync;
            PlayerHealth playerHealth = playerData.UdonPlayerHealth;

            _udonGun.UdonDamageArraySync = damageArraySync;
            _udonGun.GameObjectMask = new GameObject[] { playerHealth.gameObject };
        }
    }

    private PlayerData PlayerDataSearch(int targetPlayerId)
    {
        foreach (Transform child in _playerDataArray)
        {
            PlayerData playerData = child.GetComponent<PlayerData>();

            if (playerData.PlayerId == targetPlayerId)
            {
                return playerData;
            }
        }

        return null;
    }



    public void Pickup()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        Networking.SetOwner(localPlayer, gameObject);
        Networking.SetOwner(localPlayer, _udonGun.gameObject);

        SetPlayerId = localPlayer.playerId;
        RequestSerialization();
    }

    public void PickupUseDown()
    {
        _udonGun.Trigger = true;
    }

    public void PickupUseUp()
    {
        _udonGun.Trigger = false;
    }



    //Referenced by GroupData.Restart()
    #region
    public void Restart()
    {
        Debug.Log("Restart : SupplyGunDataSync");

        _udonGun.Trigger = false;
    }
    #endregion



    //Referenced by Gun.FixedUpdate()
    #region
    public Vector3 GetMoveVelocityReturnValue;

    public void GetMoveVelocity()
    {
        GetMoveVelocityReturnValue = Vector3.zero;

        if (_playerId == 0) return;
        VRCPlayerApi pa = VRCPlayerApi.GetPlayerById(_playerId);
        if (pa == null) return;

        GetMoveVelocityReturnValue = pa.GetVelocity();
    }
    #endregion
}
