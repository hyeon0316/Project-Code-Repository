using System.Collections.Generic;

/// <summary>
/// 한 라운드의 행동 순서를 정한다.
/// 속도 내림차순 → 아군 우선 → 원래 인덱스 순.
/// </summary>
public static class TurnOrderRule
{
    /// <summary>
    /// order를 행동 순서대로 재배치한다.
    /// 속도에는 난수가 섞여 호출마다 값이 달라지므로, 비교 중에 다시 굴리지 않도록
    /// 시작 전에 한 번만 굴려 고정한다.
    /// </summary>
    public static void Sort<T>(List<T> order) where T : IBattleUnit
    {
        var speeds = new Dictionary<T, float>(order.Count);
        var originalIndex = new Dictionary<T, int>(order.Count);
        for (int i = 0; i < order.Count; i++)
        {
            speeds[order[i]] = order[i].GetResultSpeed();
            originalIndex[order[i]] = i;
        }

        order.Sort((a, b) => Compare(a, b, speeds, originalIndex));
    }

    private static int Compare<T>(T a, T b,
        IReadOnlyDictionary<T, float> speeds,
        IReadOnlyDictionary<T, int> originalIndex) where T : IBattleUnit
    {
        //speed가 제일 높은 값 우선
        int speedComparison = speeds[b].CompareTo(speeds[a]);
        if (speedComparison != 0)
            return speedComparison;

        //아군과 적이 같은 speed일때 아군 우선
        int typeComparison = b.IsAlly.CompareTo(a.IsAlly);
        if (typeComparison != 0)
            return typeComparison;

        //인덱스가 제일 낮은 값 우선
        return originalIndex[a].CompareTo(originalIndex[b]);
    }
}
