using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Rock : LongAttack
{
    [SerializeField] private Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody.isKinematic = false;
        Invoke(nameof(CallDisableEvent), 1f);
    }

    private void FixedUpdate()
    {
        Vector3 speed = new Vector3(100, 200, 100);
        _rigidbody.AddForce(speed);
    }
    
    protected override void CallDisableEvent()
    {
        base.CallDisableEvent();
        _rigidbody.isKinematic = true;
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            other.transform.GetComponent<Creature>().TryGetDamage(_stat, this);
        }
    }
}
