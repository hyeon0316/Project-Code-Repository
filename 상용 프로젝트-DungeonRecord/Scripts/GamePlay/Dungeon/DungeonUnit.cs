using Cysharp.Threading.Tasks;
using PrimeTween;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AnimatorHandler))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class DungeonUnit : MonoBehaviour, ISkillTarget, ISkillVisual, IBattleUnit, IPointerClickHandler
{
    private const int TURN_SORTING_ORDER = 2;
    private const float HIT_FLASH_DURATION = 0.25f;
    private static readonly Color HIT_FLASH_COLOR = new Color(1f, 0f, 0f);
    private const string HIT_SFX_ID = "Hit";

    public abstract bool IsAlly { get; }

    public string ID { get; protected set; }
    public Character State { get; protected set; }
    public string Name { get; protected set; }
    public float CurHp { get; protected set; }
    public float CurMp { get; protected set; }
    public bool IsDead { get { return CurHp == 0; } }
    public int TransformIndex { get; set; }
    public AnimatorHandler Animator { get; private set; }
    public bool IsFacingRight { get { return !m_SpriteRenderer.flipX; } }
    public Vector3 BodyCenter { get { return m_Collider.bounds.center; } }
    public bool IsStun { get; private set; }
    public int StunResistTier { get; private set; }
    public bool HasCriticalHit { get; private set; }
    /// <summary>부여된 순서대로 담긴다. DoT는 앞에 있는 인스턴스부터 소모된다.</summary>
    public IReadOnlyList<EffectInstance> Effects => m_Effects;
    public IReadOnlyList<ActiveSkill> ActiveSkills { get { return m_ActiveSkills; } }
    public IReadOnlyList<PassiveSkill> PassiveSkills { get { return m_PassiveSkills; } }
    public PassiveSkillGroupSO PassiveGroup { get; private set; }
    public IReadOnlyList<DupePassiveSO> DupePassives { get { return m_DupePassives; } }
    /// <summary>같은 종류의 유닛끼리 공유하는 키. 패시브 출처 키에 붙여 캐릭터 간 충돌을 막는다.</summary>
    protected abstract string TypeKey { get; }

    /// <summary>주입하지 않으면 전역 난수를 쓴다.</summary>
    protected IRandomSource RandomSource
    {
        get { return m_RandomSource ?? UnityRandomSource.Default; }
    }

    protected List<EffectInstance> m_Effects = new();
    protected List<PassiveSkill> m_PassiveSkills = new();
    protected List<ActiveSkill> m_ActiveSkills = new();
    protected List<DupePassiveSO> m_DupePassives = new();
    private Tween m_TurnEffectTween;
    private Tween m_HitFlashTween;
    private SpriteRenderer m_SpriteRenderer;
    private BoxCollider2D m_Collider;
    private bool m_IsColliderFitted;
    private Vector3 m_OverHeadOffset;
    private bool m_IsTurn;
    private IRandomSource m_RandomSource;
    private static int m_LastHitSfxFrame = -1;

    /// <summary>전투 단위로 난수 공급원을 바꾼다. 시드 고정 재현과 테스트에 쓴다.</summary>
    public void SetRandomSource(IRandomSource source)
    {
        m_RandomSource = source;
    }

    private void Awake()
    {
        Animator = this.GetComponent<AnimatorHandler>();
        m_SpriteRenderer = this.GetComponent<SpriteRenderer>();
        m_Collider = this.GetComponent<BoxCollider2D>();
    }

    /// <summary>
    /// Init의 SetController 직후에는 아직 첫 프레임이 샘플링되지 않아 다음 프레임에 한 번만 처리한다.
    /// </summary>
    private void LateUpdate()
    {
        if (m_IsColliderFitted || m_SpriteRenderer.sprite == null)
            return;

        var bounds = m_SpriteRenderer.sprite.bounds;
        m_Collider.offset = new Vector2(m_Collider.offset.x, bounds.center.y);
        m_Collider.size = new Vector2(1.5f, bounds.size.y);
        m_OverHeadOffset = CharacterPositionUtil.GetOverHeadOffset(m_SpriteRenderer);
        m_IsColliderFitted = true;
    }

    protected void ResetState()
    {
        m_Effects.Clear();
        m_PassiveSkills.Clear();
        m_ActiveSkills.Clear();
        m_DupePassives.Clear();
        StunResistTier = 0;
        RemoveStun();
        //풀에서 재사용되면 다른 타입의 스프라이트로 바뀌므로 클릭 영역을 다시 맞춘다.
        m_IsColliderFitted = false;
    }

    public void ClearBattleEffects()
    {
        //Clear 과정에서 m_Effects가 수정될 수 있으므로 복사본을 순회한다.
        var instances = new List<EffectInstance>(m_Effects);
        foreach (var instance in instances)
        {
            if (instance.Effect.EffectType == EffectType.Stun)
            {
                RemoveStun();
                continue;
            }
            EffectExecutor.OnRemove(instance);
        }
        m_Effects.Clear();
        StunResistTier = 0;
    }

    /// <summary>
    /// 저장된 기록에서 이펙트를 복원한다. 기록된 스택 수를 그대로 되살려야 하고
    /// 저항 굴림을 다시 하면 안 되므로 EffectExecutor를 거치지 않는다.
    /// </summary>
    public void RestoreEffects(List<EffectRecord> records, bool isStun, int stunResistTier,
        Dictionary<string, DungeonUnit> unitDic)
    {
        foreach (var instance in m_Effects)
        {
            if (instance.Effect.EffectType.TryGetStatType(out var appliedStatType))
                State.RemoveStatValue(appliedStatType, instance);
        }
        m_Effects.Clear();
        foreach (var record in records)
        {
            var entry = BDatabase.GetEffect(record.EffectID);
            if (entry == null)
                continue;

            DungeonUnit caster = null;
            if (!string.IsNullOrEmpty(record.CasterID))
                unitDic.TryGetValue(record.CasterID, out caster);
            if (caster == null)
                caster = this;

            var context = new EffectContext(caster, this, record.ScalingValue, record.Source);
            string key = GetInstanceKey(context, entry);
            if (FindEffect(key) != null)
                continue;

            var instance = new EffectInstance(entry, context, key);
            instance.Restore(record.StackCount, record.RemainingTurns);
            m_Effects.Add(instance);

            if (!entry.EffectType.TryGetStatType(out var statType))
                continue;

            State.AddStatValue(statType, entry.GetSignedValue(), instance);
        }
        IsStun = isStun;
        StunResistTier = stunResistTier;
    }

    protected void InitActiveSkill(List<ActiveSkillSO> actives)
    {
        foreach (var a in actives)
        {
            m_ActiveSkills.Add(new ActiveSkill(a));
        }
    }

    protected void InitPassiveSkill(PassiveSkillGroupSO group)
    {
        PassiveGroup = group;
        if (group == null || group.Skills == null) //passive가 없는 캐릭터
            return;

        foreach (var p in group.Skills)
        {
            if (p != null)
                m_PassiveSkills.Add(new PassiveSkill(p, group.ID));
        }
    }

    protected void InitDupePassive(Ally ally)
    {
        if (ally == null)
            return;

        m_DupePassives.Clear();
        foreach (var dupe in ally.GetDupePassives())
        {
            if (dupe == null || dupe.Passive == null)
                continue;

            m_DupePassives.Add(dupe);
            m_PassiveSkills.Add(new PassiveSkill(dupe.Passive, $"DUPE_{ID}"));
        }
    }

    /// <summary>적 전용. 아군은 Ally 생성 시 캐릭터 스탯에 이미 적용되어 CloneStats로 넘어온다.</summary>
    protected void ApplyAlwaysPassive()
    {
        foreach (var skill in m_PassiveSkills)
        {
            if (skill.Data.TriggerType != EffectTriggerType.None)
                continue;

            EquipEffectApplier.Apply(State, skill.Data.EffectIDs, skill);
        }
    }

    public void ResetPassiveTriggerCount()
    {
        foreach (var skill in m_PassiveSkills)
            skill.ResetTriggerCount();
    }

    public virtual void TriggerPassive(EffectTriggerType triggerType,
        List<ISkillTarget> characters, List<ISkillTarget> targets)
    {
        foreach (var skill in m_PassiveSkills)
        {
            if (skill.Data.TriggerType != triggerType)
                continue;
            if (!skill.TryTrigger(RandomSource))
                continue;
            ExecutePassiveEffects(skill, characters, targets);
        }
    }

    private void ExecutePassiveEffects(PassiveSkill skill,
        List<ISkillTarget> characters, List<ISkillTarget> targets)
    {
        var skillData = skill.Data;
        string source = $"{TypeKey}/{skill.GroupID}/{skillData.name}";
        var tempDic = new Dictionary<int, List<ISkillTarget>>();
        skillData.CalculateTargets(TransformIndex, characters, targets, tempDic);
        if (!tempDic.TryGetValue(TransformIndex, out var resolvedTargets))
            return;

        foreach (var effectID in skillData.EffectIDs)
        {
            var effect = BDatabase.GetEffect(effectID);
            if (effect == null)
                continue;
            foreach (var target in resolvedTargets)
                EffectExecutor.Execute(effect, new EffectContext(this, target, source: source));
        }
    }

    public string GetActiveSkillName(int index)
    {
        return m_ActiveSkills[index].Data.Name;
    }

    public void SetUsingSkills()
    {
        ContentsManager.Instance.Get<DungeonContents>().SetSkill(m_ActiveSkills);
    }

    /// <summary>
    /// 지속·중첩 효과를 부여한다. 저항 대상 타입(Stun/Bleed/Blight)은 저항 굴림에 실패해야 부여된다.
    /// 같은 키(GetInstanceKey)가 이미 있으면 지속 턴 갱신·중첩만 하고, 처음 부여될 때만 핸들러의 OnApply가 불린다.
    /// 기절 중에는 기절이 다시 부여되지 않는다.
    /// </summary>
    public void ApplyEffect(EffectContext context, EffectEntry effect)
    {
        if (effect.EffectType == EffectType.Stun && IsStun)
            return;

        if (IsResisted(effect.EffectType))
            return;

        string key = GetInstanceKey(context, effect);
        var instance = FindEffect(key);
        if (instance != null)
        {
            if (instance.Apply(effect) && EffectExecutor.IsSharedStack(effect.EffectType))
                instance.SetSource(effect, context);
        }
        else
        {
            instance = new EffectInstance(effect, context, key);
            instance.Apply();
            m_Effects.Add(instance);
            EffectExecutor.OnApply(instance);
        }
        ContentsManager.Instance.Get<DungeonContents>().HudValueUpdated?.Invoke(this);
    }

    /// <summary>
    /// 기절·기절 저항과 공유 중첩 DoT는 종류당 하나, 그 외 중첩형(DoT)은 시전자별 하나,
    /// 그 외는 출처(스킬·패시브·세트)별 하나다. 다른 출처의 같은 버프는 따로 적용된다.
    /// </summary>
    private static string GetInstanceKey(EffectContext context, EffectEntry effect)
    {
        var type = effect.EffectType;
        if (type == EffectType.Stun || type == EffectType.Buff_StunResistRate
            || EffectExecutor.IsSharedStack(type))
            return type.ToString();

        if (effect.Stack > 0)
        {
            var caster = context.Caster as DungeonUnit;
            return $"{type}|{(caster != null ? caster.ID : string.Empty)}";
        }

        return $"{context.Source}|{effect.EffectID}";
    }

    private EffectInstance FindEffect(string key)
    {
        foreach (var instance in m_Effects)
        {
            if (instance.Key == key)
                return instance;
        }
        return null;
    }

    private EffectInstance FindFirstEffect(EffectType type)
    {
        foreach (var instance in m_Effects)
        {
            if (instance.Effect.EffectType == type)
                return instance;
        }
        return null;
    }

    private void RemoveEffect(EffectInstance instance)
    {
        if (!m_Effects.Remove(instance))
            return;

        EffectExecutor.OnRemove(instance);
    }

    /// <summary>RESIST_MAP에 없는 타입은 저항 대상이 아니므로 항상 false.</summary>
    private bool IsResisted(EffectType type)
    {
        if (!type.TryGetResistType(out var resistType))
            return false;

        float resistRate = State.GetStatByType(resistType).ResultValue;
        if (!ResistRule.IsResisted(type, resistRate, RandomSource))
            return false;

        HDebug.Log($"{Name} resisted {type}. rate: {resistRate}");
        ContentsManager.Instance.Get<DungeonContents>().HudResisted?.Invoke(this, type);
        return true;
    }

    /// <summary>턴 종료 시 Duration이 있는 효과의 남은 턴을 1 소모한다.</summary>
    public void UpdateEffectStacks()
    {
        var expired = new List<EffectInstance>();
        foreach (var instance in m_Effects)
        {
            if (instance.Effect.Duration == 0)
                continue;

            instance.ConsumeTurn();
            if (!instance.IsExpired)
                continue;

            //저항 버프가 자연 만료되면 누적 단계도 초기화된다.
            if (instance.Effect.EffectType == EffectType.Buff_StunResistRate)
                StunResistTier = 0;

            expired.Add(instance);
        }

        //OnRemove가 m_Effects를 수정할 수 있으므로(기절 해제 시 저항 버프 부여) 순회 밖에서 처리한다.
        foreach (var instance in expired)
            RemoveEffect(instance);

        ContentsManager.Instance.Get<DungeonContents>().HudValueUpdated?.Invoke(this);
    }

    /// <summary>
    /// 가장 먼저 부여된 인스턴스의 시전자 기준으로 피해를 주고 그 인스턴스의 스택을 1 소모한다.
    /// 피해 계산의 스택 수는 같은 종류 전체의 합이다.
    /// </summary>
    public async UniTask TriggerDot(EffectType type)
    {
        var instance = FindFirstEffect(type);
        if (instance == null)
            return;

        float damage = EffectExecutor.CalcDotDamage(instance.Effect, instance.Context, GetTotalStack(type));
        instance.Context.Target.TakeDamage(damage, attacker: instance.Context.Caster);
        instance.Context.Target.PlayHit();
        await UniTask.WaitForSeconds(1);

        instance.ConsumeStack();
        if (instance.IsExpired)
            RemoveEffect(instance);

        ContentsManager.Instance.Get<DungeonContents>().HudValueUpdated?.Invoke(this);
    }

    public int GetTotalStack(EffectType type)
    {
        int total = 0;
        foreach (var instance in m_Effects)
        {
            if (instance.Effect.EffectType == type)
                total += instance.StackCount;
        }
        return total;
    }

    public virtual void TakeDamage(float value, bool ignoreDFS = true, ISkillTarget attacker = null, bool isCritical = false)
    {
        int damage = DamageRule.CalcDamage(value, State.Defense.ResultValue, ignoreDFS);
        float prevHp = CurHp;
        CurHp = DamageRule.ApplyDamage(CurHp, damage);
        var contents = ContentsManager.Instance.Get<DungeonContents>();
        //오버킬로 기여도가 부풀지 않도록 실제 감소량만 기록한다.
        if (attacker is AllyUnit ally)
            contents.DungeonProgress.Battle.RecordDamage(ally.ID, prevHp - CurHp);

        contents.HudDamaged?.Invoke(this, damage, isCritical);
        contents.HudValueUpdated.Invoke(this);
        if (IsDead)
        {
            Dead();
        }
    }

    public void PlayHit()
    {
        if (IsDead)
            return;

        Animator.PlayHit(GlobalVariable.ANIMATION_CLIP_HURT);
        PlayHitFlash();

        if (m_LastHitSfxFrame != Time.frameCount)
        {
            m_LastHitSfxFrame = Time.frameCount;
            SoundManager.Instance.PlaySfx(HIT_SFX_ID);
        }
    }

    private void PlayHitFlash()
    {
        if (m_HitFlashTween.isAlive)
            m_HitFlashTween.Complete();

        Color origin = m_SpriteRenderer.color;
        Color flash = HIT_FLASH_COLOR;
        flash.a = origin.a;
        m_HitFlashTween = Tween.Color(m_SpriteRenderer, flash, origin, HIT_FLASH_DURATION);
    }

    public float GetAttack()
    {
        return State.Attack.ResultValue;
    }

    public bool RollCritical()
    {
        float rate = State.CriticalRate.ResultValue;
        bool isCritical = rate >= 1f || RandomSource.Value() < rate;
        if (isCritical)
            HasCriticalHit = true;

        return isCritical;
    }

    public void ClearCriticalHit()
    {
        HasCriticalHit = false;
    }

    public float GetMaxHp()
    {
        return State.MaxHp.ResultValue;
    }

    public void AddStatValue(EBonusStatType statType, float value, object source)
    {
        State.AddStatValue(statType, value, source);
    }

    public void RemoveStatValue(EBonusStatType statType, object source)
    {
        State.RemoveStatValue(statType, source);
    }

    public float GetResultSpeed()
    {
        return State.Speed.ResultValue + RandomSource.Range(1, 9);
    }

    public void Heal(float value)
    {
        float prevHp = CurHp;
        CurHp = DamageRule.ApplyHeal(CurHp, value, State.MaxHp.ResultValue);
        var contents = ContentsManager.Instance.Get<DungeonContents>();
        int healed = Mathf.RoundToInt(CurHp - prevHp);
        if (healed > 0)
            contents.HudHealed?.Invoke(this, healed);
        contents.HudValueUpdated.Invoke(this);
    }

    public virtual void RestoreMpPercent(float percent)
    {
    }

    public void LoseHpPercent(float percent)
    {
        float loss = Mathf.Min(Mathf.Round(State.MaxHp.ResultValue * percent), CurHp - 1);
        if (loss <= 0)
            return;

        CurHp -= loss;
        var contents = ContentsManager.Instance.Get<DungeonContents>();
        contents.HudDamaged?.Invoke(this, Mathf.RoundToInt(loss), false);
        contents.HudValueUpdated.Invoke(this);
    }

    /// <summary>기절이 걸리면 저항 버프는 사라지고, 단계는 유지되어 풀릴 때 다음 단계로 올라간다.</summary>
    public void ApplyStun()
    {
        IsStun = true;
        RemoveStunResistBuff();
        //TODO: UI 적용
    }

    public void RemoveStun()
    {
        IsStun = false;
    }

    /// <summary>
    /// 기절이 풀릴 때 다음 단계의 기절 저항 버프를 부여한다.
    /// 단계는 StunResistTier가 기억하므로 기절 중에 버프가 없어도 승계된다.
    /// </summary>
    public void ApplyStunResistBuff()
    {
        RemoveStunResistBuff();

        StunResistTier = StunResistRule.GetNextTier(StunResistTier);
        var entry = BDatabase.GetEffect(StunResistRule.GetBuffID(StunResistTier));
        if (entry == null)
            return;

        ApplyEffect(new EffectContext(this, this), entry);
    }

    /// <summary>기절 저항 버프 인스턴스와 스탯 보정을 제거한다. 단계(StunResistTier)는 유지한다.</summary>
    private void RemoveStunResistBuff()
    {
        var instance = FindFirstEffect(EffectType.Buff_StunResistRate);
        if (instance != null)
            RemoveEffect(instance);
    }

    public async UniTask UseItem(ConsumableItem item, DungeonUnit target)
    {
        //TODO: 아이템 사용 연출
        item.Use(target);
    }

    public virtual void Dead()
    {
        ContentsManager.Instance.Get<DungeonContents>().HudUnInitialized.Invoke(this);
        Animator.PlayAnim(GlobalVariable.ANIMATION_CLIP_DEAD);
        Animator.WaitForCurAnimationEnd().ContinueWith(() =>
        {
            ContentsManager.Instance.Get<DungeonContents>().HudHidden?.Invoke(this);
            this.gameObject.SetActive(false);
        }).Forget();
    }

    public void SetHighLight(bool isActive)
    {
        if (isActive)
        {
            if (m_TurnEffectTween.isAlive)
                return;

            m_TurnEffectTween = Tween.Color(m_SpriteRenderer, Color.yellow, 0.5f, cycles: -1,
                cycleMode: CycleMode.Yoyo);
            SetSortingOrder(1);
        }
        else
        {
            if (m_TurnEffectTween.isAlive)
                m_TurnEffectTween.Stop();

            m_SpriteRenderer.color = Color.white;
            SetSortingOrder(0);
        }
    }

    public void SetFocus(bool isFocus)
    {
        Color color = m_SpriteRenderer.color;
        color.a = isFocus ? 1 : 0.2f;
        m_SpriteRenderer.color = color;
    }

    public void SetTurn(bool isTurn)
    {
        m_IsTurn = isTurn;
        SetSortingOrder(isTurn ? TURN_SORTING_ORDER : 0);
    }

    public void SetSortingOrder(int order)
    {
        m_SpriteRenderer.sortingOrder = m_IsTurn ? TURN_SORTING_ORDER : order;
    }

    public void SetFacingRight(bool facingRight)
    {
        m_SpriteRenderer.flipX = !facingRight;
    }

    public Vector3 GetOverHeadPos()
    {
        return CharacterPositionUtil.ToOverHeadPos(m_SpriteRenderer, m_OverHeadOffset);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsDead)
            return;

        ContentsManager.Instance.Get<DungeonContents>().SelectUnitInfo(this);
    }
}
