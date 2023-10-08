using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemBuffUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] [TextArea] private string _toolTip;

    [SerializeField] private TextMeshProUGUI _valueText;

    public void SetUI(bool isActive, byte value)
    {
        gameObject.SetActive(isActive);
        if (isActive)
            _valueText.text = $"{value}";
    }
    
    public void SetUI(bool isActive)
    {
        gameObject.SetActive(isActive);
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
