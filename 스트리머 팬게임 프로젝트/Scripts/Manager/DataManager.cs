using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 여러 곳에 사용 될 데이터를 미리 캐싱하여 관리
/// </summary>
public class DataManager : Singleton<DataManager>
{
    public string PlayerNickName;

    public bool IsInit;
    
    public bool IsEmotionBlock;
    /// <summary>
    /// 플레이어의 애니메이션 캐릭터 
    /// </summary>
    public GameObject[] CharacterPrefabs;

    public Sprite[] EmotionSprites;

    /// <summary>
    /// 크립라운드 적 애니메이션 캐릭터
    /// </summary>
    public GameObject[] EnemyPrefabs;

    /// <summary>
    /// 아이템 모음
    /// </summary>
    public GameObject[] SetItems;

    /// <summary>
    /// 페이드 기능
    /// </summary>
    public CircleTransition FadeManager;

    public GameObject FadeCanvas;
    
    private void Start()
    {
        var obj = FindObjectsOfType<DataManager>();

        if (obj.Length == 1)
        {
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(FadeCanvas);
        }
        else
        {
            Destroy(gameObject);
            Destroy(FadeCanvas);
        }
        
        FadeManager.SetRadius(1);
    }
}
