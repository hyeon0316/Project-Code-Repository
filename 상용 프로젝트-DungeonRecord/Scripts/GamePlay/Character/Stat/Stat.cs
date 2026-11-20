using System;
using UnityEngine;
using System.Collections.Generic;


public abstract class Stat
{
    public float ResultValue
    {
        get
        {
            if (m_IsDirty)
            {
                m_ResultValue = CaculateValue();
                m_IsDirty = false;
            }
            return m_ResultValue;
        }
    }

    public float BaseValue
    {
        get
        {
            return m_BaseValue;
        }
    }

    protected float m_BaseValue;
    protected float m_ResultValue;
    protected bool m_IsDirty = true;
    protected List<StatModifier> m_StatModifiers = new();
    protected Predicate<StatModifier> m_Predicate;
    protected object m_SourceToRemove;

    protected abstract float CaculateValue();

    /// <summary>
    /// 특정 modifier들을 뺀 상태의 값. 기여분을 역산하는 데 쓴다.
    /// </summary>
    protected abstract float CaculateValueWithout(Predicate<StatModifier> exclude);

    public Stat()
    {
        m_StatModifiers = new List<StatModifier>();
        m_Predicate = CompareSourceToRemove;
    }

    public void SetBaseValue(float value)
    {
        m_BaseValue = value;
        m_IsDirty = true;
    }

    public void AddModifier(StatModifier mod)
    {
        m_IsDirty = true;
        InsertModifier(mod);
    }

    protected virtual void InsertModifier(StatModifier mod)
    {
        m_StatModifiers.Add(mod);
    }

    public bool RemoveModifier(StatModifier mod)
    {
        m_IsDirty = true;
        return m_StatModifiers.Remove(mod);
    }

    public bool RemoveAllModifiersFromSource(object source)
    {
        m_SourceToRemove = source;
        int removeCnt = m_StatModifiers.RemoveAll(m_Predicate);
        m_SourceToRemove = null;

        if (removeCnt > 0)
        {
            m_IsDirty = true;
            return true;
        }
        return false;
    }

    private bool CompareSourceToRemove(StatModifier m)
    {
        return m.Source == m_SourceToRemove;
    }

    /// <summary>
    /// match에 해당하는 modifier들이 실제로 올린 수치.
    /// Percent는 base에 곱해지므로 단순 합산이 아니라 뺀 값과의 차이로 구한다.
    /// </summary>
    public float GetContribution(Predicate<StatModifier> match)
    {
        return ResultValue - CaculateValueWithout(match);
    }

    public override string ToString()
    {
        return ResultValue.ToString();
    }
}