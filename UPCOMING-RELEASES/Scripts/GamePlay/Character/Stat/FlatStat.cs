using System;
using System.Collections.Generic;
using UnityEngine;

public class FlatStat : Stat
{
    /// <summary>
    /// CaculateOrder 오름차순을 유지하는 위치에 삽입한다.
    /// 같은 order끼리는 추가된 순서를 지켜야 FixedPercent 곱 순서가 흔들리지 않는다.
    /// </summary>
    protected override void InsertModifier(StatModifier mod)
    {
        int index = m_StatModifiers.Count;
        while (index > 0 && m_StatModifiers[index - 1].CaculateOrder > mod.CaculateOrder)
            index--;

        m_StatModifiers.Insert(index, mod);
    }

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
        float PercentAddSum = 0;

        for (int i = 0; i < m_StatModifiers.Count; i++)
        {
            var mod = m_StatModifiers[i];
            if (exclude != null && exclude(mod))
                continue;

            if (mod.Type == EStatValueType.Flat || mod.Type == EStatValueType.FixedFlat)
            {
                resultValue += mod.Value;
            }
            else if (mod.Type == EStatValueType.Percent)
            {
                PercentAddSum += mod.Value;
                //제외된 modifier를 건너뛰어도 Percent 구간이 끊기는 지점을 놓치지 않도록 다음 유효 항목을 본다.
                if (!HasNextPercent(i, exclude))
                {
                    resultValue *= (1 + PercentAddSum);
                    PercentAddSum = 0;
                }
            }
            else if (mod.Type == EStatValueType.FixedPercent)
            {
                resultValue *= mod.Value;
            }
        }
        return Mathf.RoundToInt(resultValue);
    }

    private bool HasNextPercent(int index, Predicate<StatModifier> exclude)
    {
        for (int i = index + 1; i < m_StatModifiers.Count; i++)
        {
            var mod = m_StatModifiers[i];
            if (exclude != null && exclude(mod))
                continue;

            return mod.Type == EStatValueType.Percent;
        }
        return false;
    }
}
