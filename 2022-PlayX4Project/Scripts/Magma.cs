using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magma : MonoBehaviour
{
    private Material _mat;
    private float _moveOffset;
    private FadeImage _fade;
    private Player _player;
    private void Awake()
    {
        _player = FindObjectOfType<Player>();
        _fade = GameObject.Find("Canvas").transform.Find("FadeImage").GetComponent<FadeImage>();
    }

    private void OnEnable()
    {
        _mat = GetComponent<MeshRenderer>().material;
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
            StartCoroutine(RevivalCo());
            this.GetComponent<BoxCollider>().enabled = false;
        }
    }

    private IEnumerator RevivalCo()
    {
        _fade.FadeIn();
        FindObjectOfType<Player>().IsStop = true;
        yield return new WaitForSeconds(2f);
        _player.transform.position = GameObject.Find("RevivalPos").transform.position;
        FindObjectOfType<CameraManager>().transform.position = _player.transform.position + Vector3.up;
        FindObjectOfType<Player>().GetComponent<Rigidbody>().useGravity = false;
        _player.GetComponent<Rigidbody>().useGravity = true;
        FindObjectOfType<Player>().IsStop = false;
        this.GetComponent<BoxCollider>().enabled = true;
        _fade.FadeOut();
    }
}
