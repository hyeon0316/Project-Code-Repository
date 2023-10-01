using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackState : MonoBehaviour
{
    public AudioClip skillSound;
    public float dmg1;
    public void TagCheck(Collider other, float dmg)
    {
        SetDmg(dmg);
        if (other.tag == "Player")
        {
            if(skillSound!=null)
                SoundManager.inst.SFXPlay(skillSound.name, skillSound);
            Debug.Log("[Player] , 데미지 :" + dmg);
            OnDamageEvent(other.gameObject);
            Destroy(this.gameObject); //이펙트가 사라져도 계속 데미지 받는 것 방지
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
