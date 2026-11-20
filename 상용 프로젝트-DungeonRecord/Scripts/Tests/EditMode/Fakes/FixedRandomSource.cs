using System.Collections.Generic;

/// <summary>
/// 결과를 지정할 수 있는 테스트용 난수 공급원.
/// 큐에 값을 넣어두면 순서대로 소비하고, 비면 고정값으로 돌아간다.
/// </summary>
public class FixedRandomSource : IRandomSource
{
    public float FixedValue;
    public int FixedRange;

    public int ValueCallCount { get; private set; }
    public int RangeCallCount { get; private set; }

    private readonly Queue<float> m_ValueQueue = new();
    private readonly Queue<int> m_RangeQueue = new();

    public FixedRandomSource(float fixedValue = 0f, int fixedRange = 0)
    {
        FixedValue = fixedValue;
        FixedRange = fixedRange;
    }

    /// <summary>Value()가 순서대로 돌려줄 값들.</summary>
    public FixedRandomSource EnqueueValues(params float[] values)
    {
        foreach (var v in values)
            m_ValueQueue.Enqueue(v);
        return this;
    }

    /// <summary>Range()가 순서대로 돌려줄 값들. 범위 클램프는 하지 않는다.</summary>
    public FixedRandomSource EnqueueRanges(params int[] values)
    {
        foreach (var v in values)
            m_RangeQueue.Enqueue(v);
        return this;
    }

    public int Range(int minInclusive, int maxExclusive)
    {
        RangeCallCount++;
        return m_RangeQueue.Count > 0 ? m_RangeQueue.Dequeue() : FixedRange;
    }

    public float Value()
    {
        ValueCallCount++;
        return m_ValueQueue.Count > 0 ? m_ValueQueue.Dequeue() : FixedValue;
    }
}
