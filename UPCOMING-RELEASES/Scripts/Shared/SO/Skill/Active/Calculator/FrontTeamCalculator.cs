using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Character/Skill/Calculator/FrontTeam")]
public class FrontTeamCalculator : SkillTargetCalculator
{
    public override void Calculate(int casterIndex, List<ISkillTarget> characters,
        List<ISkillTarget> targets, Dictionary<int, List<ISkillTarget>> targetsByPosDic)
    {
        if (targetsByPosDic.ContainsKey(casterIndex))
            return;

        var targetsByPos = new List<ISkillTarget>();
        if (casterIndex != 0)
        {
            var target = characters.Find(c => c.TransformIndex == casterIndex - 1);
            targetsByPos.Add(target);
        }
        targetsByPosDic[casterIndex] = targetsByPos;
    }

    public override SkillUIData GetUIData() => new SkillUIData { Type = SkillUIData.EType.TargetIcon };

    public override string GetTypeName()
    {
        return Localize.Get("SKILLTARGET_FRONTTEAM");
    }

    public override string GetIconKey()
    {
        return "Skill_Team";
    }
}
