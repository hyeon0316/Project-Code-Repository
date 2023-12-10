using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CachingManager : MonoBehaviour
{
    private static CachingManager _instance;

    public GameObject SecondFloorCameraPos;
    public GameObject BossCameraPos;
    public GameObject BringerSkillObj;
    public GameObject BloodObj;
    public Npc SecondFloorNpc;
    public GameObject PortalObj;

    public static CachingManager Instance()
    {
        return _instance;
    }

    private void Awake()
    {
        _instance = FindObjectOfType<CachingManager>();
    }
}
