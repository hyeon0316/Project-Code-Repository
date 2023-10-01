using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FadeImage : MonoBehaviour, IFade
{
    private Image _fadeImage;
    public float FadeCount;
    public bool IsFade;//페이드 인 이후 페이드 아웃 전에 해야할 작업들이 있을 때 사용하는 변수

        
    private void OnEnable()
    {
        _fadeImage = GetComponent<Image>();
    }


    public void FadeIn()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        StartCoroutine(FadeInCo());
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        StartCoroutine(FadeOutCo());
    }
    
    IEnumerator FadeInCo()
    {
        FadeCount = 0f;
        while (FadeCount <= 1.0f)
        {
            FadeCount += 0.01f;
            yield return new WaitForSeconds(0.01f);
            _fadeImage.color = new Color(0, 0, 0, FadeCount);
        }
        IsFade = true;
    }

    IEnumerator FadeOutCo()
    {
        FadeCount = 1f;
        IsFade = false;
        while (FadeCount >= 0)
        {
            FadeCount -= 0.01f;
            yield return new WaitForSeconds(0.01f);
            _fadeImage.color = new Color(0, 0, 0, FadeCount);
        }
        gameObject.SetActive(false);
    }
}