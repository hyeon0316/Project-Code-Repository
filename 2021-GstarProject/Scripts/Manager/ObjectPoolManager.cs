using UnityEngine;
using System.Collections.Generic;

// 개체 풀의 기본 속성
// 현재 개체 풀의 이름, 프리 팹 및 크기를 저장
[System.Serializable]
public class ObjectPool
{
    public string Name;         // Pool Name
    public GameObject prefab;   // 생성할 오브젝트 프리팹
    public int Size;            // 풀의 크기
}

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager inst;
    // ObjectPool 목록    
    public List<ObjectPool> ObjectPoolList;
    // 이름으로 모든 개체 풀을 저장
    public Dictionary<string, Queue<GameObject>> enemyObjectPool;

    // 개체 풀을 초기화합니다.여기서 개체는 인스턴스화되고 각 개체 풀에 저장됨
    private void Awake()
    {
        if (!inst) // 싱글톤
        {
            inst = this;
        }
        // 풀 초기화
        enemyObjectPool = new Dictionary<string, Queue<GameObject>>();

        // Inspector창에서 받아온 정보를 풀에 대입시켜주는 작업
        
        foreach (ObjectPool pool in ObjectPoolList)
        {
            //  각 개체 풀에 대해 빈 부모 개체를 만듭니다. DontDestroyOnLad에 생성             
            GameObject poolParentObj = new GameObject();
            poolParentObj.name = pool.Name + "Pool";

            //DontDestroyOnLoad(poolParentObj);
            // 개체 풀을 만들고 여기에 개체 저장
            Queue<GameObject> poolQueue = new Queue<GameObject>();
            for (int i = 0; i < pool.Size; i++)
            {
                GameObject obj = GameObject.Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.transform.parent = poolParentObj.transform;

                // 큐에 추가
                poolQueue.Enqueue(obj);
            }
            // 개체 풀 추가  
            enemyObjectPool.Add(pool.Name, poolQueue);
        }
    }
    // 개체 생성 함수 (꺼내 쓰기)  개체 풀에서 개체 가져 오기 개체의 위치 및 회전을 설정.        
    // 생성 예제ex). ObjectPoolManager.GetObjectFromPool("Name", Vector3.Position , Quaternion.identity);
    //                                                                  ("이름", 위치, 회전값);
    public GameObject GetObjectFromPool(string _poolName, Vector3 _position, Quaternion _rotation) // 이름, 위치, 회전값
    {
        if (enemyObjectPool.ContainsKey(_poolName))
        {
            if (enemyObjectPool[_poolName].Count > 0)
            {
                GameObject obj = enemyObjectPool[_poolName].Dequeue();
                obj.SetActive(true);
                obj.transform.position = _position;
                obj.transform.rotation = _rotation;

                return obj;
            }
            else
            {
                Debug.Log(_poolName + "더 이상 없음");
            }
        }
        else
        {
            Debug.Log(_poolName + " 의 오브젝트 풀을 사용할 수 없음. (1# 이름이 다를 수 있음. 2#풀에 등록이 안돼 있음. 3# 생성 함수 확인");
        }
        return null;
    }

    // 개체를 반환하는 함수.
    // 다시 오브젝트 풀로 돌려보냄.
    public void ReturnObjectToPool(string poolName, GameObject poolObject)
    {
        if (enemyObjectPool.ContainsKey(poolName))
        {
            enemyObjectPool[poolName].Enqueue(poolObject);
            poolObject.SetActive(false);
        }
        else
        {
            Debug.Log(poolName + "의 오브젝트 풀을 사용할 수 없음. (1# 이름이 다를 수 있음. 2# 반환 함수 확인");
        }
    }
}