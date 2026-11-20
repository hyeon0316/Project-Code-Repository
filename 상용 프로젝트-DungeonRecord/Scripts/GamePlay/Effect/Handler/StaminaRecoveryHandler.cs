public class StaminaRecoveryHandler : IEffectHandler
{
    public void Execute(EffectEntry effect)
    {
        ContentsManager.Instance.Get<StaminaContents>().AddStamina((int)effect.Value);
    }

}