using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    private TextMeshPro _text;

    public TextMeshPro Text => _text;

    [SerializeField] private float _alphaSpeed;
    [SerializeField] private float _disableTime;

    private Camera _camera;

    private Transform _startPos;

    private void Awake()
    {
        _text = GetComponent<TextMeshPro>();
    }

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if(_startPos !=null)
            UpdateTransform();
    }

    private void UpdateTransform()
    {
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, _camera.transform.rotation.eulerAngles.y, transform.rotation.z));
        transform.position = new Vector3(_startPos.position.x, transform.position.y, _startPos.position.z);
    }

    public void SetDamageText(string damage, Transform startPos)
    {
        _text.text = damage;
        _text.alpha = 1;
        _startPos = startPos;
    }

    
    public void FadeOutText()
    {
        _text.DOFade(0, _alphaSpeed);
        Invoke("DisableText", _disableTime);
    }
    public void DisableText()
    {
        ObjectPoolManager.Instance.ReturnObject(PoolType.DamageText, this.gameObject);
    }
    
}
