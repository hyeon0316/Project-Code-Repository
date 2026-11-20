using Cysharp.Threading.Tasks;

public interface IEffectHandler
{
    public void Execute(EffectEntry effect);
}

public interface ITargetEffectHandler
{
    public void Execute(EffectEntry effect, EffectContext context);
}

public interface IStatusEffectHandler
{
    public void Execute(EffectEntry effect, EffectContext context);
    public void Clear(EffectEntry effect, EffectContext context);
}

public interface IDotEffectHandler
{
    public void Execute(EffectEntry effect, EffectContext context);
    public UniTask Dot(EffectEntry effect, EffectContext context, int stackCount);
}
