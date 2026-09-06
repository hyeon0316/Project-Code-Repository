using UnityEngine;

public class EffectInstance
{
    public EffectEntry Effect;
    public bool IsEmpty => StackCount == 0;
    public int StackCount { get; private set; }
    public EffectContext Context => m_Context;
    private EffectContext m_Context;

    public EffectInstance(EffectEntry effect, EffectContext context)
    {
        Effect = effect;
        m_Context = context;
    }

    /// <summary>
    /// 스택을 Effect.Stack만큼 쌓는다. 상한은 MaxStack이며 0이면 무한 중첩.
    /// 이미 상한이라 한 개도 못 쌓으면 false.
    /// </summary>
    public bool AddStack()
    {
        int max = Effect.GetMaxStack();
        if (max > 0 && StackCount >= max)
            return false;

        StackCount += Effect.Stack;
        if (max > 0)
            StackCount = Mathf.Min(StackCount, max);

        return true;
    }

    /// <summary>저장된 진행 기록에서 스택을 그대로 복원한다.</summary>
    public void RestoreStack(int stackCount)
    {
        StackCount = stackCount;
    }

    /// <summary>스택 1개를 소모한다. 소모 시점은 EffectType별 트리거가 결정한다.</summary>
    public void ConsumeStack()
    {
        if (StackCount == 0)
            return;

        StackCount--;
    }
}
