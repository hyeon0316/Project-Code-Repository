using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // Destroy 여부 확인용
    private static bool _shuttingDown = false;
    private static object _lock = new object();
    
    private static T _instance;
    public static T Instance
    {
        get
        {
            lock (_lock) //Thread Safe
            {
                if (_instance == null && Time.timeScale != 0)
                {
                    // 인스턴스 존재 여부 확인
                    _instance = (T) FindObjectOfType(typeof(T));

                    // 아직 생성되지 않았다면 인스턴스 생성
                    if (_instance == null)
                    {
                        // 새로운 게임오브젝트를 만들어서 싱글톤 Attach
                        var singletonObject = new GameObject($"{typeof(T)} (Singleton)");
                        _instance = singletonObject.AddComponent<T>();
                    }
                }

                DontDestroyOnLoad(_instance);
                return _instance;
            }
        }
    }

    private void OnApplicationQuit()
    {
        Time.timeScale = 0;
    }
}