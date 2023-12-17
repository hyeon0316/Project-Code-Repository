using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 풀링 할 오브젝트
/// </summary>
public enum PoolType
{
    NormalAttackMissile,
    NormalAttackEffect,
    WideAreaBarrage,
    BulletRainMissile,
    BulletRainEffect,
    DamageText,
    SnowFootPrint,
    FrightFlyMissile,
    WindAttack,
    VolcanicSpike,
    Spider,
    FrightFly,
    ForestGolem1,
    ForestGolem2,
    ForestGolem3,
    SpecialGolem,
    GoblinWarrior,
    GoblinArcher,
    GoblinArcherArrow,
    Goblin,
    Boss,
    BossRock,
}


public sealed class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    [SerializeField] private GameObject[] _objectPrefabs;

    private Dictionary<PoolType, Queue<GameObject>> _poolObjects = new Dictionary<PoolType, Queue<GameObject>>();
    private Dictionary<string, Transform> _parents = new Dictionary<string, Transform>();

    private void Awake()
    {
        Init(PoolType.NormalAttackMissile, 4);
        Init(PoolType.NormalAttackEffect, 4);
        Init(PoolType.WideAreaBarrage, 3);
        Init(PoolType.BulletRainMissile, 10);
        Init(PoolType.BulletRainEffect, 10);
        Init(PoolType.DamageText, 10);
        Init(PoolType.SnowFootPrint, 5);
        Init(PoolType.FrightFlyMissile, 5);
        Init(PoolType.WindAttack, 2);
        Init(PoolType.VolcanicSpike, 1);
        Init(PoolType.Spider, 10);
        Init(PoolType.FrightFly, 10);
        Init(PoolType.ForestGolem1, 10);
        Init(PoolType.ForestGolem2, 10);
        Init(PoolType.ForestGolem3, 10);
        Init(PoolType.SpecialGolem, 1);
        Init(PoolType.GoblinWarrior, 10);
        Init(PoolType.GoblinArcher, 10);
        Init(PoolType.GoblinArcherArrow, 10);
        Init(PoolType.Goblin, 10); 
        Init(PoolType.Boss, 1);
        Init(PoolType.BossRock, 1);
    }

    /// <summary>
    /// 원하는 poolType을 원하는 initCount만큼 생성
    /// </summary>
    private void Init(PoolType poolType, int initCount)
    {
        _poolObjects.Add(poolType, new Queue<GameObject>());

        for (int i = 0; i < initCount; i++)
        {
            _poolObjects[poolType].Enqueue(CreateNewObject(poolType));
        }
    }

    /// <summary>
    /// 부모 오브젝트를 만들어 여러 PoolType의 오브젝트를 관리
    /// </summary>
    private Transform CreateParentTr(PoolType poolType)
    {
        Transform parentTr;
        string parentName = poolType.ToString();
        _parents.TryGetValue(parentName, out parentTr);
        if (parentTr != null)
        {
            return parentTr;
        }
        var newParent = new GameObject(parentName);
        newParent.transform.SetParent(this.transform);
        _parents.Add(parentName, newParent.transform);
        return newParent.transform;
    }

    private GameObject CreateNewObject(PoolType poolType)
    {
        var newObj = Instantiate(_objectPrefabs[(int) poolType]);
        newObj.SetActive(false);
        newObj.transform.SetParent(CreateParentTr(poolType));
        return newObj;
    }

    /// <summary>
    /// 생성했던 오브젝트 사용
    /// </summary>
    public GameObject GetObject(PoolType poolType, bool isActive = true)
    {
        if (_poolObjects[poolType].Count > 0)
        {
            var obj = _poolObjects[poolType].Dequeue();
            obj.transform.SetParent(null);
            obj.SetActive(isActive);
            return obj;
        }
        else // 개수가 부족할 경우 새로 만들어서 사용
        {
            var newObj = CreateNewObject(poolType);
            newObj.SetActive(isActive);
            newObj.transform.SetParent(null);
            return newObj;
        }
    }

   /// <summary>
   /// 사용했던 오브젝트를 Queue에 다시 넣어둠
   /// </summary>
    public void ReturnObject(PoolType poolType, GameObject obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(CreateParentTr(poolType));
        _poolObjects[poolType].Enqueue(obj);
    }
}
