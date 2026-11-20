public class DamageHandler : ITargetEffectHandler
{
    public void Execute(EffectEntry effect, EffectContext context)
    {
        float damage = context.Caster.GetAttack() * context.ScalingValue;
        context.Target.TakeDamage(damage, false, context.Caster);
    }
}
