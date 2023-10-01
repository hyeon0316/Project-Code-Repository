using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyspawnManager : MonoBehaviour
{

    public GameObject[] EnemyPri;

    public Transform SpawnPoint;


    public void Spawn(int index)
    {
        Instantiate(EnemyPri[index],SpawnPoint.position, Quaternion.identity,this.transform);
    }

    public void Alldestroy()
    {
        GameObject[] Enemys = GameObject.FindGameObjectsWithTag("Enemy");
        for(int i = 0; i < Enemys.Length; i++)
        {
            Destroy(Enemys[i]);
        }
    }

}
