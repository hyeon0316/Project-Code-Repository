using Cysharp.Threading.Tasks;
using PrimeTween;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AnimatorHandler))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class DungeonUnit : MonoBehaviour, ISkillTarget, ISkillVisual, IPointerClickHandler
{
    public string ID { get; protected set; }
    public Character State { get; protected set; }
    public string Name { get; protected set; }
    public float CurHp { get; protected set; }
    public float CurMp { get; protected set; }
    public bool IsDead { get { return CurHp == 0; } }
    public int TransformIndex { get; set; }
    public AnimatorHandler Animator { get; private set; }
    public bool IsFacingRight { get { return !m_SpriteRenderer.flipX; } }
    public bool IsStun { get; private set; }
    public int StunResistTier { get; private set; }
    public IReadOnlyDictionary<string, EffectInstance> Effects => m_Effects;
    public IReadOnlyList<ActiveSkill> ActiveSkills { get { return m_ActiveSkills; } }
    public IReadOnlyList<PassiveSkill> PassiveSkills { get { return m_PassiveSkills; } }

    protected Dictionary<string, EffectInstance> m_Effects = new();
    protected List<PassiveSkill> m_PassiveSkills = new();
    protected List<ActiveSkill> m_ActiveSkills = new();
    private Tween m_TurnEffectTween;
    private SpriteRenderer m_SpriteRenderer;
    private BoxCollider2D m_Collider;
    private bool m_IsColliderFitted;

    private void Awake()
    {
        Animator = this.GetComponent<AnimatorHandler>();
        m_SpriteRenderer = this.GetComponent<SpriteRenderer>();
        m_Collider = this.GetComponent<BoxCollider2D>();
    }

    /// <summary>
    /// Init의 SetController 직후에는 아직 첫 프레임이 샘플링되지 않아 다음 프레임에 한 번만 처리한다.
    /// UnitInfo를 여는 PreparePhase에서는 Idle만 재생되므로 갱신은 1회로 충분하다.
    /// </summary>
    private void LateUpdate()
    {
        if (m_IsColliderFitted || m_SpriteRenderer.sprite == null)
            return;

        var bounds = m_SpriteRenderer.sprite.bounds;
        //m_Collider.offset = bounds.center;
        m_Collider.size = new Vector2(1.5f, bounds.size.y);
        m_IsColliderFitted = true;
    }

    protected void ResetState()
    {
        m_Effects.Clear();
        m_PassiveSkills.Clear();
        m_ActiveSkills.Clear();
        StunResistTier = 0;
        RemoveStun();
        //풀에서 재사용되면 다른 타입의 스프라이트로 바뀌므로 클릭 영역을 다시 맞춘다.
        m_IsColliderFitted = false;
    }

    public void ClearBattleEffects()
    {
        //Clear 과정에서 m_Effects가 수정될 수 있으므로 복사본을 순회한다.
        var instances = new List<EffectInstance>(m_Effects.Values);
        foreach (var instance in instances)
            EffectExecutor.Clear(instance.Effect, new EffectContext(this, this));
        m_Effects.Clear();
    }

    /// <summary>
    /// 저장된 기록에서 이펙트를 복원한다. 기록된 스택 수를 그대로 되살려야 하고
    /// 저항 굴림을 다시 하면 안 되므로 EffectExecutor를 거치지 않는다.
    /// </summary>
    public void RestoreEffects(List<EffectRecord> records, bool isStun, int stunResistTier,
        Dictionary<string, DungeonUnit> unitDic)
    {
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

            var instance = new EffectInstance(entry, new EffectContext(caster, this, record.ScalingValue));
            instance.RestoreStack(record.StackCount);
            m_Effects.Add(record.EffectID, instance);

            if (!entry.EffectType.TryGetStatType(out var statType))
                continue;

            //Stack은 지속 턴 수이므로 보정은 스택 수와 무관하게 한 번만 적용한다.
            float value = entry.EffectType.IsDebuff() ? -entry.Value : entry.Value;
            State.AddStatValue(statType, value, entry);
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

        foreach (var p in ally.GetDupePassives())
        {
            if (p != null)
                m_PassiveSkills.Add(new PassiveSkill(p, "DUPE"));
        }
    }

    public void ApplyOnBattleStartPassive()
    {
        var self = new List<ISkillTarget> { this };
        var empty = new List<ISkillTarget>();
        TriggerPassive(EffectTriggerType.OnBattleStart, self, empty);
    }

    public virtual void TriggerPassive(EffectTriggerType triggerType,
        List<ISkillTarget> characters, List<ISkillTarget> targets)
    {
        foreach (var skill in m_PassiveSkills)
        {
            if (skill.Data.TriggerType != triggerType)
                continue;
            ExecutePassiveEffects(skill.Data, characters, targets);
        }
    }

    private void ExecutePassiveEffects(PassiveSkillSO skillData,
        List<ISkillTarget> characters, List<ISkillTarget> targets)
    {
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
                EffectExecutor.Execute(effect, new EffectContext(this, target));
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
    /// 이펙트를 부여한다. 저항 대상 타입(Stun/Bleed/Blight)은 저항 굴림에 실패해야 부여된다.
    /// 부여되지 않으면 false.
    /// </summary>
    private bool AddEffect(EffectContext context, EffectEntry effect)
    {
        if (IsResisted(effect.EffectType))
            return false;

        if (m_Effects.ContainsKey(effect.EffectID))
        {
            if (!m_Effects[effect.EffectID].AddStack())
                return false;
        }
        else
        {
            var instance = new EffectInstance(effect, context);
            instance.AddStack();
            m_Effects.Add(effect.EffectID, instance);
        }
        ContentsManager.Instance.Get<DungeonContents>().HudValueUpdated?.Invoke(this);
        return true;
    }

    /// <summary>RESIST_MAP에 없는 타입은 저항 대상이 아니므로 항상 false.</summary>
    private bool IsResisted(EffectType type)
    {
        if (!type.TryGetResistType(out var resistType))
            return false;

        //Random.value는 1.0을 포함하므로 저항 1.0에서 완전 면역이 되도록 별도 비교한다.
        float resistRate = State.GetStatByType(resistType).ResultValue;
        if (resistRate < 1f && UnityEngine.Random.value >= resistRate)
            return false;

        HDebug.Log($"{Name} resisted {type}. rate: {resistRate}");
        return true;
    }

    /// <summary>
    /// 턴 시작 시 지속형 효과의 스택을 1 소모한다.
    /// 트리거형 DoT(Bleed/Blight)는 자기 트리거 시점에만 줄어드므로 제외.
    /// </summary>
    public void UpdateEffectStacks()
    {
        var keysToRemove = new List<string>();
        var expired = new List<EffectInstance>();
        foreach (var kvp in m_Effects)
        {
            var instance = kvp.Value;
            if (instance.Effect.EffectType.IsTriggerDot())
                continue;

            instance.ConsumeStack();
            if (instance.IsEmpty)
            {
                //저항 버프가 자연 만료되면 누적 단계도 초기화된다.
                if (instance.Effect.EffectType == EffectType.Buff_StunResistRate)
                    StunResistTier = 0;

                expired.Add(instance);
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
            m_Effects.Remove(key);

        //Clear가 m_Effects를 수정할 수 있으므로(기절 해제 시 저항 버프 부여) 순회 밖에서 처리한다.
        foreach (var instance in expired)
            EffectExecutor.Clear(instance.Effect, instance.Context);

        ContentsManager.Instance.Get<DungeonContents>().HudValueUpdated?.Invoke(this);
    }

    /// <summary>지정 타입의 트리거형 DoT를 발동한다. 데미지 후 스택 1 감소.</summary>
    public async UniTask TriggerDot(EffectType type)
    {
        var matched = new List<EffectInstance>();
        foreach (var kvp in m_Effects)
        {
            if (kvp.Value.Effect.EffectType == type)
                matched.Add(kvp.Value);
        }

        foreach (var instance in matched)
        {
            await EffectExecutor.Dot(instance.Effect, instance.Context, instance.StackCount);
            instance.ConsumeStack();
            if (instance.IsEmpty)
                m_Effects.Remove(instance.Effect.EffectID);

            ContentsManager.Instance.Get<DungeonContents>().HudValueUpdated?.Invoke(this);
            if (IsDead)
                return;
        }
    }

    public virtual void TakeDamage(float value, bool ignoreDFS = true, ISkillTarget attacker = null)
    {
        value = ignoreDFS ? value : value * (100 / (State.Defense.ResultValue + 100));
        float prevHp = CurHp;
        CurHp = Mathf.Max(0, CurHp - Mathf.RoundToInt(value));
        var contents = ContentsManager.Instance.Get<DungeonContents>();
        //오버킬로 기여도가 부풀지 않도록 실제 감소량만 기록한다.
        if (attacker is AllyUnit ally)
            contents.DungeonProgress.Battle.RecordDamage(ally.ID, prevHp - CurHp);

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
    }

    public float GetAttack()
    {
        return State.Attack.ResultValue;
    }

    public float GetMaxHp()
    {
        return State.MaxHp.ResultValue;
    }

    public void AddStatValue(EBonusStatType statType, float value, object source, EffectContext context, EffectEntry effect)
    {
        AddEffect(context, effect);
        State.AddStatValue(statType, value, source);
    }

    public void AddStatValue(EBonusStatType statType, float value, object source)
    {
        State.AddStatValue(statType, value, source);
    }

    public void RemoveStatValue(EBonusStatType statType, object source)
    {
        State.RemoveStatValue(statType, source);
    }

    public virtual float GetResultSpeed()
    {
        return State.Speed.ResultValue + UnityEngine.Random.Range(1, 9);
    }

    public void Heal(float value)
    {
        CurHp = Mathf.Min(CurHp + Mathf.RoundToInt(value), Mathf.RoundToInt(State.MaxHp.ResultValue));
        ContentsManager.Instance.Get<DungeonContents>().HudValueUpdated.Invoke(this);
    }

    /// <summary>
    /// 기절이 풀리기 전에는 다시 부여되지 않는다. 스택 리필로 무한 기절이 되는 것을 막는다.
    /// 기절이 걸리면 저항 버프는 사라지고, 단계는 유지되어 풀릴 때 다음 단계로 올라간다.
    /// </summary>
    public void ApplyStun(EffectContext context, EffectEntry effect)
    {
        if (IsStun)
            return;

        if (!AddEffect(context, effect))
            return;

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
        var ids = GlobalVariable.STUN_SYSTEM_RESIST_BUFF_IDS;
        RemoveStunResistBuff();

        StunResistTier = Mathf.Min(StunResistTier + 1, ids.Length);
        var entry = BDatabase.GetEffect(ids[StunResistTier - 1]);
        if (entry == null)
            return;

        var instance = new EffectInstance(entry, new EffectContext(this, this));
        instance.AddStack();
        m_Effects.Add(entry.EffectID, instance);
        State.AddStatValue(EBonusStatType.StunResistRate, entry.Value, entry);
        ContentsManager.Instance.Get<DungeonContents>().HudValueUpdated?.Invoke(this);
    }

    /// <summary>기절 저항 버프 인스턴스와 스탯 보정을 제거한다. 단계(StunResistTier)는 유지한다.</summary>
    private void RemoveStunResistBuff()
    {
        string curID = null;
        EffectEntry curEntry = null;
        foreach (var kvp in m_Effects)
        {
            if (kvp.Value.Effect.EffectType != EffectType.Buff_StunResistRate)
                continue;

            curID = kvp.Key;
            curEntry = kvp.Value.Effect;
            break;
        }

        if (curEntry == null)
            return;

        State.RemoveStatValue(EBonusStatType.StunResistRate, curEntry);
        m_Effects.Remove(curID);
    }

    public void ApplyBleed(EffectContext context, EffectEntry effect)
    {
        AddEffect(context, effect);
    }

    public void ApplyBlight(EffectContext context, EffectEntry effect)
    {
        AddEffect(context, effect);
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
            m_SpriteRenderer.sortingOrder = 1;
        }
        else
        {
            if (m_TurnEffectTween.isAlive)
                m_TurnEffectTween.Stop();

            m_SpriteRenderer.color = Color.white;
            m_SpriteRenderer.sortingOrder = 0;
        }
    }

    public void SetFocus(bool isFocus)
    {
        Color color = m_SpriteRenderer.color;
        color.a = isFocus ? 1 : 0.2f;
        m_SpriteRenderer.color = color;
        m_SpriteRenderer.sortingOrder = isFocus ? 1 : 0;
    }

    public void SetFacingRight(bool facingRight)
    {
        m_SpriteRenderer.flipX = !facingRight;
    }

    public Vector3 GetOverHeadPos()
    {
        return CharacterPositionUtil.GetOverHeadPos(m_SpriteRenderer);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsDead)
            return;

        ContentsManager.Instance.Get<DungeonContents>().SelectUnitInfo(this);
    }
}
