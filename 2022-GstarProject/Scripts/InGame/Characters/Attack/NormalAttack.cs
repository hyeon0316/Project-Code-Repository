using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalAttack : Attack
{
   

    public override int CalculateDamage(Stat stat)
    {
        int resultDamage = stat.Attack * stat.AllDamge / 100 ;
        resultDamage = (int)(resultDamage * Random.Range(0.8f, 1.1f));
        return resultDamage;
    }
}
