using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class LetterBox : MonoBehaviour
{
    [SerializeField] private RectTransform m_Top;
    [SerializeField] private RectTransform m_Down;
    [SerializeField] private float m_Duration = 0.4f;

    public async UniTask Show()
    {
        var t1 = Tween.UIAnchoredPosition(m_Top, new Vector2(m_Top.anchoredPosition.x, 0f), m_Duration);
        var t2 = Tween.UIAnchoredPosition(m_Down, new Vector2(m_Down.anchoredPosition.x, 0f), m_Duration);
        await UniTask.WhenAll(t1.ToUniTask(), t2.ToUniTask());
    }

    public async UniTask Hide()
    {
        var t1 = Tween.UIAnchoredPosition(m_Top, new Vector2(m_Top.anchoredPosition.x, m_Top.rect.height), m_Duration);
        var t2 = Tween.UIAnchoredPosition(m_Down, new Vector2(m_Down.anchoredPosition.x, -m_Down.rect.height), m_Duration);
        await UniTask.WhenAll(t1.ToUniTask(), t2.ToUniTask());
    }

    public void SetVisible(bool visible)
    {
        m_Top.anchoredPosition = new Vector2(m_Top.anchoredPosition.x, visible ? 0f : m_Top.rect.height);
        m_Down.anchoredPosition = new Vector2(m_Down.anchoredPosition.x, visible ? 0f : -m_Down.rect.height);
    }
}
