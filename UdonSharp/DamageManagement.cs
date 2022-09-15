using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DamageManagement : UdonSharpBehaviour
{
    private GroupData[] _newGroupDataArray;
    private GroupData[] _groupDataArray;
    private DamageArraySync[] _damageArraySyncArray;
    private GameObject[] _damageableObjectArray = new GameObject[0];
    private int _damageableObjectCount;
    private int[] _damageArray = new int[0];
    private bool _resetting;

    public void Restart(GroupData[] newGroupDataArray)
    {
        _resetting = true;

        _newGroupDataArray = newGroupDataArray;
        DamageableObjectArraySetting();
        DamageArraySyncSetting();
        _damageArray = new int[_damageableObjectCount];

        SendCustomEventDelayedSeconds("SyncWait", 2f);
    }

    private void DamageableObjectArraySetting()
    {
        int groupDataArrayLength = _newGroupDataArray.Length;
        GameObject[][] temp_0 = new GameObject[groupDataArrayLength][];
        int count = 0;

        for (int index_0 = 0; index_0 < groupDataArrayLength; ++index_0)
        {
            GameObject[] gameObjectArray = _newGroupDataArray[index_0].DamageableObjectArray;
            temp_0[index_0] = gameObjectArray;
            count += gameObjectArray.Length;
        }

        _damageableObjectArray = new GameObject[count];
        int index_1 = 0;

        foreach (GameObject[] goArray in temp_0)
        {
            foreach (GameObject go in goArray)
            {
                UdonBehaviour ub = (UdonBehaviour)go.GetComponent(typeof(UdonBehaviour));
                ub.SetProgramVariable("DamageableId", index_1);
                _damageableObjectArray[index_1] = go;
                ++index_1;
            }
        }

        _damageableObjectCount = _damageableObjectArray.Length;
    }

    private void DamageArraySyncSetting()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        int childCount = transform.childCount;
        _damageArraySyncArray = new DamageArraySync[childCount];

        for (int index = 0; index < childCount; ++index)
        {
            GameObject go = transform.GetChild(index).gameObject;
            DamageArraySync das = go.GetComponent<DamageArraySync>();
            _damageArraySyncArray[index] = das;

            if (Networking.IsOwner(localPlayer, go))
            {
                das.ResetDamageArray(_damageableObjectCount);
            }
        }
    }

    public void SyncWait()
    {
        if (_groupDataArray != null)
        {
            foreach (GroupData gd in _groupDataArray)
            {
                gd.gameObject.SetActive(false);
            }
        }

        _groupDataArray = _newGroupDataArray;

        foreach (GroupData gd in _groupDataArray)
        {
            gd.Restart();
            gd.gameObject.SetActive(true);
        }

        _resetting = false;
    }

    private void FixedUpdate()
    {
        if (_resetting) return;
        if (_damageArraySyncArray == null) return;

        int[] newDamageArray = new int[_damageableObjectCount];
        bool skip = false;

        foreach (DamageArraySync damageArraySync in _damageArraySyncArray)
        {
            int[] damageArray = damageArraySync.DamageArray;

            if (damageArray.Length != _damageableObjectCount)
            {
                skip = true;
                break;
            }

            for (uint index = 0; index < _damageableObjectCount; ++index)
            {
                newDamageArray[index] += damageArray[index];
            }
        }

        if (skip)
        {
            Debug.Log("DamageManagement.FixedUpdate() : Array length is not as expected");
            return;
        }

        for (uint index = 0; index < _damageableObjectCount; ++index)
        {
            if (newDamageArray[index] == _damageArray[index]) continue;

            UdonBehaviour ub = (UdonBehaviour)_damageableObjectArray[index].GetComponent(typeof(UdonBehaviour));
            ub.SetProgramVariable("OnDamageArgument_0", newDamageArray[index]);
            ub.SendCustomEvent("OnDamage");
        }

        _damageArray = newDamageArray;
    }
}
