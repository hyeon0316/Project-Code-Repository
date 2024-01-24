using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongAttackCreator : MonoBehaviour
{
    public void CreateLongAttack(GameObject longAttackObj, Stat stat)
    {
        var longAttack = ObjectPoolManager.Instance.GetObject(longAttackObj);
        longAttack.transform.position = this.transform.position;
        longAttack.transform.rotation = this.transform.rotation;

        longAttack.GetComponent<Attack>().SetStat(stat);
    }

    public T CreateLongAttack<T>(GameObject longAttackObj) where T : LongAttack
    {
        T longAttack = ObjectPoolManager.Instance.GetObject(longAttackObj).GetComponent<T>();
        longAttack.transform.position = this.transform.position;
        longAttack.transform.rotation = this.transform.rotation;

        return longAttack;
    }
}
