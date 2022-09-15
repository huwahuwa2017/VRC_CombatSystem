using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class DamageArraySync : UdonSharpBehaviour
{
    [SerializeField]
    private int _syncFrameCount = 10;

    [UdonSynced]
    private int[] _damageArray = new int[0];

    private int _syncTime;

    public int[] DamageArray
    {
        get => _damageArray;
    }

    public void ResetDamageArray(int damageableObjectCount)
    {
        _damageArray = new int[damageableObjectCount];
    }

    public void DamageDataAdd(ushort hitId, int hitDamage)
    {
        _damageArray[hitId] += hitDamage;
    }

    private void FixedUpdate()
    {
        if (++_syncTime < _syncFrameCount) return;
        _syncTime = 0;

        if (Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            RequestSerialization();
        }
    }
}
