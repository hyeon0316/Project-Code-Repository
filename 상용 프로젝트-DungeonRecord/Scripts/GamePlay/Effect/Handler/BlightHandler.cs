public class BlightHandler : IDotEffectHandler
{
    public bool IsSharedStack { get { return true; } }

    /// <summary>스택 수 × Value × 대상 최대 HP. 시전자 능력치는 쓰지 않는다.</summary>
    public float CalcDotDamage(EffectEntry effect, EffectContext context, int stackCount)
    {
        return stackCount * effect.Value * context.Target.GetMaxHp();
    }
}
