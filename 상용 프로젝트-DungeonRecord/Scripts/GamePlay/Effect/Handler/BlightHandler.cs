using Cysharp.Threading.Tasks;

public class BlightHandler : IDotEffectHandler
{
    public void Execute(EffectEntry effect, EffectContext context)
    {
        context.Target.ApplyBlight(context, effect);
    }

    public async UniTask Dot(EffectEntry effect, EffectContext context, int stackCount)
    {
        context.Target.TakeDamage(stackCount, attacker: context.Caster);
        context.Target.PlayHit();
        await UniTask.WaitForSeconds(1);
    }
}
