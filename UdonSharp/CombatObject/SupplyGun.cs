using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SupplyGun : UdonSharpBehaviour
{
    [SerializeField]
    private SupplyGunDataSync _udonSupplyGunDataSync;

    public override void OnPickup()
    {
        _udonSupplyGunDataSync.Pickup();
    }

    public override void OnPickupUseDown()
    {
        _udonSupplyGunDataSync.PickupUseDown();
    }

    public override void OnPickupUseUp()
    {
        _udonSupplyGunDataSync.PickupUseUp();
    }



    //Referenced by GroupData.Restart()
    #region
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private bool _initialized;

    public void Restart()
    {
        Debug.Log("Restart : SupplyGun");

        Initialize();

        transform.position = _startPosition;
        transform.rotation = _startRotation;
    }

    private void Initialize()
    {
        if (_initialized) return;

        _startPosition = transform.position;
        _startRotation = transform.rotation;

        _initialized = true;
    }
    #endregion
}

