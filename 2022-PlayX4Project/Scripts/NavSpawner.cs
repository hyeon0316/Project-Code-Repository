using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavSpawner : MonoBehaviour
{
    public GameObject NavItem;
    private PathFinding _pathFinding;
    private Player _player;

    private void Awake()
    {
        _pathFinding = NavItem.GetComponent<PathFinding>();
        _player = FindObjectOfType<Player>();
    }

    private void Start()
    {
        SpawnNav(FindObjectOfType<NpcTalk>().transform.position);
    }

    public void SpawnNav(Vector3 targetObj)
    {
        _pathFinding.TargetObj = targetObj;        
        Instantiate(NavItem, _player.transform.position, Quaternion.identity);
    }
    
}
