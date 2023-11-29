using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

public class EnemyStage : MonoBehaviour
{
    [SerializeField] private UnityEvent _stageStartEvent;
    [SerializeField] private UnityEvent _stageEndEvent;

    [SerializeField] private GameObject[] _activeWarps;

    private void OnEnable()
    {
        if (this.transform.childCount != 0)
        {
            _stageStartEvent?.Invoke();
        }
    }

    public void CallEndEvent()
    {
        _stageEndEvent?.Invoke();
        ActiveWarp();
    }

    private void ActiveWarp()
    {
        foreach (var warp in _activeWarps)
        {
            warp.SetActive(true);
        }
    }
}
