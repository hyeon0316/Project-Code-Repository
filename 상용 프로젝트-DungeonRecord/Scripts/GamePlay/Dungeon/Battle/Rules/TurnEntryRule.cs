public enum ETurnEntry
{
    SkipDead,
    DotDeath,
    SkipStunned,
    AllyTurn,
    EnemyTurn,
}

/// <summary>
/// 턴이 돌아온 유닛이 실제로 행동할 수 있는지 판정한다.
/// </summary>
public static class TurnEntryRule
{
    public static ETurnEntry ResolveEntry(IBattleUnit unit)
    {
        if (unit.IsDead)
            return ETurnEntry.SkipDead;

        return unit.IsAlly ? ETurnEntry.AllyTurn : ETurnEntry.EnemyTurn;
    }

    public static void ConsumeTurnEndStacks(IBattleUnit unit)
    {
        if (unit.IsDead)
            return;

        unit.UpdateEffectStacks();
    }

    public static ETurnEntry ResolveAfterDot(IBattleUnit unit)
    {
        if (unit.IsDead)
            return ETurnEntry.DotDeath;

        if (unit.IsStun)
            return ETurnEntry.SkipStunned;

        return unit.IsAlly ? ETurnEntry.AllyTurn : ETurnEntry.EnemyTurn;
    }
}
