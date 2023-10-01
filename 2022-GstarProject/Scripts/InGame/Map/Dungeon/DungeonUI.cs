using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Gold1;
    [SerializeField] private TextMeshProUGUI Gold2;
    [SerializeField] private TextMeshProUGUI Gold3;
    private EnemySpawnArea spawnArea;
    private Transform _tr;
    public GameObject MapObj;
    
    private int dunIndex;

    private void Start()
    {
        spawnArea = MapManager.Instance.DunArea;
        _tr = spawnArea.gameObject.transform;
    }

    public void BtnEasy()
    {
        dunIndex = 0;
        SpawnC();
       
        

    }
    public void BtnNormal()
    {
        dunIndex = 1;
        SpawnC();
        
        
    }
    public void BtnHard()
    {
        dunIndex = 2;
        SpawnC();
        
        
    }
    public void SpawnC()
    {
        spawnArea.Difficulty = dunIndex;
        SoundManager.Instance.BgmPlay(1);
        DataManager.Instance.Player.UseTeleport(_tr);
        MapObj.SetActive(false);
    }
    public void BtnExit()
    {
        MapObj.SetActive(false);
    }
}
