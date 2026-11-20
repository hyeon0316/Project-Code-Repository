public class StatBuffHandler : IStatusEffectHandler
{
    /// <summary>같은 효과가 출처별로 여러 개 걸릴 수 있으므로 보정의 source는 인스턴스다.</summary>
    public void OnApply(EffectInstance instance)
    {
        var effect = instance.Effect;
        if (!effect.EffectType.TryGetStatType(out var statType))
            return;

        instance.Context.Target?.AddStatValue(statType, effect.GetSignedValue(), instance);
    }

    public void OnRemove(EffectInstance instance)
    {
        if (!instance.Effect.EffectType.TryGetStatType(out var statType))
            return;

        instance.Context.Target?.RemoveStatValue(statType, instance);
    }
}
