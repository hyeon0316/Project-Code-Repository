using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scarecrow : Life,I_hp,I_EnemyControl
{

    public Enemystate Enemystate;

    public Enemystate _enemystate
    {
        get { return Enemystate; }
        set { Enemystate = value; }
    }

    public Animator animator;

    public bool Gethit(float Cvalue, float coefficient)
    {
        if (HP > 0)
        {
            if (Cvalue > 0)
            {

                animator.SetTrigger("Hit");

            }
            //HP -= Cvalue * coefficient;
            return CheckLiving();
        }
        return false;
    }

    public bool CheckLiving()
    {
        if (HP <= 0)
        {
            
            StartCoroutine(DeadAniPlayer());
            return true;
        }
        else
            return false;
    }


    public IEnumerator DeadAniPlayer()
    {
        yield return 0;
    }

    public void EnemyAttack(float coefficient)
    {

    }

     public void EnemyMove()
    {

    }

     public void SelectHit(AttackHitSoundType type)
     {
         switch (type)
         {
             case AttackHitSoundType.ZHit1:
                 FindObjectOfType<SoundManager>().Play("Player/ZAttackHit1",SoundType.Effect);
                 break;
             case AttackHitSoundType.ZHit2:
                 FindObjectOfType<SoundManager>().Play("Player/ZAttackHit2",SoundType.Effect);
                 break;
             case AttackHitSoundType.XHit:
                 FindObjectOfType<SoundManager>().Play("Player/XAttackHit",SoundType.Effect);
                 break;
             case AttackHitSoundType.AHit:
                 FindObjectOfType<SoundManager>().Play("Player/BulletHit",SoundType.Effect);
                 break;
             case AttackHitSoundType.SHit:
                 FindObjectOfType<SoundManager>().Play("Player/DashAttackHit",SoundType.Effect);
                 break;
         }
     }

    // Start is called before the first frame update
    void Start()
    {
        Initdata(99999, 10000, 0, 0);
        Enemystate = Enemystate.Idle;
        animator = this.GetComponentInChildren<Animator>();
        GameObject.Find("UICanvas").transform.Find("TalkPanel").gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        GameObject.Find("UICanvas").transform.Find("TalkPanel").transform.position =
            this.transform.position + new Vector3(0.7f, 1.2f, 0);
        if(GetComponent<UnityEngine.AI.NavMeshAgent>().enabled)
            GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
}
