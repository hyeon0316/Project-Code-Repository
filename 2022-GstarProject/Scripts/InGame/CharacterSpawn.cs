using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawn : MonoBehaviour
{
    public GameObject[] CharPrefabs;

    private void Awake()
    {
        Spawn();
    }

    /// <summary>
    /// 선택창에서 골랐던 캐릭터 생성
    /// </summary>
    private void Spawn()
    {
        GameObject player = Instantiate(CharPrefabs[(int) DataManager.Instance.SelectJobType]);
        player.transform.position = transform.position;//생성 위치
        player.name = $"{DataManager.Instance.SelectJobType}";
    }
}
