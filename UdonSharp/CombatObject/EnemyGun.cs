using UdonSharp;
using UnityEngine;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class EnemyGun : UdonSharpBehaviour
{
    [SerializeField]
    private Gun _udonGun;

    [SerializeField]
    private TargetPoint[] _targetPointArray;

    private int _preTargetIndex = -1;
    private float _bulletSpeed = 1000f;
    private float _targetDistance;

    private void FixedUpdate()
    {
        float smd = float.MaxValue;
        int targetIndex = -1;
        Vector3 myPos = transform.position;
        Vector3 targetDirection = Vector3.forward;

        for (int index = 0; index < _targetPointArray.Length; index++)
        {
            TargetPoint targetPoint = _targetPointArray[index];
            Collider temp_4 = targetPoint.TargetCollider;
            if (!temp_4.enabled) continue;

            Vector3 temp_0 = targetPoint.transform.position - myPos;
            float temp_1 = temp_0.sqrMagnitude;

            if (temp_1 < smd)
            {
                targetDirection = temp_0;
                targetIndex = index;
                smd = temp_1;
            }
        }

        if (targetIndex == -1) return;

        if (targetIndex != _preTargetIndex)
        {
            _preTargetIndex = targetIndex;
            _targetDistance = targetDirection.magnitude;
        }

        UdonBehaviour userUdonBehaviour = _targetPointArray[targetIndex].UserUdonBehaviour;
        userUdonBehaviour.SendCustomEvent("GetMoveVelocity");
        Vector3 moveVelocity = (Vector3)userUdonBehaviour.GetProgramVariable("GetMoveVelocityReturnValue");

        float arrivalTime = _targetDistance / _bulletSpeed;

        targetDirection += moveVelocity * arrivalTime;
        _targetDistance = targetDirection.magnitude;

        transform.rotation = Quaternion.LookRotation(targetDirection, transform.parent.up);

        if (_targetDistance <= 200f)
        {
            _udonGun.Trigger = true;

            float t = Random.Range(-0.05f, 0.05f);
            _udonGun.EffectiveTime = arrivalTime + t;
        }
        else
        {
            _udonGun.Trigger = false;
        }
    }



    //Referenced by GroupData.Restart()
    #region
    public void Restart()
    {
        Debug.Log("Restart : EnemyGun");

        _preTargetIndex = -1;
        _bulletSpeed = _udonGun.BulletSpeed;
    }
    #endregion



    //Referenced by Gun.FixedUpdate()
    #region
    public Vector3 GetMoveVelocityReturnValue;

    public void GetMoveVelocity()
    {
        GetMoveVelocityReturnValue = Vector3.zero;
    }
    #endregion
}
