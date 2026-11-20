public readonly struct EffectContext
{
    public readonly ISkillTarget Caster;
    public readonly ISkillTarget Target;
    public readonly float ScalingValue;

    public EffectContext(ISkillTarget caster, ISkillTarget target, float scalingValue = 1)
    {
        Caster = caster;
        Target = target;
        ScalingValue = scalingValue;
    }
}
