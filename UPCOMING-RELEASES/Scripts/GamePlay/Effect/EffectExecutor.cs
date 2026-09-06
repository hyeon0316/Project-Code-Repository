using Cysharp.Threading.Tasks;
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
        else if (m_StatusHandler.TryGetValue(effect.EffectType, out var statusHandler))
            statusHandler.Execute(effect, context);
        else if (m_DotHandler.TryGetValue(effect.EffectType, out var dotHandler))
            dotHandler.Execute(effect, context);
    }

    public static async UniTask Dot(EffectEntry effect, EffectContext context, int stackCount)
    {
        if (m_DotHandler.TryGetValue(effect.EffectType, out var handler))
            await handler.Dot(effect, context, stackCount);
    }

    public static void Clear(EffectEntry effect, EffectContext context)
    {
        if (m_StatusHandler.TryGetValue(effect.EffectType, out var handler))
            handler.Clear(effect, context);
    }
}
