using UnityEngine;

public class BubblePanel : DialoguePanel
{
    public enum EDirectionType
    {
        Left,
        Right,
        Center
    }

    [SerializeField] private RectTransform m_MoveRect;
    [SerializeField] private RectTransform m_PanelRect;
    [SerializeField] private float m_DirectionOffset = 150f;

    public void SetDirection(EDirectionType dir)
    {
        float resultX = 0;
        if (dir == EDirectionType.Left)
            resultX = -m_DirectionOffset;
        else if (dir == EDirectionType.Right)
            resultX = m_DirectionOffset;
        else if (dir == EDirectionType.Center)
            resultX = 0;
        m_PanelRect.anchoredPosition = new Vector2(resultX, m_PanelRect.anchoredPosition.y);
    }

    public void SetPos(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            m_MoveRect.parent as RectTransform, screenPos, null, out var localPos);
        m_MoveRect.anchoredPosition = localPos;
    }

    public void SetAnchoredPos(Vector2 pos)
    {
        m_MoveRect.anchoredPosition = pos;
    }
}
