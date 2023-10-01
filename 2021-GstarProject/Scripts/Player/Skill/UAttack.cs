using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UAttack : AttackState
{
    float dmg;
    float startingDmg = 20f;
    void Awake()
    {
        dmg = Player.inst.power + startingDmg;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {

            Debug.Log("[Enemy] , 데미지 :" + dmg);
            OnDamageEvent1(other.gameObject);
        }
        if (other.tag == "Boss")
        {
            Debug.Log("[BOss] , 데미지 :" + dmg);
            OnDamageEvent1(other.gameObject);
        }
    }
    public void OnDamageEvent1(GameObject targetEntity)
    {

        //공격 대상을 지정할 추적 대상의 LivingEntity 컴포넌트 가져오기
        LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();

        //공격 처리(플레이어에게)
        attackTarget.OnDamage(dmg);
    }
}
