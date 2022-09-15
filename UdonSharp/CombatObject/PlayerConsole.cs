using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class PlayerConsole : UdonSharpBehaviour
{
    [SerializeField]
    private Text _UI_Text;

    [SerializeField]
    private PlayerData _playerData;

    [SerializeField]
    private FlightController _flightControllerVR;

    private int _playerId;

    public int SetPlayerId
    {
        set
        {
            _playerId = value;

            FlightControllerRespawn();

            if (_playerId == 0)
            {
                _UI_Text.text = string.Empty;
                return;
            }

            VRCPlayerApi targetPlayer = VRCPlayerApi.GetPlayerById(_playerId);
            _UI_Text.text = $"{targetPlayer.displayName} ({_playerId})";
        }
    }

    public override void Interact()
    {
        Debug.Log("Interact");

        if (Networking.LocalPlayer.playerId != _playerId)
        {
            _playerData.PlayerConsoleInteract();
            return;
        }

        SendCustomNetworkEvent(NetworkEventTarget.All, "FlightControllerRespawn");
    }

    public void FlightControllerRespawn()
    {
        Transform fcvrTransform = _flightControllerVR.transform;
        fcvrTransform.position = transform.position + new Vector3(0f, 0.6f, -0.4f);
        fcvrTransform.rotation = transform.rotation;
    }
}
