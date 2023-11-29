using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderWalf : MonoBehaviour
{
    public Transform NextLadderWalf;
    public float CameraZ;
    public float StartPos;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FadeManager.Instance.FadeInAsync().ContinueWith(WalfNextLadder).Forget();
        }
    }

    private void WalfNextLadder()
    {
        PlayerManager.Instance.Player.transform.position = new Vector3(NextLadderWalf.transform.position.x,
             NextLadderWalf.transform.position.y + StartPos, NextLadderWalf.transform.position.z);

        CameraManager.Instance.CameraMovetype = 0;
        CameraManager.Instance.BackgroudUpdate();
        CameraManager.Instance.transform.position = new Vector3(PlayerManager.Instance.Player.transform.position.x,
           PlayerManager.Instance.Player.transform.position.y + StartPos, PlayerManager.Instance.Player.transform.position.z + CameraZ);
        CameraManager.Instance.ChangeCameraType();
        FadeManager.Instance.FadeOut();
    }
}
