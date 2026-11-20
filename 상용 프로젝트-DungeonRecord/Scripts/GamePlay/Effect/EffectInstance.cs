using UnityEngine;

public class EffectInstance
{
    public EffectEntry Effect;
    /// <summary>같은 키로 다시 부여되면 새 인스턴스 대신 이 인스턴스에 합쳐진다.</summary>
    public string Key { get; private set; }
    public int StackCount { get; private set; }
    public int RemainingTurns { get; private set; }
    public EffectContext Context => m_Context;
    /// <summary>Duration과 Stack이 모두 0인 효과는 전투가 끝날 때까지 유지된다.</summary>
    public bool IsExpired
    {
        get
        {
            return (Effect.Duration > 0 && RemainingTurns == 0)
                || (Effect.Stack > 0 && StackCount == 0);
        }
    }
    /// <summary>중첩형은 중첩 수, 지속형은 남은 턴. 둘 다 아니면 0.</summary>
    public int DisplayCount
    {
        get { return Effect.Stack > 0 ? StackCount : RemainingTurns; }
    }
    private EffectContext m_Context;

    public EffectInstance(EffectEntry effect, EffectContext context, string key = "")
    {
        Effect = effect;
        m_Context = context;
        Key = key;
    }

    public void Apply()
    {
        Apply(Effect);
    }

    /// <summary>
    /// 부여 1회를 반영한다. 같은 키의 다른 효과가 합쳐질 때는 applied가 그 효과다.
    /// 지속 턴은 applied.Duration으로 갱신하되 남은 턴이 더 길면 유지한다.
    /// 중첩은 applied.Stack만큼 쌓되 applied.MaxStack에서 멈추고, 이미 그보다 많으면 줄이지 않는다.
    /// 스택이 늘었으면 true.
    /// </summary>
    public bool Apply(EffectEntry applied)
    {
        if (applied.Duration > 0)
            RemainingTurns = Mathf.Max(RemainingTurns, applied.Duration);

        if (applied.Stack == 0)
            return false;

        int next = StackCount + applied.Stack;
        if (applied.MaxStack > 0)
            next = Mathf.Min(next, applied.MaxStack);

        if (next <= StackCount)
            return false;

        StackCount = next;
        return true;
    }

    /// <summary>공유 중첩에서 스택을 끌어올린 효과와 시전자로 교체한다. 피해와 기여도가 이를 따른다.</summary>
    public void SetSource(EffectEntry effect, EffectContext context)
    {
        Effect = effect;
        m_Context = context;
    }

    /// <summary>저장된 진행 기록에서 그대로 복원한다. 상한을 다시 적용하지 않는다.</summary>
    public void Restore(int stackCount, int remainingTurns)
    {
        StackCount = stackCount;
        RemainingTurns = remainingTurns;
    }

    public void ConsumeTurn()
    {
        if (RemainingTurns == 0)
            return;

        RemainingTurns--;
    }

    public void ConsumeStack()
    {
        if (StackCount == 0)
            return;

        StackCount--;
    }
}
