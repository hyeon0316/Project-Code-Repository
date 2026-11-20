using UnityEngine;

[System.Serializable]
public class TutorialStep
{
    public ETutorialAnchorID HighlightAnchor = ETutorialAnchorID.None;
    public ETutorialAdvanceType Advance;

    public EGameEventType CompleteEvent = EGameEventType.None;
    public string CompleteParam;

    public string CornerText;
    public string[] Pages;

    public bool HasHighlight
    {
        get { return HighlightAnchor != ETutorialAnchorID.None; }
    }

#if UNITY_EDITOR
    public void OnValidate()
    {
        if (Advance == ETutorialAdvanceType.ClickTarget && !HasHighlight)
            Debug.LogError("[TutorialStep] ClickTarget은 HighlightAnchor가 필요합니다.");

        if (Advance == ETutorialAdvanceType.GameEvent && CompleteEvent == EGameEventType.None)
            Debug.LogError("[TutorialStep] GameEvent 진행은 CompleteEvent가 필요합니다.");

        if (Advance == ETutorialAdvanceType.Pages)
        {
            if (Pages == null || Pages.Length == 0)
                Debug.LogError("[TutorialStep] Pages 진행은 Pages가 1개 이상 필요합니다.");
            if (HasHighlight)
                Debug.LogError("[TutorialStep] Pages 진행과 HighlightAnchor를 동시에 사용할 수 없습니다.");
        }
    }
#endif
}
