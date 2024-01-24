using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongAttack : Attack, IPoolable
{
    public int InstanceId { get; set; }

    protected virtual void CallDisableEvent()
    {
        ObjectPoolManager.Instance.ReturnObject(this.gameObject);
    }
}
