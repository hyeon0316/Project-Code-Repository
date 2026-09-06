
public class PassiveSkill
{
    public PassiveSkillSO Data;
    public string GroupID;

    public PassiveSkill(PassiveSkillSO data, string groupID)
    {
        Data = data;
        GroupID = groupID;
    }
}

public class ActiveSkill
{
    public ActiveSkillSO Data;
    public ActiveSkill(ActiveSkillSO data)
    {
        Data = data;
    }
}