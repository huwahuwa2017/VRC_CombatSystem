using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayerData : UdonSharpBehaviour
{
    [SerializeField]
    private PlayerConsole _udonPlayerConsole;

    [SerializeField]
    private PlayerHealth _udonPlayerHealth;

    [SerializeField]
    private FlightController _udonFlightController;

    [SerializeField]
    private DamageArraySync _udonDamageArraySync;

    public PlayerConsole UdonPlayerConsole
    {
        get => _udonPlayerConsole;
    }

    public PlayerHealth UdonPlayerHealth
    {
        get => _udonPlayerHealth;
    }

    public FlightController UdonFlightController
    {
        get => _udonFlightController;
    }

    public DamageArraySync UdonDamageArraySync
    {
        get => _udonDamageArraySync;
    }



    [UdonSynced, FieldChangeCallback(nameof(PlayerId))]
    private int _playerId;

    public int PlayerId
    {
        get
        {
            return _playerId;
        }
        set
        {
            _playerId = value;
            _udonPlayerConsole.SetPlayerId = _playerId;
            _udonPlayerHealth.SetPlayerId = _playerId;
            _udonFlightController.SetPlayerId = _playerId;
        }
    }



    public void PlayerConsoleInteract()
    {
        Debug.Log("Interact");

        if (_playerId != 0) return;

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        int newId = localPlayer.playerId;

        Transform parent = transform.parent;

        foreach (Transform child in parent)
        {
            PlayerData playerData = child.GetComponent<PlayerData>();
            if (playerData.PlayerId == newId) return;
        }

        if (!Networking.IsOwner(localPlayer, gameObject))
        {
            Networking.SetOwner(localPlayer, gameObject);
        }

        GameObject temp = _udonDamageArraySync.gameObject;

        if (!Networking.IsOwner(localPlayer, temp))
        {
            Networking.SetOwner(localPlayer, temp);
        }

        PlayerId = newId;

        RequestSerialization();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        Debug.Log("OnPlayerLeft");

        if (player.playerId != _playerId) return;

        PlayerId = 0;
    }
}
