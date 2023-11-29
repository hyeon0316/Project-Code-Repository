using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interaction : MonoBehaviour
{
    protected bool _canInteract;

    [SerializeField] protected GameObject _actionButtonObj;
    [SerializeField] private Vector3 _actionButtonVec;

    /// <summary>
    /// 상호작용을 시작하는 함수
    /// </summary>
    public abstract void StartInteract();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("상호작용 가능");
            _canInteract = true;
            _actionButtonObj.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("상호작용 불가능");
            _canInteract = false;
            _actionButtonObj.SetActive(false);
        }
    }

    private void Update()
    {
        if (_canInteract)
        {
            Vector2 pos = Camera.main.WorldToScreenPoint(this.transform.position + _actionButtonVec);
            _actionButtonObj.transform.position = pos;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartInteract();
                _actionButtonObj.SetActive(false);
                _canInteract = false;
            }
        }
    }
}
