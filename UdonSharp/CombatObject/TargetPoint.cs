using UdonSharp;
using UnityEngine;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class TargetPoint : UdonSharpBehaviour
{
    [SerializeField]
    private Collider _targetCollider;

    [SerializeField]
    private UdonBehaviour _userUdonBehaviour;

    public Collider TargetCollider
    {
        get => _targetCollider;
    }

    public UdonBehaviour UserUdonBehaviour
    {
        get => _userUdonBehaviour;
    }
}
