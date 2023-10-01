using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : NormalAttack
{
    private Rigidbody _rigidbody;
    
    private Stat _stat;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void SetStat(Stat stat)
    {
        _stat = stat;
        _rigidbody.isKinematic = false;
        DelayDisable();
    }
    
    private void FixedUpdate()
    {
        Vector3 speed = new Vector3(100, 200, 100);
        _rigidbody.AddForce(speed);
    }
    
    private void DelayDisable()
    {
        Invoke("DisableObject", 1f);
    }

    protected override void DisableObject()
    {
        base.DisableObject();
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
