using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Transform[] points;
    public GameObject monsterPrefab;
    public int maxMonster = 10;
    public float spawnTime;

    // Start is called before the first frame update
    void Start()
    {
        points = GameObject.Find("SpawnPoints").GetComponentsInChildren<Transform>();
        StartCoroutine("SpawnMonster");
    }

    IEnumerator SpawnMonster()
    {
        while(true)
        {
            int monsterCount = (int)GameObject.FindGameObjectsWithTag("Enemy").Length;

            if(monsterCount < maxMonster)
            {
                yield return new WaitForSeconds(spawnTime);
            
                for (int i = 0; i < points.Length; i++)
                {
                    float rand1 = Random.Range(points[i].position.x - 4, points[i].position.x + 4);
                    float rand2 = Random.Range(points[i].position.z - 4, points[i].position.z + 4);
                    Instantiate(monsterPrefab, new Vector3(rand1,points[i].position.y,rand2), points[i].rotation);
                }
            }
            else
            {
                yield return null;
            }
        }
    }
}
