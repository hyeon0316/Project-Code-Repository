using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Character/Skill/Calculator/AdjacentTarget")]
public class AdjacentTargetCalculator : SkillTargetCalculator
{
    [Range(1, 7)] public int Range;

    public override void Calculate(int casterIndex, List<ISkillTarget> characters,
        List<ISkillTarget> targets, Dictionary<int, List<ISkillTarget>> targetsByPosDic)
    {
        if (targetsByPosDic.ContainsKey(casterIndex))
            return;

        var targetsByPos = new List<ISkillTarget>();
        int[] targetIndices = { Range - casterIndex - 1, Range - casterIndex, Range - casterIndex - 2 }; // Center, Right, Left
        foreach (int index in targetIndices)
        {
            if (index >= 0 && index < targets.Count)
            {
                var target = targets.Find(c => c.TransformIndex == index);
                if (target != null)
                    targetsByPos.Add(target);
            }
        }
        targetsByPosDic[casterIndex] = targetsByPos;
    }

    public override SkillUIData GetUIData() => new SkillUIData
    {
        Type = SkillUIData.EType.AdjacentRange,
        Range = Range,
        Color = Color.yellow
    };

    public override string GetTypeName()
    {
        return new LString("SKILLTARGET_ADJACENT").ToFormat(Range);
    }

    public override string GetIconKey()
    {
        return "Skill_Target";
    }
}
