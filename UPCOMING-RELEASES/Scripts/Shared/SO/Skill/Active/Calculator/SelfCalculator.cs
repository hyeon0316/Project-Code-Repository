using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Character/Skill/Calculator/Self")]
public class SelfCalculator : SkillTargetCalculator
{
    public override void Calculate(int casterIndex, List<ISkillTarget> characters,
        List<ISkillTarget> targets, Dictionary<int, List<ISkillTarget>> targetsByPosDic)
    {
        if (targetsByPosDic.ContainsKey(casterIndex))
            return;

        var caster = characters.Find(c => c.TransformIndex == casterIndex);
        targetsByPosDic[casterIndex] = new List<ISkillTarget> { caster };
    }

    public override SkillUIData GetUIData() => new SkillUIData { Type = SkillUIData.EType.TargetIcon };

    public override string GetTypeName()
    {
        return Localize.Get("SKILLTARGET_SELF");
    }

    public override string GetIconKey()
    {
        return "Skill_Self";
    }
}
