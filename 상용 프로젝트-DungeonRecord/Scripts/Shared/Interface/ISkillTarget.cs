public interface ISkillTarget
{
    public int TransformIndex { get; set; }
    public void TakeDamage(float value, bool ignoreDFS = true, ISkillTarget attacker = null);
    public void ApplyStun(EffectContext context, EffectEntry effect);
    public void RemoveStun();
    public void ApplyStunResistBuff();
    public void ApplyBleed(EffectContext context, EffectEntry effect);
    public void ApplyBlight(EffectContext context, EffectEntry effect);
    public float GetAttack();
    public float GetMaxHp();
    public void AddStatValue(EBonusStatType statType, float value, object source, EffectContext context, EffectEntry effect);
    public void AddStatValue(EBonusStatType statType, float value, object source);
    public void RemoveStatValue(EBonusStatType statType, object source);
    public void Heal(float value);
    public void PlayHit();
}