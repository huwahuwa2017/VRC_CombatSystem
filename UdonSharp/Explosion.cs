using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Explosion : UdonSharpBehaviour
{
    //C#ではconstはあまり使わないほうが良いのだが、U#ではstaticが使えないので仕方なくconstを使用している
    private const int _layer = 30;



    [SerializeField]
    private SphereCollider _explosionCollider;

    [SerializeField]
    private GameObject _explosionParticles;

    private DamageArraySync _udonDamageArraySync;
    private int _damage;
    private float _explosionRadius;
    private ulong _teamIdMask;
    private bool _detonation;

    public DamageArraySync UdonDamageArraySync
    {
        set => _udonDamageArraySync = value;
    }

    public int Damage
    {
        set => _damage = value;
    }

    public float ExplosionRadius
    {
        set => _explosionRadius = value;
    }

    public ulong TeamIdMask
    {
        set => _teamIdMask = value;
    }

    private void Start()
    {
        _explosionCollider.radius = _explosionRadius;
    }

    public void Detonation()
    {
        _explosionCollider.enabled = true;

        _explosionParticles.SetActive(true);
        _explosionParticles.transform.parent = null;
        _explosionParticles.transform.localScale = new Vector3(_explosionRadius, _explosionRadius, _explosionRadius);

        Destroy(_explosionParticles, 2f);
    }

    private void OnTriggerEnter(Collider collider)
    {
        _detonation = true;

        if (!Networking.IsOwner(Networking.LocalPlayer, _udonDamageArraySync.gameObject)) return;

        GameObject hitObject = collider.gameObject;
        if (hitObject.layer != _layer) return;

        UdonBehaviour damageableUB = (UdonBehaviour)hitObject.GetComponent(typeof(UdonBehaviour));
        if (damageableUB == null) return;

        byte targetTeamId = (byte)damageableUB.GetProgramVariable("TeamId");
        if (((_teamIdMask << targetTeamId) & 1u) == 1u) return;

        ushort id = (ushort)damageableUB.GetProgramVariable("DamageableId");
        _udonDamageArraySync.DamageDataAdd(id, _damage);
    }

    private void FixedUpdate()
    {
        if (!_detonation) return;

        _explosionCollider.enabled = false;
        Destroy(gameObject);
    }
}
