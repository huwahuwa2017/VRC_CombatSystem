using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Bullet : UdonSharpBehaviour
{
    //C#ではconstはあまり使わないほうが良いのだが、U#ではstaticが使えないので仕方なくconstを使用している
    private const int _layer = 30;
    private const int _layerMask = 1 << _layer;



    [SerializeField]
    private GameObject _mesh;

    [SerializeField]
    private GameObject _trail;

    [SerializeField]
    private GameObject _hitParticles;

    private DamageArraySync _udonDamageArraySync;
    private Explosion _udonExplosion;
    private int _damage;
    private bool _penetration;
    private float _effectiveTime;
    private Vector3 _velocityPerFrame;
    private ulong _teamIdMask;
    private GameObject[] _gameObjectMask;

    private bool _explosive;
    private float _rayDistance;

    public DamageArraySync UdonDamageArraySync
    {
        set => _udonDamageArraySync = value;
    }

    public Explosion UdonExplosion
    {
        set => _udonExplosion = value;
    }

    public int Damage
    {
        set => _damage = value;
    }

    public bool Penetration
    {
        set => _penetration = value;
    }

    public float EffectiveTime
    {
        set => _effectiveTime = value;
    }

    public Vector3 VelocityPerFrame
    {
        set => _velocityPerFrame = value;
    }

    public ulong TeamIdMask
    {
        set => _teamIdMask = value;
    }

    public GameObject[] GameObjectMask
    {
        set => _gameObjectMask = value;
    }

    private void Start()
    {
        _explosive = _udonExplosion != null;
        _rayDistance = _velocityPerFrame.magnitude;

        transform.parent = null;

        _mesh.SetActive(true);
        _trail.SetActive(true);
    }

    private void FixedUpdate()
    {
        if (_effectiveTime <= 0f)
        {
            if (_explosive)
            {
                Explosive();
                return;
            }

            DestroyBullet(false);
            return;
        }

        _effectiveTime -= Time.deltaTime;



        Ray ray = new Ray(transform.position, _velocityPerFrame);
        RaycastHit[] raycastHitArray = Physics.RaycastAll(ray, _rayDistance, _layerMask);
        RaycastHitArraySort(raycastHitArray);

        foreach (RaycastHit raycastHit in raycastHitArray)
        {
            GameObject hitObject = raycastHit.collider.gameObject;
            UdonBehaviour damageableUB = (UdonBehaviour)hitObject.GetComponent(typeof(UdonBehaviour));

            if (RaycastHitMask(hitObject, damageableUB)) continue;

            if (_explosive)
            {
                transform.position = raycastHit.point;
                Explosive();
                return;
            }

            DamageCalculation(damageableUB);

            if (_damage <= 0f)
            {
                transform.position = raycastHit.point;
                DestroyBullet(true);
                return;
            }
        }

        transform.position += _velocityPerFrame;
    }

    private void Explosive()
    {
        if (_udonExplosion == null) return;

        _udonExplosion.transform.position = transform.position;
        _udonExplosion.Detonation();

        Destroy(gameObject);
    }

    private void DestroyBullet(bool hit)
    {
        if (hit)
        {
            _hitParticles.SetActive(true);
            _hitParticles.transform.parent = null;
            Destroy(_hitParticles, 3f);
        }

        if (_trail != null)
        {
            _trail.transform.parent = null;
        }

        Destroy(gameObject);
    }

    private void RaycastHitArraySort(RaycastHit[] raycastHitArray)
    {
        if (raycastHitArray.Length == 0) return;

        int arrayLength = raycastHitArray.Length;
        int minIndex = 0;
        RaycastHit temp_1;

        for (int startIndex = 0; startIndex < arrayLength - 1; ++startIndex)
        {
            float min = float.MaxValue;

            for (int index = startIndex; index < arrayLength; ++index)
            {
                float temp_0 = raycastHitArray[index].distance;

                if (temp_0 < min)
                {
                    min = temp_0;
                    minIndex = index;
                }
            }

            if (minIndex != startIndex)
            {
                temp_1 = raycastHitArray[startIndex];
                raycastHitArray[startIndex] = raycastHitArray[minIndex];
                raycastHitArray[minIndex] = temp_1;
            }
        }
    }

    private bool RaycastHitMask(GameObject hitObject, UdonBehaviour damageableUB)
    {
        if (damageableUB == null) return false;

        byte targetTeamId = (byte)damageableUB.GetProgramVariable("TeamId");
        if (((_teamIdMask << targetTeamId) & 1u) == 1u) return true;

        foreach (GameObject go in _gameObjectMask)
        {
            if (hitObject == go) return true;
        }

        return false;
    }

    private void DamageCalculation(UdonBehaviour damageableUB)
    {
        if (damageableUB == null)
        {
            //Debug.Log("Indestructible Hit");
            _damage = 0;
            return;
        }

        if (Networking.IsOwner(Networking.LocalPlayer, _udonDamageArraySync.gameObject))
        {
            ushort id = (ushort)damageableUB.GetProgramVariable("DamageableId");
            _udonDamageArraySync.DamageDataAdd(id, _damage);
        }

        if (!_penetration)
        {
            _damage = 0;
            return;
        }

        int health = (int)damageableUB.GetProgramVariable("CurrentHealth");
        _damage = Mathf.Max(_damage - health, 0);
    }
}
