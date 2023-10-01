using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Portal : MonoBehaviour
{
    public GameObject[] EnemyPrefabs;
    public Transform SummonEnemysTr;
    private void OnEnable()
    {
        FindObjectOfType<SoundManager>().Play("Enemy/Necromancer/NecromancerPortal",SoundType.Effect);
        this.transform.position = GameObject.Find("Demon_Page1").transform.position +
                                  new Vector3(Random.Range(-2f, 2f), -0.5f, Random.Range(-2f, 2f));
    }

    private void Update()
    {
        if(this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >=0.9f)
            this.gameObject.SetActive(false);
    }

    public void Summon()
    {
        if (Necromancer.IsSkill)
        {
            GameObject go = Instantiate(EnemyPrefabs[4], this.transform.position, this.transform.rotation);
            Necromancer.IsSkill = false;
            go.transform.parent = SummonEnemysTr;
        }
        else
        {
            GameObject go = Instantiate(EnemyPrefabs[Random.Range(0, 4)], this.transform.position,
                this.transform.rotation);
            go.transform.parent = SummonEnemysTr;
        }
    }

}
