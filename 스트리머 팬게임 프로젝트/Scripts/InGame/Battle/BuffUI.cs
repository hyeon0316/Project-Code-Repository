using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuffUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI _buffCountText;
    private int _count;

    [SerializeField] [TextArea] private string _toolTip;

    public void SetCount(int count)
    {
        gameObject.SetActive(count != 0);
        _count = count;
        _buffCountText.text = $"{_count}";
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
        ToolTipManager.Instance.CloseBuffDeBuff();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ToolTipManager.Instance.ShowBuffDeBuff(_toolTip);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instance.CloseBuffDeBuff();
    }
}
