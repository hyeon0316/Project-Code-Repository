using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortAttack : NormalAttack
{
    private Stat _stat;

    public void SetStat(Stat stat)
    {
        _stat = stat;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            other.GetComponent<Creature>().TryGetDamage(_stat, this);
        }
    }
}
