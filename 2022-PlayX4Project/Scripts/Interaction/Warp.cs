using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Warp : Interaction
{
    [SerializeField] private Transform _nextPos;
    [SerializeField] private MapType _targetMap;


    public override void StartInteract()
    {
        PlayerManager.Instance.Player.StopMove();
        FadeManager.Instance.FadeInAsync().ContinueWith(MoveMap).Forget();
    }

    private void MoveMap()
    {
        GameEvent.CallMapChange(_targetMap);

        CameraManager.Instance.CameraMovetype = 0;
        CameraManager.Instance.BackgroudUpdate();
        CameraManager.Instance.transform.position += new Vector3(CameraManager.Instance.BackgroundImg.transform.position.x, 0, 0);
        CameraManager.Instance.ChangeCameraType();

        PlayerManager.Instance.Player.transform.position = _nextPos.transform.position;
        PlayerManager.Instance.Player.ReMove();

        FadeManager.Instance.FadeOut();
    }
}

