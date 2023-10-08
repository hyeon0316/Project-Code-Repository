using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHightLight : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField] private GameObject _guideText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _guideText.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _guideText.SetActive(true);
    }
}
