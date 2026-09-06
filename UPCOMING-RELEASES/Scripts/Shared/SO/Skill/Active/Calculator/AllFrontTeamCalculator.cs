using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Character/Skill/Calculator/AllFrontTeam")]
public class AllFrontTeamCalculator : SkillTargetCalculator
{
    public override void Calculate(int casterIndex, List<ISkillTarget> characters,
        List<ISkillTarget> targets, Dictionary<int, List<ISkillTarget>> targetsByPosDic)
    {
        if (targetsByPosDic.ContainsKey(casterIndex))
            return;

        var targetsByPos = new List<ISkillTarget>();
        for (int i = casterIndex - 1; i >= 0; i--)
        {
            var target = characters.Find(c => c.TransformIndex == i);
            targetsByPos.Add(target);
        }
        targetsByPosDic[casterIndex] = targetsByPos;
    }

    public override SkillUIData GetUIData() => new SkillUIData { Type = SkillUIData.EType.TargetIcon };

    public override string GetTypeName()
    {
        return Localize.Get("SKILLTARGET_ALLFRONTTEAM");
    }

    public override string GetIconKey()
    {
        return "Skill_Team";
    }
}
