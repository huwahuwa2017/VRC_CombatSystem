using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Gun : UdonSharpBehaviour
{
    [SerializeField]
    private Transform _grip;

    [SerializeField]
    private GameObject _bulletPrefab;

    [SerializeField]
    private GameObject _explosivePrefab;

    [SerializeField]
    private DamageArraySync _udonDamageArraySync;

    [SerializeField]
    private UdonBehaviour _userUdonBehaviour;

    [SerializeField]
    private ulong _teamIdMask;

    [SerializeField]
    private GameObject[] _gameObjectMask;

    [SerializeField]
    private float _firingInterval = 0.1f;

    [SerializeField]
    private float _scatteringAngle = 1f;

    [SerializeField]
    private float _bulletSpeed = 100f;

    [SerializeField]
    private int _damage = 10;

    [SerializeField]
    private bool _penetration = false;

    [SerializeField]
    private float _effectiveTime = 5f;

    [SerializeField]
    private float _explosionRadius = 0f;

    private AudioSource _audioSource;
    private AudioClip _audioClip_shot;
    private float _elapsedTime;

    public float FiringInterval
    {
        get => _firingInterval;
        set => _firingInterval = value;
    }

    public float ScatteringAngle
    {
        get => _scatteringAngle;
        set => _scatteringAngle = value;
    }

    public float BulletSpeed
    {
        get => _bulletSpeed;
        set => _bulletSpeed = value;
    }

    public int Damage
    {
        get => _damage;
        set => _damage = value;
    }

    public bool Penetration
    {
        get => _penetration;
        set => _penetration = value;
    }

    public float EffectiveTime
    {
        get => _effectiveTime;
        set => _effectiveTime = value;
    }

    public float ExplosionRadius
    {
        get => _explosionRadius;
        set => _explosionRadius = value;
    }

    public DamageArraySync UdonDamageArraySync
    {
        set => _udonDamageArraySync = value;
    }

    public GameObject[] GameObjectMask
    {
        set => _gameObjectMask = value;
    }



    [UdonSynced]
    private bool _trigger;

    public bool Trigger
    {
        set
        {
            if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;

            _trigger = value;
            RequestSerialization();
        }
    }



    private void FixedUpdate()
    {
        _elapsedTime -= Time.deltaTime;

        if (!_trigger)
        {
            _elapsedTime = Mathf.Max(_elapsedTime, 0f);
            return;
        }

        if (_elapsedTime > 0) return;

        _userUdonBehaviour.SendCustomEvent("GetMoveVelocity");
        Vector3 moveVelocity = (Vector3)_userUdonBehaviour.GetProgramVariable("GetMoveVelocityReturnValue");

        while (_elapsedTime <= 0)
        {
            _elapsedTime += _firingInterval;
            GenerateBullet(moveVelocity);
        }
    }

    public void GenerateBullet(Vector3 moveVelocity)
    {
        _audioSource.PlayOneShot(_audioClip_shot);

        float rad_0 = Random.Range(-_scatteringAngle, _scatteringAngle) * Mathf.Deg2Rad;
        float rad_1 = Random.Range(-180f, 180f) * Mathf.Deg2Rad;

        float tempSin = Mathf.Sin(rad_0);
        Vector3 temp_0 = new Vector3(Mathf.Sin(rad_1) * tempSin, Mathf.Cos(rad_1) * tempSin, Mathf.Cos(rad_0));
        Vector3 bulletVelocity = (transform.rotation * temp_0) * _bulletSpeed + moveVelocity;

        GameObject newBulletObject = VRCInstantiate(_bulletPrefab);
        newBulletObject.transform.SetParent(transform, false);

        Bullet udonBullet = newBulletObject.GetComponent<Bullet>();
        udonBullet.UdonDamageArraySync = _udonDamageArraySync;
        udonBullet.Damage = _damage;
        udonBullet.Penetration = _penetration;
        udonBullet.EffectiveTime = _effectiveTime;
        udonBullet.VelocityPerFrame = bulletVelocity * Time.deltaTime;
        udonBullet.TeamIdMask = _teamIdMask;
        udonBullet.GameObjectMask = _gameObjectMask;

        if (_explosionRadius <= 0f) return;

        GameObject newExplosiveObject = VRCInstantiate(_explosivePrefab);

        Explosion udonExplosion = newExplosiveObject.GetComponent<Explosion>();
        udonExplosion.UdonDamageArraySync = _udonDamageArraySync;
        udonExplosion.Damage = _damage;
        udonExplosion.ExplosionRadius = _explosionRadius;
        udonExplosion.TeamIdMask = _teamIdMask;

        udonBullet.UdonExplosion = udonExplosion;
    }



    //Referenced by GroupData.Restart()
    #region
    private bool _initialized;

    public void Restart()
    {
        Debug.Log("Restart : Gun");

        Initialize();

        _trigger = false;
        _elapsedTime = 0f;
    }

    private void Initialize()
    {
        if (_initialized) return;

        _audioSource = gameObject.GetComponent<AudioSource>();
        _audioClip_shot = _audioSource.clip;

        _initialized = true;
    }
    #endregion
}
