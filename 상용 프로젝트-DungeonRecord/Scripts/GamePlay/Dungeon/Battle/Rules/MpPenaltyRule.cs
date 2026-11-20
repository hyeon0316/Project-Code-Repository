using UnityEngine;

/// <summary>
/// MP가 음수로 내려갔을 때 붙는 패널티 단계 판정.
/// 단계는 누적이라 3단계면 1·2·3이 모두 활성이다.
/// </summary>
public static class MpPenaltyRule
{
    public const int MAX_LEVEL = 4;
    public const float MIN_MP = -100f;

    private static readonly float[] THRESHOLDS = { 1f, 25f, 50f, 75f };

    public static float Spend(float curMp, int cost)
    {
        return Mathf.Max(MIN_MP, curMp - cost);
    }

    public static bool IsActive(int penaltyLevel, float deficitMp)
    {
        if (penaltyLevel < 1 || penaltyLevel > MAX_LEVEL)
            return false;

        return deficitMp >= THRESHOLDS[penaltyLevel - 1];
    }

    public static int GetHighestActiveLevel(float deficitMp)
    {
        int highest = 0;
        for (int level = 1; level <= MAX_LEVEL; level++)
        {
            if (IsActive(level, deficitMp))
                highest = level;
        }
        return highest;
    }
}
