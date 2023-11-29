using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MapType
{
    Dungeon1,
    Dungeon2,
    Dungeon3,
    UnderDungeon,
    Boss,
    Second
}

public class MapSystem : MonoBehaviour
{
    [SerializeField] private GameObject[] _maps;

    private void Awake()
    {
        Warp.MapChangeEvent += SetMap;
    }

    public void SetMap(MapType type)
    {
        for (int i = 0; i < _maps.Length; i++)
        {
            _maps[i].SetActive(i == (int)type);
        }
    }
}
