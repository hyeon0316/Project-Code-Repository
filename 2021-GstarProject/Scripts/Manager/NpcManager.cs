using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class NpcManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static NpcManager inst = null;
    public bool wizardNpc;
    public bool ghostNpc;
    public bool knightNpc;
    public bool witchNpc;
    //포탈이름
    public bool townRoom;
    public bool dunTea;
    public bool roomBoss;
    //
    public bool minor;
    public bool minor1;
    public bool room;
    public bool dungeon;
    public bool boss;
    public bool tea;
    GameObject _target;
    private void Awake()
    {
        if (inst == null)
        {
            inst = this;
        }
        wizardNpc = false;
        ghostNpc = false;
        knightNpc = false;
        witchNpc = true;
        townRoom = false;
        dunTea = false;
        roomBoss = false;
        minor = false;
        minor1 = false;
        room = false;
        dungeon = false;
        boss = false;
        tea = false;
    }
    public void Setmap()
    {
        minor = false;
        minor1 = false;
        room = false;
        dungeon = false;
        boss = false;
        tea = false;
    }
    // Update is called once per frame
    public void NpcCheck()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Town")
        {
            _target = GameObject.Find("Wizard");
            _target.GetComponent<ObjData>().questQ.SetActive(wizardNpc);
            _target = GameObject.Find("Witch");
            _target.GetComponent<ObjData>().questQ.SetActive(witchNpc);
            _target = GameObject.Find("KnightFemale");
            _target.GetComponent<ObjData>().questQ.SetActive(knightNpc);
        }
        if (sceneName == "Room")
        {
            _target = GameObject.Find("Ghost");
            _target.GetComponent<ObjData>().questQ.SetActive(ghostNpc);
        }
    }

    public void MapCheck()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (minor1)
        {
            if (sceneName == "Town")
            {
                _target = GameObject.Find("TownMinor");
            }
            if (sceneName == "Minor")
            {
                _target = GameObject.Find("Minor 1");
            }
        }
        if (minor)
        {
            if (sceneName == "Town")
            {
                _target = GameObject.Find("TownMinor");
            }
        }
        if (dungeon)
        {
            if (sceneName == "Town")
            {
                _target = GameObject.Find("TownRoom");
            }
            if (sceneName == "Room")
            {
                _target = GameObject.Find("RoomDungeon");
            }
        }
        if (boss)
        {
            if (sceneName == "Town")
            {
                _target = GameObject.Find("TownRoom");
            }
            if (sceneName == "Room")
            {
                _target = GameObject.Find("RoomBoss");
            }
            if (sceneName == "Dungeon")
            {
                _target = GameObject.Find("DungeonRoom");
            }
        }
        if (tea)
        {
            if (sceneName == "Room")
            {
                _target = GameObject.Find("RoomDungeon");
            }

            if (sceneName == "Dungeon")
            {
                _target = GameObject.Find("DungeonTea");
            }
        }
        if (room)
        {
            if (sceneName == "Town")
            {
                _target = GameObject.Find("TownRoom");
            }
        }

        FindNpc.inst._target = _target;
    }

    public void PotalCheck()
    {
        GameObject h;
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Town")
        {
            h = GameObject.Find("TownRoom");
            h.transform.GetChild(0).gameObject.SetActive(townRoom);
            h.transform.GetChild(1).gameObject.SetActive(townRoom);
        }
        if (sceneName == "Dungeon")
        {
            h = GameObject.Find("DungeonTea");
            h.transform.GetChild(0).gameObject.SetActive(dunTea);
            h.transform.GetChild(1).gameObject.SetActive(dunTea);
        }
        if (sceneName == "Room")
        {
            h = GameObject.Find("RoomBoss");
            h.transform.GetChild(0).gameObject.SetActive(roomBoss);
            h.transform.GetChild(1).gameObject.SetActive(roomBoss);
        }

    }
}
