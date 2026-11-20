using System.Collections.Generic;

public enum EWipeResult
{
    NoDeath,
    SomeDied,
    AllDied,
}

/// <summary>
/// 진영의 사망자를 정리하고 전멸 여부를 판정한다.
/// </summary>
public static class WipeRule
{

    public static EWipeResult MoveDeadAndCheck<T>(List<T> alive, List<T> dead, int reserveCount = 0)
        where T : IBattleUnit
    {
        var deadUnits = new List<T>();
        foreach (var unit in alive)
        {
            if (unit.IsDead)
                deadUnits.Add(unit);
        }

        if (deadUnits.Count == 0)
            return EWipeResult.NoDeath;

        foreach (var unit in deadUnits)
        {
            dead.Add(unit);
            alive.Remove(unit);
        }

        return alive.Count == 0 && reserveCount == 0 ? EWipeResult.AllDied : EWipeResult.SomeDied;
    }
}
