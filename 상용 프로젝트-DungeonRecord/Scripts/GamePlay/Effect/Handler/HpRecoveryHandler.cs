public class HpRecoveryHandler : ITargetEffectHandler
{
    public void Execute(EffectEntry effect, EffectContext context)
    {
        float amount = effect.EffectType == EffectType.Hp_Recovery_Percent
            ? context.Target.GetMaxHp() * effect.Value
            : effect.Value;

        context.Target.Heal(amount);
    }
}
