using UnityEngine;

[System.Serializable]
public class TutorialStep
{
    public bool UseHoleMesh;
    public ETutorialAnchorID AnchorID;

    public bool UseCenterPage;
    public string[] Pages;

    public bool UseCornerText;
    public string CornerText;

    public bool UseExtraText;
    public string ExtraText;

    public bool UseAdvanceArea;

#if UNITY_EDITOR
    public void OnValidate()
    {
        if (UseHoleMesh && UseCenterPage)
            Debug.LogError("[TutorialStep] UseHoleMesh와 UseCenterPage를 동시에 활성화할 수 없습니다.");
    }
#endif
}
