using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillE : AttackState
{

    float dmg;
    float startingDmg = 20f;
    void Awake()
    {
        dmg = (Player.inst.power/2) + startingDmg;
        SetDmg(dmg);
    }

    private void OnTriggerEnter(Collider other)
    {

        SoundManager.inst.SFXPlay("SkillE", SoundManager.inst.skList[2]);
        TagCheck(other, dmg);
    }
}
