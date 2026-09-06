using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Character/Skill/Calculator/SingleTarget")]
public class SingleTargetCalculator : SkillTargetCalculator
{
    [Range(1, 7)] public int Range;

    public override void Calculate(int casterIndex, List<ISkillTarget> characters,
        List<ISkillTarget> targets, Dictionary<int, List<ISkillTarget>> targetsByPosDic)
    {
        if (targetsByPosDic.ContainsKey(casterIndex))
            return;

        var targetsByPos = new List<ISkillTarget>();
        int resultTargetIndex = Range - casterIndex - 1;
        if (resultTargetIndex >= 0 && resultTargetIndex < targets.Count)
        {
            var target = targets.Find(t => t.TransformIndex == resultTargetIndex);
            targetsByPos.Add(target);
        }
        targetsByPosDic[casterIndex] = targetsByPos;
    }

    public override SkillUIData GetUIData() => new SkillUIData
    {
        Type = SkillUIData.EType.Range,
        Range = Range,
        Color = Color.green
    };

    public override string GetTypeName()
    {
        return new LString("SKILLTARGET_SINGLE").ToFormat(Range);
    }

    public override string GetIconKey()
    {
        return "Skill_Target";
    }
}
