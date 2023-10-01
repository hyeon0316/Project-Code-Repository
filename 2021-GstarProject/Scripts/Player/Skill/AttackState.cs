using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : MonoBehaviour
{
    // Start is called before the first frame update

    public float dmg1;
    public void TagCheck(Collider other,float dmg)
    {
        SetDmg(dmg);
        if (other.tag == "Enemy")
        {
            
            Debug.Log("[Enemy] , 데미지 :" + dmg);
            OnDamageEvent(other.gameObject);
        }
        if (other.tag == "Boss")
        {
            Debug.Log("[BOss] , 데미지 :" + dmg);
            OnDamageEvent(other.gameObject);
        }
    }
    public void SetDmg(float dmg)
    {
        dmg1 = dmg;
    }
    public void OnDamageEvent(GameObject targetEntity)
    {
        
        //공격 대상을 지정할 추적 대상의 LivingEntity 컴포넌트 가져오기
        LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();

        //공격 처리(플레이어에게)
        attackTarget.OnDamage(dmg1);
    }
}
