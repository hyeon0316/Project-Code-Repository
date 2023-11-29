using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magma : MonoBehaviour
{
    [SerializeField] private Transform _resetPos;
    [SerializeField] private Material _mat;

    private float _moveOffset;

    private void OnEnable()
    {
        _moveOffset = 0;
    }

    private void Update()
    {
        _moveOffset += 0.003f;
        _mat.SetTextureOffset("_MainTex", new Vector2(_moveOffset * 0.1f, 0));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Fall();
            this.GetComponent<BoxCollider>().enabled = false;
        }
    }

    private void Fall()
    {
        PlayerManager.Instance.Player.StopMove();
        FadeManager.Instance.FadeInAsync().ContinueWith(Respawn).Forget();
    }

    private void Respawn()
    {
        PlayerManager.Instance.Player.transform.position = _resetPos.position;
        CameraManager.Instance.transform.position = PlayerManager.Instance.Player.transform.position + Vector3.up;
        PlayerManager.Instance.Player.GetComponent<Rigidbody>().useGravity = false;
        PlayerManager.Instance.Player.GetComponent<Rigidbody>().useGravity = true;
        PlayerManager.Instance.Player.ReMove();
        this.GetComponent<BoxCollider>().enabled = true;
        FadeManager.Instance.FadeOut();
    }
}
