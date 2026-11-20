using UnityEngine;

public static class DamageRule
{
    public static int CalcDamage(float rawValue, float defense, bool ignoreDFS)
    {
        float value = ignoreDFS ? rawValue : rawValue * (100 / (defense + 100));
        return Mathf.RoundToInt(value);
    }

    public static float ApplyDamage(float curHp, int damage)
    {
        return Mathf.Max(0, curHp - damage);
    }

    public static float ApplyHeal(float curHp, float healValue, float maxHp)
    {
        return Mathf.Min(curHp + Mathf.RoundToInt(healValue), Mathf.RoundToInt(maxHp));
    }
}
