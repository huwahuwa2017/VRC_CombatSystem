using UdonSharp;
using UnityEngine;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class GroupData : UdonSharpBehaviour
{
    [SerializeField]
    private GameObject[] _resetArray;

    [SerializeField]
    private GameObject[] _damageableObjectArray;

    public GameObject[] DamageableObjectArray
    {
        get => _damageableObjectArray;
    }

    public void Restart()
    {
        foreach (GameObject go in _resetArray)
        {
            UdonBehaviour ub = (UdonBehaviour)go.GetComponent(typeof(UdonBehaviour));
            ub.SendCustomEvent("Restart");
        }
    }
}
