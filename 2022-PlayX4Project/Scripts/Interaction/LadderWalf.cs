using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderWalf : MonoBehaviour
{
    public string MapColliderName;
    public Transform NextLadderWalf;
    public float CameraZ;
    private FadeImage _fade;
    public float StartPos;
    private CameraManager _camera;
    private Player _player;
    private bool _isLadderWalf;
    
    

    private void Awake()
    {
        _fade = GameObject.Find("Canvas").transform.Find("FadeImage").GetComponent<FadeImage>();
        _camera = GameObject.Find("Camera").GetComponent<CameraManager>();
        _player = GameObject.Find("Player").GetComponent<Player>();
    }

    private void Update()
    {
        if (_fade.IsFade && _isLadderWalf)
        {
            Debug.Log("다음 사다리로");
           FindObjectOfType<GameManager>().ActivateCollider(MapColliderName);

            _player.transform.position = new Vector3(NextLadderWalf.transform.position.x,
                NextLadderWalf.transform.position.y + StartPos, NextLadderWalf.transform.position.z);
            
            _camera.CameraMovetype = 0;
            _camera.BackgroudUpdate();
            _camera.transform.position = new Vector3(_player.transform.position.x,
                _player.transform.position.y + StartPos, _player.transform.position.z + CameraZ);
            _camera.ChangeCameraType();
            _fade.FadeOut();
            _isLadderWalf = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _fade.FadeIn();
            _isLadderWalf = true;
        }
    }
}
