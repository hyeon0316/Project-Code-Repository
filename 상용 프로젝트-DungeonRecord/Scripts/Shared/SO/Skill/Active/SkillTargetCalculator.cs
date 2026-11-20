using System.Collections.Generic;
using UnityEngine;

public abstract class SkillTargetCalculator : ScriptableObject
{
    public abstract void Calculate(int casterIndex, List<ISkillTarget> characters,
        List<ISkillTarget> targets, Dictionary<int, List<ISkillTarget>> targetsByPosDic);

    public abstract SkillUIData GetUIData();
    public abstract string GetTypeName();
    public abstract string GetIconKey();
}
