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
    WindAttackEffect,
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
    None
}

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    [SerializeField] private GameObject[] _objectPrefabs;

    private Dictionary<PoolType, Queue<GameObject>> _poolObjects = new Dictionary<PoolType, Queue<GameObject>>();

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
        Init(PoolType.WindAttackEffect, 3);
        Init(PoolType.VolcanicSpike, 1);
        Init(PoolType.Spider, 15);
        Init(PoolType.FrightFly, 15);
        Init(PoolType.ForestGolem1, 40);
        Init(PoolType.ForestGolem2, 40);
        Init(PoolType.ForestGolem3, 40);
        Init(PoolType.SpecialGolem, 1);
        Init(PoolType.GoblinWarrior, 25);
        Init(PoolType.GoblinArcher, 25);
        Init(PoolType.GoblinArcherArrow, 20);
        Init(PoolType.Goblin, 25);
        Init(PoolType.Boss, 1);
        Init(PoolType.BossRock, 1);
    }

    /// <summary>
    /// 원하는 poolType을 원하는 initCount만큼 생성
    /// </summary>
    private void Init(PoolType poolType, int initCount)
    {
        if(!_poolObjects.ContainsKey(poolType))
            _poolObjects.Add(poolType, new Queue<GameObject>());
        
        for (int i = 0; i < initCount; i++)
        {
            _poolObjects[poolType].Enqueue(CreateNewObject(poolType));
        }
    }

    /// <summary>
    /// 부모 오브젝트를 만들어 여러 PoolType의 오브젝트를 관리
    /// </summary>
    private GameObject CreateParent(PoolType poolType)
    {
        if (!transform.Find(poolType.ToString()))
        {
            var newParent = new GameObject(poolType.ToString());
            newParent.transform.SetParent(transform);
            return newParent;
        }
        else //이미 부모오브젝트가 생성되어 있는 경우
        {
            var parent = transform.Find(poolType.ToString()).gameObject;
            parent.transform.SetParent(transform);
            return parent;
        }
    }
    
    private GameObject CreateNewObject(PoolType poolType)
    {
        var newObj = Instantiate(_objectPrefabs[(int) poolType]);
        newObj.SetActive(false);
        newObj.transform.SetParent(CreateParent(poolType).transform);
        return newObj;
    }

    /// <summary>
    /// 생성했던 오브젝트 사용
    /// </summary>
    public GameObject GetObject(PoolType poolType)
    {
        if (_poolObjects[poolType].Count > 0)
        {
            var obj = _poolObjects[poolType].Dequeue();
            obj.transform.SetParent(null);
            obj.SetActive(true);
            return obj;
        }
        else // 개수가 부족할 경우 새로 만들어서 사용
        {
            var newObj = CreateNewObject(poolType);
            newObj.SetActive(true);
            newObj.transform.SetParent(null);
            return newObj;
        }
    }
    
   /// <summary>
   /// 사용했던 오브젝트를 Queue에 다시 넣어둠
   /// </summary>
   /// <param name="poolType">오브젝트를 관리하는 Queue</param>
   /// <param name="obj">사용했던 오브젝트</param>
    public void ReturnObject(PoolType poolType, GameObject obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(CreateParent(poolType).transform);
        _poolObjects[poolType].Enqueue(obj);
    }
}
