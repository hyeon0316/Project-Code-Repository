public class BleedHandler : IDotEffectHandler
{
    public bool IsSharedStack { get { return false; } }

    /// <summary>고정값 + 시전자 공격력 비례. 스택 수는 데미지에 영향을 주지 않는다.</summary>
    public float CalcDotDamage(EffectEntry effect, EffectContext context, int stackCount)
    {
        return effect.Value + context.Caster.GetAttack() * GlobalVariable.BLEED_ATK_RATIO;
    }
}
