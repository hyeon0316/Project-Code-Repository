using Cysharp.Threading.Tasks;

public class BleedHandler : IDotEffectHandler
{
    public void Execute(EffectEntry effect, EffectContext context)
    {
        context.Target.ApplyBleed(context, effect);
    }

    public async UniTask Dot(EffectEntry effect, EffectContext context, int stackCount)
    {
        float damage = effect.Value + context.Caster.GetAttack() * GlobalVariable.BLEED_ATK_RATIO;
        context.Target.TakeDamage(damage, attacker: context.Caster);
        context.Target.PlayHit();
        await UniTask.WaitForSeconds(1);
    }
}
