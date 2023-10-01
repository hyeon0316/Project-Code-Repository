using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAttack : Attack
{
    [Header("스킬 공격퍼센트")]
    [SerializeField] private float _percentDamage;

    public override int CalculateDamage(Stat stat)
    {
        int resultDamage = (int)(stat.Attack * (_percentDamage * stat.SkillDamage * stat.AllDamge) / Mathf.Pow(100,3) * Random.Range(0.8f, 1.1f));
        // 최종 데미지 = 플레이어공격력 * _percentDamage * 스킬데미지% * 모든데미지% * 80%~100%
        
        return resultDamage;
    }
}
