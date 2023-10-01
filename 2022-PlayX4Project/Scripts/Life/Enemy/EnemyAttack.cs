using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private bool _isHitPlayer;

    public bool IshitPlayer
    {
        get { return _isHitPlayer; }
    }

    /// <summary>
    /// 이벤트 함수로 실행
    /// </summary>
   public void Attackhit(float coefficient)
    {
        Debug.Log("공격에니메이션실행");
        this.transform.parent.GetComponent<I_EnemyControl>().EnemyAttack(coefficient);
    }

    public void RangedAttack()
    {
        FindObjectOfType<Demon>().LaunchFireBall();
    }

    public void ThisGethit(float Cvalue)
    {
        float Beforehp = this.transform.parent.GetComponent<Life>().HpRatio;
        if (this.transform.parent.GetComponent<I_hp>().Gethit(Cvalue,1))
        {
            //회복이라서 true 를 탈꺼 같지는 않은데 만약 채력을 소모해서 사용하는 과정에서 자살이라도 한다면 사용
            GameObject.Find("Canvas(Enemy)").GetComponent<EnemyHpbar>().SwitchHPbar(this.transform.parent.GetComponent<Life>().LifeId, this.transform.parent.GetComponent<Life>().HpRatio, Beforehp,true);
        }
        else
        {
            GameObject.Find("Canvas(Enemy)").GetComponent<EnemyHpbar>().SwitchHPbar(this.transform.parent.GetComponent<Life>().LifeId, this.transform.parent.GetComponent<Life>().HpRatio, Beforehp);
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            _isHitPlayer = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            _isHitPlayer = false;
        }
    }
}
