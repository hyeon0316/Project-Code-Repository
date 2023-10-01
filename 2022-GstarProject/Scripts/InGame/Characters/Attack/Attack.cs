using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Attack : MonoBehaviour
{
    [SerializeField] private PoolType _curPoolType;
    
    /// <summary>
    /// 최종 데미지를 계산
    /// </summary>
    /// <returns></returns>
    public abstract int CalculateDamage(Stat stat);
    

    /// <summary>
    /// 풀링 오브젝트 사용 뒤 반환
    /// </summary>
    protected virtual void DisableObject()
    {
        ObjectPoolManager.Instance.ReturnObject(_curPoolType, this.gameObject);
    }

    
}
