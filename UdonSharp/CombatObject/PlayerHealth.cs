using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PlayerHealth : UdonSharpBehaviour
{
    [SerializeField]
    private CapsuleCollider _capsuleCollider;

    [SerializeField]
    private Transform _respawnTransform;

    //Get:Bullet
    [SerializeField]
    public byte TeamId = 0;

    [SerializeField]
    private int _maxHealth = 100;

    [SerializeField]
    private int _startHealth = 100;

    [SerializeField]
    private float _respawnTime = 4f;

    [SerializeField]
    private float _invincibleTime = 4f;

    //Set:DamageManagement Get:Bullet,Explosion
    [HideInInspector]
    public ushort DamageableId;

    //Get:Bullet
    [HideInInspector]
    public int CurrentHealth;

    private int _accumulatedDamage;
    private int _recoveryHealth;
    private VRCPlayerApi _playerApi;
    private bool _absence = true;

    private int _playerId;

    public int SetPlayerId
    {
        set
        {
            _playerId = value;
            PlayerApiUpdate();
        }
    }

    private void PlayerApiUpdate()
    {
        _capsuleCollider.enabled = false;
        _absence = true;
        _playerApi = null;

        if (_playerId == 0) return;
        _playerApi = VRCPlayerApi.GetPlayerById(_playerId);
        if (_playerApi == null) return;

        _playerApi.CombatSetup();
        _playerApi.CombatSetRespawn(true, _respawnTime, _respawnTransform);
        _playerApi.CombatSetDamageGraphic(null);

        _capsuleCollider.enabled = true;
        _absence = false;
    }

    private void LateUpdate()
    {
        if (_absence) return;

        transform.position = _playerApi.GetPosition();
    }



    //Referenced by GroupData.Restart()
    #region
    public void Restart()
    {
        Debug.Log("Restart : PlayerHealth");

        CurrentHealth = _startHealth;
        _accumulatedDamage = 0;
        _recoveryHealth = 0;

        PlayerApiUpdate();
    }
    #endregion



    //Referenced by EnemyGun.FixedUpdate()
    #region
    public Vector3 GetMoveVelocityReturnValue;

    public void GetMoveVelocity()
    {
        GetMoveVelocityReturnValue = _playerApi.GetVelocity();
    }
    #endregion



    //Referenced by DamageManagement.FixedUpdate()
    #region
    public int OnDamageArgument_0;

    public void OnDamage()
    {
        _accumulatedDamage = OnDamageArgument_0;
        HealthUpdate();

        if (CurrentHealth <= 0)
        {
            Dead();
        }
    }

    private void Dead()
    {
        if (_absence) return;

        _playerApi.CombatSetCurrentHitpoints(0f);
        _capsuleCollider.enabled = false;
        SendCustomEventDelayedSeconds("Respawn", _respawnTime);
    }

    public void Respawn()
    {
        if (_absence) return;

        _recoveryHealth = _accumulatedDamage;
        HealthUpdate();
        SendCustomEventDelayedSeconds("ReleaseInvincibility", _invincibleTime);
    }

    public void ReleaseInvincibility()
    {
        if (_absence) return;

        _capsuleCollider.enabled = true;
    }

    private void HealthUpdate()
    {
        CurrentHealth = _startHealth + _recoveryHealth - _accumulatedDamage;
    }
    #endregion
}
