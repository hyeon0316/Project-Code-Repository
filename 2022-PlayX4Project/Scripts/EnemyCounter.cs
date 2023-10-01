using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class EnemyCounter : MonoBehaviour
{
    public int EnemyPosIndex;
    public int WalfIndex;

    private bool _isLock;

    private GameManager _gameManager;

    public static bool IsPlayerStop;

    private bool _isSecondFloorEvent;
    private bool _isBossRoomEvent;

    private void Awake()
    {
        _isSecondFloorEvent = true;
        _isBossRoomEvent = true;
        _gameManager = FindObjectOfType<GameManager>();
    }

    private void OnEnable()
    {
        if (this.transform.childCount != 0)
        {
            if (this.transform.name.Equals("EnemyPos_Second") && _isSecondFloorEvent)
            {
                _gameManager.PlayCutScene(1);
                IsPlayerStop = true;
                _isSecondFloorEvent = false;
                _isLock = true;
            }
            else if (this.transform.name.Equals("EnemyPos_Boss") && _isBossRoomEvent)
            {
                FindObjectOfType<SoundManager>().Play("BossBGM",SoundType.Bgm);
                _gameManager.PlayCutScene(2);
                IsPlayerStop = true;
                _isBossRoomEvent = false;
            }
        }
    }

    private void Update()
    {
        if (_isLock)
        {
            if (_gameManager.EnemyPos[2].transform.childCount == 0)
            {
                FindObjectOfType<Inventory>().AddMaterial("PrisonKey");
                _isLock = false;
            }
        }

        if (this.transform.childCount == 0)
        {
            _gameManager.ActivateWalf(EnemyPosIndex, WalfIndex);
        }

        if (IsPlayerStop)
        {
            FindObjectOfType<Player>().IsStop = true;
        }
    }
}
