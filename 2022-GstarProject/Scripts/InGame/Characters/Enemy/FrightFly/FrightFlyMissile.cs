using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrightFlyMissile : NormalAttack
{
    [SerializeField] private float _missileSpeed;

    private Stat _stat;
    
    public void SetStat(Stat stat)
    {
        _stat = stat;
        DelayDisable();
    }
   
    private void FixedUpdate()
    {
        transform.Translate(Vector3.forward * _missileSpeed);
    }
    
    private void DelayDisable()
    {
        Invoke("DisableObject", 0.5f);
    }
    
    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            other.transform.GetComponent<Creature>().TryGetDamage(_stat, this);
            DisableObject();
        }
    }
    
}
