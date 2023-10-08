using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RouletteButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button _button;
    [SerializeField]private Image _image;
    
    [SerializeField] private Sprite _closeSprite;
    [SerializeField] private Sprite _openSprite;
    
    
    public void InteractableButton(bool isActive)
    {
        _button.interactable = isActive;

        if (!isActive)
            _image.sprite = _closeSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(_button.interactable)
            _image.sprite = _openSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _image.sprite = _closeSprite;
    }
}
