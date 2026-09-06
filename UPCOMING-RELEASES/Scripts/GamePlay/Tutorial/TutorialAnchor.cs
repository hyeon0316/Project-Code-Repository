using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 튜토리얼 대기 신호 전용 앵커. rect/클릭이 필요없는 대상(ScreenRoot, hole 없는 page/cornerText 대기용)에 부착한다.
/// hole 강조 + 클릭 진행이 필요하면 TutorialClickAnchor를 쓴다.
/// </summary>
public class TutorialAnchor : MonoBehaviour
{
    public static event Action<ETutorialAnchorID> OnAnchorRegistered;

    private static readonly HashSet<ETutorialAnchorID> s_RegisteredSet = new();
    private static readonly Dictionary<ETutorialAnchorID, RectTransform> s_RectRegistry = new();

    [SerializeField] protected ETutorialAnchorID m_AnchorID;

    protected virtual void OnEnable()
    {
        RegisterSet(m_AnchorID);
    }

    protected virtual void OnDisable()
    {
        UnregisterSet(m_AnchorID);
    }

    protected static void RegisterSet(ETutorialAnchorID id)
    {
        s_RegisteredSet.Add(id);
        OnAnchorRegistered?.Invoke(id);
    }

    protected static void UnregisterSet(ETutorialAnchorID id)
    {
        s_RegisteredSet.Remove(id);
    }

    protected static void RegisterRect(ETutorialAnchorID id, RectTransform rect)
    {
        s_RectRegistry[id] = rect;
    }

    protected static void UnregisterRect(ETutorialAnchorID id, RectTransform rect)
    {
        if (s_RectRegistry.TryGetValue(id, out var registered) && registered == rect)
            s_RectRegistry.Remove(id);
    }

    public static bool IsRegistered(ETutorialAnchorID id)
    {
        return s_RegisteredSet.Contains(id);
    }

    public static RectTransform FindRect(ETutorialAnchorID id)
    {
        s_RectRegistry.TryGetValue(id, out var rect);
        return rect;
    }
}
