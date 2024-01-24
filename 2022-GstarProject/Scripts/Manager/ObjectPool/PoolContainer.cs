using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolContainer
{
    private Queue<GameObject> _poolObjects = new Queue<GameObject>();
    private Transform _containerTr;

    public PoolContainer(string name, Transform parentTr)
    {
        _containerTr = CreateParentTr(name, parentTr);
    }
 
    public void CreateNewObject(GameObject obj, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var newObj = GameObject.Instantiate(obj);
            newObj.SetActive(false);
            newObj.transform.SetParent(_containerTr);
            newObj.GetComponent<IPoolable>().InstanceId = obj.GetInstanceID();
            _poolObjects.Enqueue(newObj);
        }
    }

    public GameObject GetObject(GameObject obj, bool isActive)
    {
        if (_poolObjects.Count == 0)
            CreateNewObject(obj, 1);

        var poolingObj = _poolObjects.Dequeue();
        poolingObj.transform.SetParent(null);
        poolingObj.SetActive(isActive);
        return poolingObj;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(_containerTr);
        _poolObjects.Enqueue(obj);
    }


    private Transform CreateParentTr(string name, Transform parentTr)
    {
        var newParent = new GameObject(name);
        newParent.transform.SetParent(parentTr);
        return newParent.transform;
    }
}
