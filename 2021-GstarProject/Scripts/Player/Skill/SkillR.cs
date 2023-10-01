using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillR : AttackState
{
    float dmg;
    float startingDmg = 200f;
    void Awake()
    {
        dmg = (Player.inst.power*5) + startingDmg;
        SetDmg(dmg);
    }

    private void OnTriggerEnter(Collider other)
    {
        TagCheck(other, dmg);
    }
}
