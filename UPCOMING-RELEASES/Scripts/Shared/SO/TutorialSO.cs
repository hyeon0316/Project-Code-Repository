using UnityEngine;

[CreateAssetMenu(menuName = "SO/Tutorial/Tutorial")]
public class TutorialSO : ScriptableObject
{
    public ETutorialID ID;
    public TutorialStep[] Steps;
    public EGameEventType Trigger;
    public string TriggerParamter;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Steps == null)
            return;

        foreach (var step in Steps)
            step.OnValidate();
    }
#endif
}
