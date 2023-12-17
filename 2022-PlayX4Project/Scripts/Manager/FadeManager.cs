using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public sealed class FadeManager : Singleton<FadeManager>
{
    [Range(0,1)]
    [SerializeField] private float _fadeDelayRate;
    [SerializeField] private Image _fadeImage;

    private float _fadeRate;
    private Color _color = new Color(0, 0, 0);
        
    public void FadeIn()
    {
        StopAllCoroutines();
        _fadeImage.gameObject.SetActive(true);
        StartCoroutine(FadeInCo());
    }

    public async UniTask FadeInAsync()
    {
        _fadeRate = 0f;
        while (_fadeRate <= 1.0f)
        {
            _fadeRate += _fadeDelayRate;
            await UniTask.Delay(TimeSpan.FromSeconds(_fadeDelayRate));
            _color.a = _fadeRate;
            _fadeImage.color = _color;
        }
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        _fadeImage.gameObject.SetActive(true);
        StartCoroutine(FadeOutCo());
    }
    
    private IEnumerator FadeInCo()
    {
        _fadeRate = 0f;
        while (_fadeRate <= 1.0f)
        {
            _fadeRate += _fadeDelayRate;
            yield return new WaitForSeconds(_fadeDelayRate);
            _color.a = _fadeRate;
            _fadeImage.color = _color;
        }
    }

    private IEnumerator FadeOutCo()
    {
        _fadeRate = 1f;
        while (_fadeRate >= 0)
        {
            _fadeRate -= _fadeDelayRate;
            yield return new WaitForSeconds(_fadeDelayRate);
            _color.a = _fadeRate;
            _fadeImage.color = _color;
        }
        _fadeImage.gameObject.SetActive(false);
    }
}