using System.Collections.Generic;

public class Ally : Character
{
    public ECharacterType CharType { get; private set; }
    public CharacterExpTranslation XPTranslation;

    private List<EquippableItem> m_EquipItems = new(5);
    private Dictionary<string, int> m_SkillLevelDic;

    public int LevelModPhase { get; private set; }
    public int DupeModPhase { get; private set; }
    public int DupeTokenOwned { get; private set; }
    public bool CanAscend => XPTranslation.NeedsAscension(LevelModPhase)
        && LevelModPhase < UserManager.Instance.WorldModPhase;

    private Dictionary<string, int> m_DupeSkillBonusDic = new();
    private List<PassiveSkillSO> m_DupePassives = new();
    private HashSet<WearableSetsEffectEntry> m_ActiveSets = new();

    public Ally(ECharacterType type)
    {
        m_SkillLevelDic = new();
        XPTranslation = new();
        CharType = type;
        var baseInfo = SODatabase.GetCharacterBaseInfo(type);
        Init(XPTranslation.CurLevel, baseInfo.StatGrowthID, LevelModPhase);
        ApplyAllDupeEffects();
    }

    private void Init(int level, string growthID, int modPhase)
    {
        var statScaling = BDatabase.CharacterTable.GetStatScaling(growthID, level, modPhase);
        var baseStat = BDatabase.CharacterTable.BaseStatDic[CharType];

        MaxHp.SetBaseValue(baseStat.HP * statScaling.HPScale);
        MaxMp.SetBaseValue(baseStat.MP * statScaling.MPScale);
        Attack.SetBaseValue(baseStat.ATK * statScaling.ATKScale);
        Defense.SetBaseValue(baseStat.DEF * statScaling.DEFScale);
        Speed.SetBaseValue(baseStat.Speed);
    }

    public Ally(CharacterRecord record,
        Dictionary<long, CharacterEquipRecord> equipRecords)
    {
        XPTranslation = new();

        var inventoryContents = ContentsManager.Instance.Get<InventoryContents>();
        foreach (var pair in equipRecords)
        {
            var item = inventoryContents.FindEquippableItem(pair.Value.ItemType, pair.Key);
            if (item == null)
                throw new System.Exception($"[Suspect] Item not found in inventory. {pair.Key} {pair.Value}");

            item.Equip(this);
            m_EquipItems.Add(item);
        }

        LevelModPhase = record.LevelModPhase;
        DupeModPhase = record.DupeTokenUsed;
        DupeTokenOwned = record.DupeTokenOwned;
        m_SkillLevelDic = record.SkillLevelDic;
        CharType = record.Type;
        XPTranslation.AddExp(record.TotalExp);
        var baseInfo = SODatabase.GetCharacterBaseInfo(record.Type);
        Init(XPTranslation.CurLevel, baseInfo.StatGrowthID, LevelModPhase);
        ApplyAllDupeEffects();
        RefreshSetsEffects();
    }

    public void GainXp(long xp)
    {
        if (XPTranslation.AddExp(xp))
        {
            var baseInfo = SODatabase.GetCharacterBaseInfo(CharType);
            Init(XPTranslation.CurLevel, baseInfo.StatGrowthID, LevelModPhase);
        }
    }

    public void Ascend()
    {
        LevelModPhase++;
        var baseInfo = SODatabase.GetCharacterBaseInfo(CharType);
        Init(XPTranslation.CurLevel, baseInfo.StatGrowthID, LevelModPhase);
    }

    public void UpgradeDupePhase()
    {
        var baseInfo = SODatabase.GetCharacterBaseInfo(CharType);
        if (DupeModPhase >= baseInfo.DupeEffects.Count)
            return;

        ApplyDupeEffect(baseInfo.DupeEffects[DupeModPhase]);
        DupeModPhase++;
    }

    public IReadOnlyList<PassiveSkillSO> GetDupePassives()
    {
        return m_DupePassives;
    }

    private void ApplyAllDupeEffects()
    {
        var baseInfo = SODatabase.GetCharacterBaseInfo(CharType);
        for (int i = 0; i < DupeModPhase && i < baseInfo.DupeEffects.Count; i++)
        {
            ApplyDupeEffect(baseInfo.DupeEffects[i]);
        }
    }

