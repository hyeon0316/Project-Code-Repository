using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleTransition : MonoBehaviour
{
    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }
    

    /// <summary>
    /// 화면을 서서히 가림
    /// </summary>
    public void FadeIn(float duration)
    {
        StartCoroutine(TransitionCo(duration, 1, 0));
    }
    
    
    /// <summary>
    /// 화면을 서서히 드러냄
    /// </summary>
    /// <param name="duration">걸리는 시간</param>
    public void FadeOut(float duration)
    {
        StartCoroutine(TransitionCo(duration, 0, 1));
    }

    public void StopFade()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// 아웃 상태로 초기화
    /// </summary>
    public void SetRadius(float value)
    {
        _image.material.SetFloat(Global.CircleFade, value);
    }
    
    private IEnumerator TransitionCo(float duration, float beginRadius, float endRadius)
    {
        float time = 0f;
        while (time <= duration)
        {
            time += Time.deltaTime;
            float lerp = time / duration;
            float radius = Mathf.Lerp(beginRadius, endRadius, lerp);
            _image.material.SetFloat(Global.CircleFade, radius);
            yield return null;
        }
    }
}
