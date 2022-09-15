using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class FlightController : UdonSharpBehaviour
{
    [SerializeField]
    private VRCPickup _vrcPickup;

    [SerializeField]
    private float _speed = 1f;

    private float _inputMoveHorizontal;

    private float _inputMoveVertical;

    private bool _inputJump;

    private bool _desktopFlightMode;

    private bool _pickup;

    private int _playerId;

    public int SetPlayerId
    {
        set
        {
            _playerId = value;
            gameObject.SetActive(_playerId != 0);
        }
    }

    public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
    {
        _inputMoveHorizontal = value;
    }

    public override void InputMoveVertical(float value, UdonInputEventArgs args)
    {
        _inputMoveVertical = value;
    }

    public override void InputJump(bool value, UdonInputEventArgs args)
    {
        _inputJump = value;
    }

    public override void OnPickup()
    {
        if (Networking.LocalPlayer.playerId != _playerId)
        {
            _vrcPickup.Drop();
            return;
        }

        _pickup = true;
    }

    public override void OnDrop()
    {
        if (Networking.LocalPlayer.playerId != _playerId) return;

        _pickup = false;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (localPlayer.playerId != _playerId || localPlayer.IsUserInVR()) return;

        _desktopFlightMode = !_desktopFlightMode;
    }

    private void FixedUpdate()
    {
        if (!(_pickup || _desktopFlightMode)) return;

        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        Quaternion controllerRotation = (_desktopFlightMode) ? localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation : transform.rotation;
        Vector3 addVelocity = controllerRotation * new Vector3(_inputMoveHorizontal, (_inputJump) ? 1f : 0f, _inputMoveVertical) * _speed;
        Vector3 newVelocity = (localPlayer.GetVelocity() + addVelocity) * 0.98f;
        localPlayer.SetVelocity(newVelocity);
    }

    /*
    private void FlightModeOn()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        localPlayer.SetWalkSpeed(0);
        localPlayer.SetRunSpeed(0);
        localPlayer.SetStrafeSpeed(0);
        localPlayer.SetJumpImpulse(0);
        localPlayer.SetGravityStrength(1);
    }

    private void FlightModeOff()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        localPlayer.SetWalkSpeed();
        localPlayer.SetRunSpeed();
        localPlayer.SetStrafeSpeed();
        localPlayer.SetJumpImpulse();
        localPlayer.SetGravityStrength();
    }
    */
}
