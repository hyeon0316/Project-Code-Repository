using System;
using UnityEngine;
using System.Collections.Generic;

public class PercentageStat : Stat
{

    protected override float CaculateValue()
    {
        return Caculate(null);
    }

    protected override float CaculateValueWithout(Predicate<StatModifier> exclude)
    {
        return Caculate(exclude);
    }

    private float Caculate(Predicate<StatModifier> exclude)
    {
        float resultValue = m_BaseValue;

        for (int i = 0; i < m_StatModifiers.Count; i++)
        {
            var mod = m_StatModifiers[i];
            if (exclude != null && exclude(mod))
                continue;

            resultValue += mod.Value;
        }
        return resultValue;
    }

    public override string ToString()
    {
        return (ResultValue * 100).ToString("0.#") + "%";
    }
}

