using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTrap : BossAttackState
{
    // Start is called before the first frame update

    float dmg;
    public float startingDmg = 100f;
    void Awake()
    {
        dmg = startingDmg;
    }

    private void OnTriggerEnter(Collider other)
    {
        TagCheck(other, dmg);
    }
}
