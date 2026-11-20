public static class EffectTriggerTypeExtension
{
    /// <summary>
    /// 발동 조건 문구. None은 상시 효과라 조건이 없으므로 빈 문자열이다.
    /// </summary>
    public static string GetConditionText(this EffectTriggerType type)
    {
        if (type == EffectTriggerType.None)
            return string.Empty;

        return Localize.Get($"TRIGGER_{type.ToString().ToUpper()}");
    }
}
