using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Character/Skill/Passive")]
public class PassiveSkillSO : ScriptableObject
{
    public string Name;
    public EffectTriggerType TriggerType;
    public string[] EffectIDs;

    [SerializeField] private SkillTargetCalculator m_TargetCalculator;

    public void CalculateTargets(int casterIndex, List<ISkillTarget> characters,
        List<ISkillTarget> targets, Dictionary<int, List<ISkillTarget>> targetsByPosDic)
    {
        m_TargetCalculator.Calculate(casterIndex, characters, targets, targetsByPosDic);
    }

}
