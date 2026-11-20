/// <summary>
/// 상태이상 저항 굴림. 저항에 성공하면 효과가 부여되지 않는다.
/// </summary>
public static class ResistRule
{
    public static bool IsResisted(EffectType type, float resistRate, IRandomSource random)
    {
        if (!type.TryGetResistType(out _))
            return false;

        if (resistRate >= 1f)
            return true;

        return random.Value() < resistRate;
    }
}
