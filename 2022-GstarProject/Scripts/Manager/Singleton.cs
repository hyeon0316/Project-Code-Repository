using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static object _lock = new object();
    
    private static T _instance;
    public static T Instance
    {
        get
        {
            lock (_lock) //Thread Safe
            {
                if (_instance == null && Time.timeScale != 0) // 에디터 플레이 종료시 호출 방지
                {
                    _instance = (T) FindObjectOfType(typeof(T));

                    if (_instance == null)
                    {
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