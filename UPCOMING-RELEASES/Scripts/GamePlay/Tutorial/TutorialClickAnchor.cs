using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// hole mesh 강조 + 클릭으로 튜토리얼 진행이 필요한 앵커. rect 좌표를 등록하고 클릭 이벤트를 발행한다.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class TutorialClickAnchor : TutorialAnchor, IPointerClickHandler
{
    public static event Action<ETutorialAnchorID> OnAnchorClicked;

    private RectTransform m_Rect;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_Rect = GetComponent<RectTransform>();
        RegisterRect(m_AnchorID, m_Rect);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UnregisterRect(m_AnchorID, m_Rect);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnAnchorClicked?.Invoke(m_AnchorID);
    }
}
