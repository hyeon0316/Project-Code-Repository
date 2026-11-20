using UnityEngine;

/// <summary>
/// 기절 저항 단계. 기절이 풀릴 때마다 한 단계씩 올라가 재기절을 어렵게 만든다.
/// </summary>
public static class StunResistRule
{
    private static readonly string[] BUFF_IDS =
    {
        "EFF_SYSTEM_STUN_BUFF_1",
        "EFF_SYSTEM_STUN_BUFF_2",
        "EFF_SYSTEM_STUN_BUFF_3",
        "EFF_SYSTEM_STUN_BUFF_4",
    };

    public static int MaxTier
    {
        get { return BUFF_IDS.Length; }
    }

    public static int GetNextTier(int curTier)
    {
        return Mathf.Min(curTier + 1, MaxTier);
    }

    public static string GetBuffID(int tier)
    {
        if (tier < 1 || tier > MaxTier)
            return string.Empty;

        return BUFF_IDS[tier - 1];
    }
}
