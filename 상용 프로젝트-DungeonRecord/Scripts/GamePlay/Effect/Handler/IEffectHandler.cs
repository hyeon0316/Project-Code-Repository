public interface IEffectHandler
{
    public void Execute(EffectEntry effect);
}

public interface ITargetEffectHandler
{
    public void Execute(EffectEntry effect, EffectContext context);
}

/// <summary>인스턴스가 새로 생길 때와 사라질 때 할 일. 등록·갱신·중첩은 대상의 ApplyEffect가 맡는다.</summary>
public interface IStatusEffectHandler
{
    public void OnApply(EffectInstance instance);
    public void OnRemove(EffectInstance instance);
}

public interface IDotEffectHandler
{
    /// <summary>
    /// 피해가 시전자와 무관하면 true. 대상당 스택 하나를 공유하고,
    /// false면 시전자별로 따로 쌓여 먼저 부여된 것부터 소모된다.
    /// </summary>
    public bool IsSharedStack { get; }
    public float CalcDotDamage(EffectEntry effect, EffectContext context, int stackCount);
}
