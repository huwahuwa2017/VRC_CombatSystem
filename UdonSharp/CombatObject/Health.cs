using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Health : UdonSharpBehaviour
{
    //Get:Bullet
    [SerializeField]
    public byte TeamId = 0;

    [SerializeField]
    private int _maxHealth = 100;

    [SerializeField]
    private int _startHealth = 100;

    //Set:DamageManagement Get:Bullet,Explosion
    [HideInInspector]
    public ushort DamageableId;

    //Get:Bullet
    [HideInInspector]
    public int CurrentHealth;



    //Referenced by GroupData.Restart()
    #region
    public void Restart()
    {
        Debug.Log("Restart : Health");

        CurrentHealth = _startHealth;
        gameObject.SetActive(true);
    }
    #endregion



    //Referenced by DamageManagement.FixedUpdate()
    #region
    public int OnDamageArgument_0;

    public void OnDamage()
    {
        CurrentHealth = _startHealth - OnDamageArgument_0;

        if (CurrentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }
    #endregion
}
