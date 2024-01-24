using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    private Dictionary<int, PoolContainer> _poolList = new Dictionary<int, PoolContainer>();

    public void Init(GameObject obj, int initCount)
    {
        if (!_poolList.ContainsKey(obj.GetInstanceID()))
        {
            PoolContainer container = new PoolContainer(obj.name, this.transform);
            _poolList.Add(obj.GetInstanceID(), container);
        }

        _poolList[obj.GetInstanceID()].CreateNewObject(obj, initCount);
    }

    public GameObject GetObject(GameObject obj, bool isActive = true)
    {
        return _poolList[obj.GetInstanceID()].GetObject(obj, isActive);
    }

    public void ReturnObject(GameObject obj)
    {
        int id = obj.GetComponent<IPoolable>().InstanceId;
        _poolList[id].ReturnObject(obj);
    }
}
