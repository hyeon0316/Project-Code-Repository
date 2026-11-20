using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EffectRecord
{
    public string EffectID;
    public string CasterID;
    public int StackCount;
    public float ScalingValue;

    public EffectRecord()
    {
    }

    public EffectRecord(string effectID, EffectInstance instance)
    {
        EffectID = effectID;
        StackCount = instance.StackCount;
        ScalingValue = instance.Context.ScalingValue;
        var caster = instance.Context.Caster as DungeonUnit;
        CasterID = caster != null ? caster.ID : string.Empty;
    }
}

[System.Serializable]
public class DungeonCharacterRecord
{
    public int TransformIndex;
    public string ID;
    public float Hp;
    public float Mp;
    public bool IsStun;
    public int StunResistTier;
    public List<EffectRecord> Effects = new();

    public DungeonCharacterRecord()
    {
    }

    public DungeonCharacterRecord(DungeonUnit character)
    {
        Set(character);
    }

    public void Set(DungeonUnit character)
    {
        ID = character.ID;
        Hp = character.CurHp;
        Mp = character.CurMp;
        TransformIndex = character.TransformIndex;
        IsStun = character.IsStun;
        StunResistTier = character.StunResistTier;

        Effects.Clear();
        foreach (var kvp in character.Effects)
        {
            Effects.Add(new EffectRecord(kvp.Key, kvp.Value));
        }
    }
}

[System.Serializable]
public class BattleProgress
{
    public List<DungeonCharacterRecord> AllyRecords = new(GlobalVariable.PARTY_MEMER_MAX_COUNT);
    public List<DungeonCharacterRecord> DeadAllyRecords = new(GlobalVariable.PARTY_MEMER_MAX_COUNT);
    public List<DungeonCharacterRecord> EnemyRecords = new(GlobalVariable.PARTY_MEMER_MAX_COUNT);
    public List<DungeonCharacterRecord> DeadEnemyRecords = new(GlobalVariable.PARTY_MEMER_MAX_COUNT);
    public List<string> PriorityOrderRecords = new(GlobalVariable.PARTY_MEMER_MAX_COUNT * 2);
    /// <summary>스테이지 진입 시점 생존 아군. 패배 후 재시작 롤백에 쓴다.</summary>
    public List<DungeonCharacterRecord> StageEntryAllyRecords = new(GlobalVariable.PARTY_MEMER_MAX_COUNT);
    /// <summary>스테이지 진입 시점에 이미 사망해 있던 아군. 재시작해도 부활하지 않는다.</summary>
    public List<DungeonCharacterRecord> StageEntryDeadAllyRecords = new(GlobalVariable.PARTY_MEMER_MAX_COUNT);
    /// <summary>아군 ID별 누적 데미지. 전투별 MVP 선정에 쓰인다.</summary>
    public Dictionary<string, float> DamageRecords = new(GlobalVariable.PARTY_MEMER_MAX_COUNT);
    /// <summary>던전 전체 기준 아군 ID별 누적 데미지. 결과 화면 MVP 선정에 쓰인다.</summary>
    public Dictionary<string, float> TotalDamageRecords = new(GlobalVariable.PARTY_MEMER_MAX_COUNT);

    /// <summary>호출부가 아군 여부를 판별해 넘긴다. 아군 ID는 ECharacterType 이름과 동일하다.</summary>
    public void RecordDamage(string attackerID, float amount)
    {
        if (amount <= 0)
            return;

        DamageRecords.TryGetValue(attackerID, out float prev);
        DamageRecords[attackerID] = prev + amount;

        TotalDamageRecords.TryGetValue(attackerID, out float totalPrev);
        TotalDamageRecords[attackerID] = totalPrev + amount;
    }

    public string GetMvpAllyID()
    {
        return FindMaxDamageAllyID(DamageRecords);
    }

    public string GetTotalMvpAllyID()
    {
        return FindMaxDamageAllyID(TotalDamageRecords);
    }

    private string FindMaxDamageAllyID(Dictionary<string, float> records)
    {
        string mvpID = string.Empty;
        float maxDamage = 0f;
        foreach (var kvp in records)
        {
            if (kvp.Value <= maxDamage)
                continue;

            maxDamage = kvp.Value;
            mvpID = kvp.Key;
        }
        return mvpID;
    }

    public void UpdateState(List<DungeonUnit> allies, List<DungeonUnit> deadAllies
        , List<DungeonUnit> enemies, List<DungeonUnit> deadEnemies, List<EnemyUnit> reserveEnemies)
    {
        AllyRecords.Clear();
        EnemyRecords.Clear();
        DeadAllyRecords.Clear();
        DeadEnemyRecords.Clear();
        foreach (var c in allies)
        {
            AllyRecords.Add(new DungeonCharacterRecord(c));
        }
        foreach (var c in enemies)
        {
            EnemyRecords.Add(new DungeonCharacterRecord(c));
        }
        if (reserveEnemies != null)
        {
            foreach (var c in reserveEnemies)
            {
                EnemyRecords.Add(new DungeonCharacterRecord(c));
            }
        }
        foreach (var c in deadAllies)
        {
            DeadAllyRecords.Add(new DungeonCharacterRecord(c));
        }
        foreach (var c in deadEnemies)
        {
            DeadEnemyRecords.Add(new DungeonCharacterRecord(c));
        }
    }

    public void SetPriorityOrder(List<DungeonUnit> order)
    {
        PriorityOrderRecords.Clear();
        foreach (var c in order)
        {
            PriorityOrderRecords.Add(c.ID);
        }
    }

    public void End()
    {
        EnemyRecords.Clear();
        PriorityOrderRecords.Clear();
    }

    public void Clear()
    {
        AllyRecords.Clear();
        EnemyRecords.Clear();
        PriorityOrderRecords.Clear();
        DamageRecords.Clear();
        TotalDamageRecords.Clear();
    }
}
