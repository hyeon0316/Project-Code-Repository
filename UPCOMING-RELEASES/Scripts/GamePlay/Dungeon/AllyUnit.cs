using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


public class AllyUnit : DungeonUnit
{
    public static AssetID AssetID = new("Assets/Prefabs/Character/Ally.prefab");

    public ECharacterType Type { get; private set; }

    /// <summary>
    /// 완성 세트가 가진 조건형 효과. 한 세트도 효과마다 트리거가 달라 개별 항목으로 편다.
    /// 장비는 아군만 착용하므로 여기서 관리한다.
    /// </summary>
    private List<(EffectTriggerType TriggerType, string EffectID)> m_SetsEffects = new();
    private HashSet<int> m_MpPenalties = new();

    public void Init(string id, DungeonCharacterRecord record = null)
    {
        ID = id;
        var type = Enum.Parse<ECharacterType>(id);
        var baseInfo = SODatabase.GetCharacterBaseInfo(type);
        State = ContentsManager.Instance.Get<CharacterDataContents>().FindOwnedCharacter(type);
        //시작 캐릭터는 플레이어 본인이므로 닉네임으로 표시한다.
        Name = UserManager.Instance.UserData.StartingCharacter == type ?
            UserManager.Instance.Profile.NickName : baseInfo.Name;
        CurHp = Mathf.RoundToInt(State.MaxHp.ResultValue);
        CurMp = State.MaxMp.ResultValue;
        SetFacingRight(true);
        Animator.SetController(baseInfo.DungeonAnimator);
        InitPassiveSkill(baseInfo.PassiveSkillGroup);
        InitDupePassive(State as Ally);
        InitSetsEffect(State as Ally);
        InitActiveSkill(baseInfo.ActiveSkills);
        ApplyOnBattleStartPassive();
        Type = type;

        if (record != null)
        {
            CurHp = record.Hp;
            CurMp = record.Mp;
            TransformIndex = record.TransformIndex;
        }
    }

    /// <summary>
    /// 완성된 세트에서 트리거가 지정된 효과만 등록한다.
    /// None인 효과는 장착 시점에 이미 스탯으로 적용되어 있다.
    /// </summary>
    private void InitSetsEffect(Ally ally)
    {
        m_SetsEffects.Clear();
        if (ally == null)
            return;

        foreach (var entry in ally.GetActiveSetsEntries())
        {
            for (int i = 0; i < entry.Effects.Length && i < entry.TriggerTypes.Length; i++)
            {
                if (entry.TriggerTypes[i] == EffectTriggerType.None)
                    continue;

                m_SetsEffects.Add((entry.TriggerTypes[i], entry.Effects[i]));
            }
        }
    }

    public override void TriggerPassive(EffectTriggerType triggerType,
        List<ISkillTarget> characters, List<ISkillTarget> targets)
    {
        base.TriggerPassive(triggerType, characters, targets);

        //세트 효과는 착용자 자신에게 걸리는 버프이므로 타겟 계산이 필요 없다.
        foreach (var setsEffect in m_SetsEffects)
        {
            if (setsEffect.TriggerType != triggerType)
                continue;

            var effect = BDatabase.GetEffect(setsEffect.EffectID);
            if (effect == null)
                continue;

            EffectExecutor.Execute(effect, new EffectContext(this, this));
        }
    }

    public async UniTask ExecuteSkill(int index, List<DungeonUnit> targets, ISkillCamera camera)
    {
        var skill = m_ActiveSkills[index].Data;
        UseMp(skill.MpCost);
        var context = new SkillMotionContext(this, targets.Cast<ISkillVisual>().ToList(), camera);
        await skill.PlayMotion(context);
        int skillLevel = State is Ally ally ? ally.GetSkillLevel(skill.ID) : 1;
        var skillGrowth = BDatabase.GetSkillGrowth(skill.ID, skillLevel);
        float scalingValue = skillGrowth != null ? skillGrowth.ScalingValue : 1f;
        foreach (var effectID in skill.EffectIDs)
        {
            var effect = BDatabase.GetEffect(effectID);
            if (effect == null)
                continue;
            foreach (var target in targets)
                EffectExecutor.Execute(effect, new EffectContext(this, target, scalingValue));
        }
    }

    public void RestoreMp(int mp)
    {
        //TODO: 회복 연출
        CurMp = Mathf.Min(CurMp + mp, State.MaxMp.ResultValue);
        if (CurMp >= 0)
        {
            RemoveAllPenalties();
        }
        else
        {
            UpdateMpPenalty();
        }
    }

    public void RestoreMpPercent(float percent)
    {

    }

    private void UseMp(int mpCost)
    {
        //TODO: Hud Update
        CurMp = Mathf.Max(-100, CurMp - mpCost);
        if (CurMp < 0)
        {
            UpdateMpPenalty();
        }
    }

    private void UpdateMpPenalty()
    {
        float deficitMp = Mathf.Abs(CurMp);

        ApplyMpPenalty(1, deficitMp >= 1);
        ApplyMpPenalty(2, deficitMp >= 25);
        ApplyMpPenalty(3, deficitMp >= 50);
        ApplyMpPenalty(4, deficitMp >= 75);
    }

    private void ApplyMpPenalty(int penaltyLevel, bool condition)
    {
        if (!condition && m_MpPenalties.Contains(penaltyLevel))
        {
            RemoveMpPenalty(penaltyLevel);
            m_MpPenalties.Remove(penaltyLevel);
        }
        else if (condition && !m_MpPenalties.Contains(penaltyLevel))
        {
            ApplyPenalty(penaltyLevel);
            m_MpPenalties.Add(penaltyLevel);
        }
    }

    private void ApplyPenalty(int level)
    {
        switch (level) //TODO: 패널티 아이콘 생성 및 연출 등
        {
            case 1:
                State.Speed.AddModifier(new StatModifier(0.5f, EStatValueType.FixedPercent, "MpPenalty"));
                break;
            case 2:
                State.Speed.AddModifier(new StatModifier(0, EStatValueType.FixedPercent, "MpPenalty"));
                break;
            case 3:
                State.Attack.AddModifier(new StatModifier(0.5f, EStatValueType.FixedPercent, "MpPenalty"));
                break;
            case 4:
                State.Attack.AddModifier(new StatModifier(0, EStatValueType.FixedPercent, "MpPenalty"));
                break;
        }
    }

    private void RemoveMpPenalty(int level)
    {
        switch (level) //TODO: 패널티 아이콘 해제
        {
            case 1:
            case 2:
                State.Speed.RemoveAllModifiersFromSource("MpPenalty");
                break;
            case 3:
            case 4:
                State.Attack.RemoveAllModifiersFromSource("MpPenalty");
                break;
        }
    }

    private void RemoveAllPenalties()
    {
        foreach (var level in m_MpPenalties)
        {
            RemoveMpPenalty(level);
        }
        m_MpPenalties.Clear();
    }

    public override float GetResultSpeed()
    {
        return State.Speed.ResultValue + UnityEngine.Random.Range(1, 9);
    }

    public override void TakeDamage(float value, bool ignoreDFS = true, ISkillTarget attacker = null)
    {
        GameEventReporter.Report(EGameEventType.TakeDamage, ID);
        base.TakeDamage(value, ignoreDFS, attacker);
    }
}
