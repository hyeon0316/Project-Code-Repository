using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Character/Skill/Calculator/AllTarget")]
public class AllTargetCalculator : SkillTargetCalculator
{
    public override void Calculate(int casterIndex, List<ISkillTarget> characters,
        List<ISkillTarget> targets, Dictionary<int, List<ISkillTarget>> targetsByPosDic)
    {
        if (targetsByPosDic.ContainsKey(casterIndex))
            return;

        var targetsByPos = new List<ISkillTarget>();
        targetsByPos.AddRange(targets);
        targetsByPosDic[casterIndex] = targetsByPos;
    }

    public override SkillUIData GetUIData() => new SkillUIData { Type = SkillUIData.EType.None };

    public override string GetTypeName()
    {
        return Localize.Get("SKILLTARGET_ALLTARGET");
    }

    public override string GetIconKey()
    {
        return "Skill_Target";
    }
}
