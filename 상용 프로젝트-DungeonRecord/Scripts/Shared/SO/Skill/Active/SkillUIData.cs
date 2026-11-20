using UnityEngine;

public struct SkillUIData
{
    public enum EType { None, Range, AdjacentRange, TargetIcon }

    public EType Type;
    public int Range;
    public Color Color;
}
