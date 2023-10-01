using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillQ : AttackState
{
    // Start is called before the first frame update
    
    float dmg;
    float startingDmg = 100f;
    
    void Awake()
    {
        dmg = Player.inst.power + startingDmg;
    }

    private void OnTriggerEnter(Collider other)
    {
        TagCheck(other, dmg);
    }
}
