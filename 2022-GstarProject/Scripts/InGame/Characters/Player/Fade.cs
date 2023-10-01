using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    private Image _image;
    [Header("페이드 인 또는 아웃 하는데 걸리는 시간")]
    [SerializeField] private float _doTime;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    private void Start()
    {
        _image.raycastTarget = false;
    }

    public void FadeInOut(Action action)
    {
        StartCoroutine(FadeCo(action));
    }

    /// <summary>
    /// 장면 전환시 사용
    /// </summary>
    private IEnumerator FadeCo(Action action)
    {
        WaitForSeconds delay = new WaitForSeconds(_doTime);
        
        FadeIn();
        yield return delay;
        action();
        yield return null;
        FadeOut();
    }
    
    
    public void FadeIn()
    {
        _image.DOFade(1, _doTime);
        _image.raycastTarget = true;
        DataManager.Instance.Player.SetInvincibility(true);
    }

    public void FadeOut()
    {
        DataManager.Instance.Player.SetInvincibility(false);
        _image.DOFade(0, _doTime);
        Invoke("DelayActiveRaycast", _doTime);
    }

    private void DelayActiveRaycast()
    {
        _image.raycastTarget = false;
    }
    
}
