using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Character/Skill/Calculator/AllTeam")]
public class AllTeamCalculator : SkillTargetCalculator
{
    public override void Calculate(int casterIndex, List<ISkillTarget> characters,
        List<ISkillTarget> targets, Dictionary<int, List<ISkillTarget>> targetsByPosDic)
    {
        if (targetsByPosDic.ContainsKey(casterIndex))
            return;

        var targetsByPos = new List<ISkillTarget>();
        targetsByPos.AddRange(characters);
        targetsByPosDic[casterIndex] = targetsByPos;
    }

    public override SkillUIData GetUIData() => new SkillUIData { Type = SkillUIData.EType.None };

    public override string GetTypeName()
    {
        return Localize.Get("SKILLTARGET_ALLTEAM");
    }

    public override string GetIconKey()
    {
        return "Skill_Team";
    }
}
