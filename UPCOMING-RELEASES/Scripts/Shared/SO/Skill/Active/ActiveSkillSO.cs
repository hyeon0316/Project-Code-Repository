using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;


[CreateAssetMenu(menuName = "SO/Character/Skill/Active")]
public class ActiveSkillSO : ScriptableObject
{
    public string ID;
    public string Name;
    public int MpCost;
    public string[] EffectIDs;

    [SerializeField] private SkillMotionSO m_SkillMotion;
    [SerializeField] private SkillTargetCalculator m_TargetCalculator;

    private void OnValidate()
    {
        ID = this.name;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public async UniTask PlayMotion(SkillMotionContext context)
    {
        if (m_SkillMotion == null)
            return;

        await m_SkillMotion.Play(context);
    }

    public void CalculateTargets(int casterIndex, List<ISkillTarget> characters,
        List<ISkillTarget> targets, ActiveSkillState skillState)
    {
        m_TargetCalculator.Calculate(casterIndex, characters, targets, skillState.TargetsByPosDic);
    }

    public SkillUIData GetUIData()
    {
        return m_TargetCalculator.GetUIData();
    }

    public string GetTypeName()
    {
        return m_TargetCalculator.GetTypeName();
    }

    public string GetIconKey()
    {
        return m_TargetCalculator.GetIconKey();
    }


}
