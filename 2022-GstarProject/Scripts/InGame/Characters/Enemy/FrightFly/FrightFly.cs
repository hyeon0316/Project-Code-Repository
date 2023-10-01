using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrightFly : Enemy
{
    [SerializeField] private LongAttack _attackPos;

    protected override void Attack()
    {
        StartCoroutine(AttackCo());
    }

    private IEnumerator AttackCo()
    {
        _isAttack = true;
        yield return new WaitForSeconds(1f);
        _animator.SetInteger(Global.EnemyStateInteger,2);
        yield return new WaitForSeconds(0.5f);
        _isAttack = false;
    }

    public void CreateMissile()
    {
        _attackPos.CreateProjectile(PoolType.FrightFlyMissile, Stat);
    }
    
}
