using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GoblinArcherArrow : LongAttack
{
    [SerializeField] private float _speed;

    private void Start()
    {
        Invoke(nameof(CallDisableEvent), 1f);
    }

    private void FixedUpdate()
    {
        transform.Translate(Vector3.forward * _speed);
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
