using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float dmg;
    public float speed;
    public GameObject explossionEffect;
    Vector3 dir;
    void Awake()
    {
        transform.LookAt(Player.inst.transform.position);
        dir = Player.inst.transform.position - transform.position;
        dir.y = 0;
    }
    private void Update()
    {
        transform.position += dir.normalized * Time.deltaTime* speed;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            GameObject impactP = Instantiate(explossionEffect, transform.position, transform.rotation) as GameObject;
            Debug.Log("[Player] , 데미지 :" + dmg);
            OnDamageEvent1(other.gameObject);
            Destroy(gameObject);
            Destroy(impactP, 2f);
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
