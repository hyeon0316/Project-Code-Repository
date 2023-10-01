using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongAttack : MonoBehaviour
{
    public void CreateProjectile(PoolType poolType, Stat stat)
    {
        var projectile = ObjectPoolManager.Instance.GetObject(poolType);
        projectile.transform.position = transform.position;
        projectile.transform.rotation = transform.rotation;

        switch (poolType)
        {
            case PoolType.FrightFlyMissile:
                projectile.GetComponent<FrightFlyMissile>().SetStat(stat);
                break;
            case PoolType.GoblinArcherArrow:
                projectile.GetComponent<GoblinArcherArrow>().SetStat(stat);
                break;
            case PoolType.BossRock:
                projectile.GetComponent<Rock>().SetStat(stat);
                break;
        }
    }
}
