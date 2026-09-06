
using System;

public enum EStatValueType
{
    Flat,
    Percent,
    /// <summary>
    /// 마지막에 더해지는 값
    /// </summary>
    FixedFlat,
    /// <summary>
    /// 마지막에 곱해지는 값
    /// </summary>
    FixedPercent,
}

public readonly struct StatModifier : IEquatable<StatModifier>
{
    public readonly float Value;
    public readonly EStatValueType Type;
    public readonly int CaculateOrder;
    public readonly object Source;

    private readonly int m_HashCode;

    public StatModifier(float value, EStatValueType type, object source)
    {
        Value = value;
        Type = type;
        CaculateOrder = (int)type;
        Source = source;
        m_HashCode = HashCode.Combine(Value, Type, CaculateOrder, Source);
    }

    public bool Equals(StatModifier other)
    {
        return Value == other.Value && Type == other.Type && CaculateOrder == other.CaculateOrder
            && Source == other.Source;
    }

    public override bool Equals(object obj)
    {
        return obj is StatModifier other && Equals(other);
    }

    public override int GetHashCode()
    {
        return m_HashCode;
    }
}
