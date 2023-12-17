using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    protected Stat _stat;

    /// <summary>
    /// 최종 데미지를 계산
    /// </summary>
    /// <returns></returns>
    public virtual int CalculateDamage(Stat stat)
    {
        int resultDamage = stat.Attack * stat.AllDamge / 100;
        resultDamage = (int)(resultDamage * Random.Range(0.8f, 1.1f));
        return resultDamage;
    }


    public virtual void SetStat(Stat stat)
    {
        _stat = stat;
    }
}
