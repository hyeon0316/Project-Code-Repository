using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.AI;
using Object = System.Object;

public class Spider : Enemy
{
    [SerializeField] private ShortAttack _shortAttackArea;
    [SerializeField] private BoxCollider _attackCollider;


    protected override void OnEnable()
    {
        base.OnEnable();
        _attackCollider.enabled = false;
    }
    
    protected override void Start()
    {
        base.Start();
        _shortAttackArea.SetStat(Stat);
    }

    protected override void Attack()
    {
        _isAttack = true;
        _animator.SetInteger(Global.EnemyStateInteger, 2);
    }

    public void ActiveAttackCollider()
    {
        _attackCollider.enabled = true;
    }
    
    public void InActiveAttackCollider()
    {
        _attackCollider.enabled = false;
        _isAttack = false;
    }
}
