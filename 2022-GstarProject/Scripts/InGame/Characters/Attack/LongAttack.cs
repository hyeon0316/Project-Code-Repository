using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongAttack : Attack
{
    [SerializeField] private PoolType _curPoolType;

    protected virtual void CallDisableEvent()
    {
        ObjectPoolManager.Instance.ReturnObject(_curPoolType, this.gameObject);
    }
}
