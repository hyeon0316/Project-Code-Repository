using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 카메라 상하좌우 회전 담당
/// </summary>
public class CameraRotate : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Vector2 _playerTouchVectorOutput;
    private bool _isPlayerTouchingPanel;
    private Touch _myTouch;
    private int _touchID;

    private bool _isDragging;

    private void FixedUpdate()
    {
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                _myTouch = Input.GetTouch(i);
                if (_isPlayerTouchingPanel)
                {
                    if (_myTouch.fingerId == _touchID)
                    {
                        if (_myTouch.phase == TouchPhase.Stationary) // 터치했으나 마지막 프레임 변화가 없을때
                            OutputVectorValue(Vector2.zero);
                    }
                }
            }
        }
    }
    
    private void OutputVectorValue(Vector2 outputValue)
    {
        _playerTouchVectorOutput = outputValue;
    }
    
    /// <summary>
    /// 플레이어가 터치한 벡터값을 반환
    /// </summary>
    public Vector2 PlayerJoystickOutputVector()
    {
        return _playerTouchVectorOutput;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _touchID = _myTouch.fingerId;
        _isPlayerTouchingPanel = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OutputVectorValue(Vector2.zero);
        _isPlayerTouchingPanel = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        OutputVectorValue(new Vector2(eventData.delta.normalized.x, eventData.delta.normalized.y));
    }

}
