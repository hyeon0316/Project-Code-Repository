using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stat : MonoBehaviour
{
    public Text hp;
    public Text mp;
    public Text power;
    public Text dp;

    // Update is called once per frame
    void Update()
    {
        hp.text = string.Format("체력: {0}", Player.inst.startingHealth);
        mp.text = string.Format("마나: {0}", Player.inst.startingMana);
        power.text = string.Format("마력: {0}", Player.inst.power);
        dp.text = string.Format("방어력: {0}", Player.inst.dP);
    }
}
