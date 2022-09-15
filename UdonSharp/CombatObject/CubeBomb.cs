using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class CubeBomb : UdonSharpBehaviour
{
    private GameObject _explosionObject;

    public override void Interact()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, "Detonation");
    }

    public void Detonation()
    {
        if (_explosionObject == null) return;

        _explosionObject.transform.position = transform.position;
        Explosion explosionUB = _explosionObject.GetComponent<Explosion>();
        explosionUB.Detonation();

        gameObject.SetActive(false);
    }



    //Referenced by GroupData.Restart()
    #region
    [SerializeField]
    private GameObject _explosivePrefab;

    [SerializeField]
    private DamageArraySync _damageArraySync;

    [SerializeField]
    private int _damage;

    [SerializeField]
    private float _explosionRadius;

    public void Restart()
    {
        Debug.Log("Restart : CubeButton_1");

        if (_explosionObject != null)
        {
            Destroy(_explosionObject);
            _explosionObject = null;
        }

        _explosionObject = VRCInstantiate(_explosivePrefab);

        Explosion explosionUB = _explosionObject.GetComponent<Explosion>();
        explosionUB.UdonDamageArraySync = _damageArraySync;
        explosionUB.Damage = _damage;
        explosionUB.ExplosionRadius = _explosionRadius;

        gameObject.SetActive(true);
    }
    #endregion
}
