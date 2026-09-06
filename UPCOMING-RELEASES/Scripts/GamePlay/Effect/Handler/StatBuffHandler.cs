public class StatBuffHandler : IStatusEffectHandler
{
    public void Execute(EffectEntry effect, EffectContext context)
    {
        if (!effect.EffectType.TryGetStatType(out var statType))
            return;

        float value = effect.EffectType.IsDebuff() ? -effect.Value : effect.Value;
        context.Target?.AddStatValue(statType, value, effect, context, effect);
    }

    public void Clear(EffectEntry effect, EffectContext context)
    {
        if (!effect.EffectType.TryGetStatType(out var statType))
            return;

        context.Target?.RemoveStatValue(statType, effect);
    }
}
