using System.Collections.Generic;


[System.Serializable]
public class ActiveSkillState
{
    public ActiveSkillSO Data;

    public Dictionary<int, List<ISkillTarget>> TargetsByPosDic = new(GlobalVariable.PARTY_MEMER_MAX_COUNT);

    public void Clear()
    {
        foreach (var target in TargetsByPosDic)
        {
            target.Value.Clear();
        }
        TargetsByPosDic.Clear();
    }
}
