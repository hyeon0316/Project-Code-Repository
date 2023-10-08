using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Drop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _image;

    [SerializeField] private Sprite _closeSprite;
    [SerializeField] private Sprite _openSprite;
    
    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void SetCloseSprite()
    {
        _image.sprite = _closeSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CursorManager.Instance.IsDrag)
        {
            _image.sprite = _openSprite;
        }
            
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetCloseSprite();
    }
}