    private void ApplyDupeEffect(DupeEffectSO effect)
    {
        if (effect is DupeStatSO stat)
            AddStatValue(stat.StatType, stat.Value, stat);
        else if (effect is DupeSkillLevelSO skill)
            m_DupeSkillBonusDic[skill.SkillID] = skill.BonusLevel;
        else if (effect is DupePassiveSO passive)
            m_DupePassives.Add(passive.Passive);
    }

    public void EquipItem(EquippableItem item, ECharacterType type)
    {
        var contents = ContentsManager.Instance.Get<CharacterDataContents>();
        var equippedItem = m_EquipItems.Find(i => i.Info.Base.Type == item.Info.Base.Type);
        if (equippedItem != null)
            UnEquipItem(equippedItem);

        item.Equip(this);
        m_EquipItems.Add(item);
        contents.AddEquipItemOnwer(item.InstanceID, type, item.Info.Base.Type);
        RefreshSetsEffects();
    }

    public void UnEquipItem(EquippableItem item)
    {
        var contents = ContentsManager.Instance.Get<CharacterDataContents>();
        item.UnEquip(this);
        m_EquipItems.Remove(item);
        contents.RemoveEquipItemOnwer(item.InstanceID);
        RefreshSetsEffects();
    }

    private void RefreshSetsEffects()
    {
        var equippedCountDic = new Dictionary<WearableSetsEffectEntry, int>();
        foreach (var item in m_EquipItems)
        {
            if (item.Info is not WearableItemData wearable || wearable.SetsEffect == null)
                continue;

            equippedCountDic.TryGetValue(wearable.SetsEffect, out int count);
            equippedCountDic[wearable.SetsEffect] = count + 1;
        }

        //해제된 세트를 먼저 걷어낸다. 순회 중 수정되지 않도록 복사본을 돈다.
        var activeSets = new List<WearableSetsEffectEntry>(m_ActiveSets);
        foreach (var entry in activeSets)
        {
            equippedCountDic.TryGetValue(entry, out int equipped);
            if (equipped >= entry.TotalCount)
                continue;

            EquipEffectApplier.Remove(this, entry.Effects, entry.TriggerTypes, entry);
            m_ActiveSets.Remove(entry);
        }

        foreach (var pair in equippedCountDic)
        {
            var entry = pair.Key;
            if (entry.TotalCount == 0 || pair.Value < entry.TotalCount
                || m_ActiveSets.Contains(entry))
                continue;

            EquipEffectApplier.Apply(this, entry.Effects, entry.TriggerTypes, entry);
            m_ActiveSets.Add(entry);
        }
    }

    public IEnumerable<WearableSetsEffectEntry> GetActiveSetsEntries()
    {
        return m_ActiveSets;
    }

    /// <summary>
    /// 장비(무기/방어구)와 발동 중인 세트 효과가 해당 스탯에 더해준 수치.
    /// 돌파 등 장비 외 보정은 제외된다.
    /// </summary>
    public float GetEquipContribution(EBonusStatType statType)
    {
        return GetStatByType(statType).GetContribution(IsEquipSource);
    }

    private bool IsEquipSource(StatModifier mod)
    {
        return mod.Source is EquippableItem || mod.Source is WearableSetsEffectEntry;
    }

    public void LevelUpSkill(string skillID)
    {
        int curLevel = GetBaseSkillLevel(skillID);
        SetSkillLevel(skillID, curLevel + 1);
    }

    public void SetSkillLevel(string skillID, int level)
    {
        m_SkillLevelDic[skillID] = level;
    }

    public int GetSkillLevel(string skillID)
    {
        int level = GetBaseSkillLevel(skillID);
        if (m_DupeSkillBonusDic.TryGetValue(skillID, out int bonus))
            level += bonus;

        return level;
    }

    public int GetBaseSkillLevel(string skillID)
    {
        if (m_SkillLevelDic.TryGetValue(skillID, out int level))
            return level;
        else
            return 1;
    }

    public IReadOnlyList<EquippableItem> GetEquipItems()
    {
        return m_EquipItems;
    }

    public void AddDupeTokenOnwed()
    {
        DupeTokenOwned++;
    }

    public CharacterRecord ToRecord()
    {
        return new CharacterRecord
        {
            Type = CharType,
            SkillLevelDic = m_SkillLevelDic,
            TotalExp = XPTranslation.GetTotalAccumulatedExp(),
            LevelModPhase = LevelModPhase,
            DupeTokenUsed = DupeModPhase,
            DupeTokenOwned = DupeTokenOwned
        };
    }
}
