using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class FrightFlyMissile : LongAttack
{
    [SerializeField] private float _missileSpeed;
   
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
            CallDisableEvent();
        }
    }
    
}
