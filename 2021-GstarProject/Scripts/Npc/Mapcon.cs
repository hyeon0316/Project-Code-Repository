using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Mapcon : MonoBehaviour//미니맵(캐릭터 위치표시, 퀘스트 길잡이)
{
    // Start is called before the first frame update
    void Start()
    {
        string mapName = SceneManager.GetActiveScene().name;
        if (mapName == "Town")
            mapName = "마을";
        if (mapName == "Dungeon")
            mapName = "던전 1";
        if (mapName == "Room")
            mapName = "던전 입구";
        if (mapName == "Battle Room 1")
            mapName = "던전 2";
        if (mapName == "Battle Room 2")
            mapName = "던전 3";
        if (mapName == "Boss")
            mapName = "보스방";
        if (mapName == "Minor 1")
            mapName = "광산 2";
        if (mapName == "Minor")
            mapName = "광산1";
        if (mapName == "Treasure")
            mapName = "보물방";


        Player.inst.nowMap.text = mapName;
        NpcManager.inst.NpcCheck();
        NpcManager.inst.MapCheck();
        NpcManager.inst.PotalCheck();
    }
}
