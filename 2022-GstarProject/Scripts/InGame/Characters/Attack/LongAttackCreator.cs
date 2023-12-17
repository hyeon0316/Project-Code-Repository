using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongAttackCreator : MonoBehaviour
{
    public void CreateLongAttack(PoolType poolType, Stat stat)
    {
        var longAttack = ObjectPoolManager.Instance.GetObject(poolType);
        longAttack.transform.position = this.transform.position;
        longAttack.transform.rotation = this.transform.rotation;

        longAttack.GetComponent<Attack>().SetStat(stat);
    }

    public T CreateLongAttack<T>(PoolType poolType) where T : LongAttack
    {
        T longAttack = ObjectPoolManager.Instance.GetObject(poolType).GetComponent<T>();
        longAttack.transform.position = this.transform.position;
        longAttack.transform.rotation = this.transform.rotation;

        return longAttack;
    }
}
