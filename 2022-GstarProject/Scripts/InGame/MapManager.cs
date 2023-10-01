using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : Singleton<MapManager>
{
    public Transform[] NpcTransForm;
    public Transform[] MapTransfom;
    public Transform[] EnemyTransfom;
    public GameObject dun;
    public EnemySpawnArea DunArea;
    public NpcData TargetNpc { get; set; }
    public EnemySpawnController[] enemySpawns;

    public Transform GetNpcData(int _id)
    {
        for (int i = 0; i < NpcTransForm.Length; i++)
        {
            if (NpcTransForm[i].GetComponent<NpcData>().ID == _id)
            {
                return NpcTransForm[i];
            }
        }
        return null;
    }
    public Transform GetEnemySpwan(int _id)
    {
        switch (_id)
        {
            case 4:
                return EnemyTransfom[0];
            case 5:
                return EnemyTransfom[1];
            case 6:
                return EnemyTransfom[2];
            case 7:
                return EnemyTransfom[3];
            case 10:
                return EnemyTransfom[4];
            case 11:
                return EnemyTransfom[5];
            case 12:
                return EnemyTransfom[6];
            case 14:
                return EnemyTransfom[8];
            case 15:
                return EnemyTransfom[7];
        }
        return null;
    }
    public Transform GetSpwan(int _id)
    {
        for (int i = 0; i < enemySpawns.Length; i++)
        {
            enemySpawns[i].InActiveSpawnArea();
        }
       
        SoundManager.Instance.BgmPlay(2);
        switch (_id)
        {
            case 1:
                return MapTransfom[0];
            case 2:
                return MapTransfom[0];
            case 3:
                return MapTransfom[0];
            case 4:
                return MapTransfom[1];
            case 5:
                return MapTransfom[1];
            case 6:
                return MapTransfom[2];
            case 7:
                return MapTransfom[2];
            case 8:
                return MapTransfom[0];
            case 9:
                return MapTransfom[3];
            case 10:
                return MapTransfom[3];
            case 11:
                return MapTransfom[3];
            case 12:
                return MapTransfom[4];
            case 13:
                return MapTransfom[0];
            case 14:
                return MapTransfom[5];
            case 15:
                return MapTransfom[4];
        }
        return null;
    }
}
