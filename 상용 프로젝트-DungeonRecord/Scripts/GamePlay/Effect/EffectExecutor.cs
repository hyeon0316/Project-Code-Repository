using System.Collections.Generic;

public static class EffectExecutor
{
    private static readonly Dictionary<EffectType, IEffectHandler> m_Handler = new()
    {
        {EffectType.Stamina_Recovery, new StaminaRecoveryHandler()},
    };
    private static readonly Dictionary<EffectType, ITargetEffectHandler> m_TargetHandler = new()
    {
        {EffectType.Damage, new DamageHandler()},
        {EffectType.Hp_Recovery_Flat, new HpRecoveryHandler()},
        {EffectType.Hp_Recovery_Percent, new HpRecoveryHandler()},
        {EffectType.Mp_Recovery_Percent, new MpRecoveryHandler()},
        {EffectType.Hp_Loss_Percent, new HpLossHandler()},
    };
    private static readonly Dictionary<EffectType, IDotEffectHandler> m_DotHandler = new()
    {
        {EffectType.Bleed, new BleedHandler()},
        {EffectType.Blight, new BlightHandler()},
    };
    private static readonly Dictionary<EffectType, IStatusEffectHandler> m_StatusHandler = CreateStatusHandlers();

    private static Dictionary<EffectType, IStatusEffectHandler> CreateStatusHandlers()
    {
        var dic = new Dictionary<EffectType, IStatusEffectHandler>
        {
            {EffectType.Stun, new StunHandler()},
        };

        var statBuffHandler = new StatBuffHandler();
        foreach (EffectType type in System.Enum.GetValues(typeof(EffectType)))
        {
            if (type.TryGetStatType(out _))
                dic[type] = statBuffHandler;
        }

        return dic;
    }

    public static void Execute(EffectEntry effect)
    {
        if (m_Handler.TryGetValue(effect.EffectType, out var handler))
            handler.Execute(effect);
    }

    public static void Execute(EffectEntry effect, EffectContext context)
    {
        if (m_TargetHandler.TryGetValue(effect.EffectType, out var targetHandler))
            targetHandler.Execute(effect, context);
        else if (m_StatusHandler.ContainsKey(effect.EffectType) || m_DotHandler.ContainsKey(effect.EffectType))
            context.Target.ApplyEffect(context, effect);
    }

    public static void OnApply(EffectInstance instance)
    {
        if (m_StatusHandler.TryGetValue(instance.Effect.EffectType, out var handler))
            handler.OnApply(instance);
    }

    public static bool IsSharedStack(EffectType type)
    {
        return m_DotHandler.TryGetValue(type, out var handler) && handler.IsSharedStack;
    }

    public static float CalcDotDamage(EffectEntry effect, EffectContext context, int stackCount)
    {
        if (m_DotHandler.TryGetValue(effect.EffectType, out var handler))
            return handler.CalcDotDamage(effect, context, stackCount);

        return 0f;
    }

    public static void OnRemove(EffectInstance instance)
    {
        if (m_StatusHandler.TryGetValue(instance.Effect.EffectType, out var handler))
            handler.OnRemove(instance);
    }
}
