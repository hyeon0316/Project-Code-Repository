public class StunHandler : IStatusEffectHandler
{
    public void Execute(EffectEntry effect, EffectContext context)
    {
        context.Target.ApplyStun(context, effect);
    }
    public void Clear(EffectEntry effect, EffectContext context)
    {
        context.Target.RemoveStun();
        context.Target.ApplyStunResistBuff();
    }
}