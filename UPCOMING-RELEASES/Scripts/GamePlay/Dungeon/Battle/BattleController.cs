using Cysharp.Threading.Tasks;
using PrimeTween;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BattleController : MonoBehaviour, GlobalEvent<DungeonContentsParam>.IEventHandler
{
    [SerializeField] private List<Transform> m_AllyTrs;
    [SerializeField] private List<Transform> m_EnemyTrs;
    [SerializeField] private Transform m_ReserveEnemyContainer;
    [SerializeField] private Transform m_MapObjContainer;
    [SerializeField] private DungeonCamera m_DungeonCamera;

    private List<DungeonUnit> m_Allies = new(GlobalVariable.PARTY_MEMER_MAX_COUNT);
    private List<DungeonUnit> m_DeadAllies = new(GlobalVariable.PARTY_MEMER_MAX_COUNT);
    private List<DungeonUnit> m_Enemies;
    private List<DungeonUnit> m_DeadEnemies = new(GlobalVariable.PARTY_MEMER_MAX_COUNT);
    private List<EnemyUnit> m_ReserveEnemies;
    private List<DungeonUnit> m_PriorityOrder;
    private DungeonUnit m_CurCharacter;
    private bool m_IsChanging;
    private DungeonContents m_Contents;

    private void Awake()
    {
        m_Contents = ContentsManager.Instance.Get<DungeonContents>();
        m_Contents.RegisterHandler(this);
    }

    private void Start()
    {
        _ = StartAsync();
    }

    private async UniTask StartAsync()
    {
        m_Contents.SetEnemyScreenPos(m_EnemyTrs);
        if (m_Contents.IsEntered) LoadMainParty(); else SetMainParty();

        bool isResumeBattle = m_Contents.IsEntered
            && m_Contents.DungeonProgress.StageType == EDungeonState.Battle;

        //전투 세팅 전에 종료된 경우 맵으로 되돌린다.
        if (isResumeBattle && m_Contents.DungeonProgress.Battle.EnemyRecords.Count == 0)
        {
            isResumeBattle = false;
            m_Contents.DungeonProgress.StageType = EDungeonState.Map;
            m_Contents.SaveProgress();
        }

        if (isResumeBattle)
            SetBattle();

        await UIManager.Instance.PageSystem.GoAsync(Pages_DungeonMainUI.AssetID);

    }

    private void OnDestroy()
    {
        m_Contents.UnRegisterHandler(this);
    }

    private void SetDungeonTerrain()
    {
        for (int i = m_MapObjContainer.childCount - 1; i >= 0; i--)
            Destroy(m_MapObjContainer.GetChild(i).gameObject);

        var prefabs = m_Contents.CurDungeonData.TerrainPrefabs;
        int index = m_Contents.DungeonProgress.TerrainIndex;
        Instantiate(prefabs[index], m_MapObjContainer);
    }

    private void SetBattle()
    {
        SetDungeonTerrain();

        if (m_Contents.DungeonProgress.Battle.EnemyRecords.Count == 0)
        {
            //새 전투이므로 이전 전투의 기여도 기록을 버린다.
            m_Contents.DungeonProgress.Battle.DamageRecords.Clear();
            CaptureStageEntrySnapshot();
            SetEnemy();
        }
        else
        {
            LoadEnemy();
            LoadActionOrder();
            RestoreEffects();
        }
    }

    private async UniTask StartBattle()
    {
        m_Contents.HudInitialized.Invoke(m_Allies, m_Enemies);
        await UniTask.WaitForSeconds(2);
        _ = NextAction();
    }

    /// <summary>
    /// 스테이지 진입 시점 아군 상태를 1회 기록한다. 재시작 롤백의 기준점.
    /// 생존/사망을 나눠 담아, 이전 스테이지에서 죽은 아군이 재시작으로 부활하지 않게 한다.
    /// </summary>
    private void CaptureStageEntrySnapshot()
    {
        var battle = m_Contents.DungeonProgress.Battle;
        battle.StageEntryAllyRecords.Clear();
        battle.StageEntryDeadAllyRecords.Clear();

        foreach (var ally in m_Allies)
        {
            battle.StageEntryAllyRecords.Add(new DungeonCharacterRecord(ally));
        }
        foreach (var ally in m_DeadAllies)
        {
            battle.StageEntryDeadAllyRecords.Add(new DungeonCharacterRecord(ally));
        }
    }

    /// <summary>
    /// 아군을 스테이지 진입 시점으로 되돌리고 전투를 새로 구성한다.
    /// 기존 유닛 오브젝트는 파기하고 스냅샷에서 다시 만든다.
    /// 재구성 중에는 유닛이 파기·재생성되는 과정이 보이므로 로딩 화면으로 가린다.
    /// </summary>
    private async UniTask RestartStage()
    {
        using (new LoadingScope(ELoadingType.Background))
        {
            //State는 영구 Ally 객체라 오브젝트를 파기해도 스탯 보정이 남으므로 먼저 걷어낸다.
            foreach (var ally in m_Allies)
                ally.ClearBattleEffects();
            foreach (var ally in m_DeadAllies)
                ally.ClearBattleEffects();

            foreach (var ally in m_Allies)
                Destroy(ally.gameObject);
            foreach (var ally in m_DeadAllies)
                Destroy(ally.gameObject);
            m_Allies.Clear();
            m_DeadAllies.Clear();

            var battle = m_Contents.DungeonProgress.Battle;
            battle.AllyRecords.Clear();
            battle.DeadAllyRecords.Clear();
            foreach (var record in battle.StageEntryAllyRecords)
            {
                battle.AllyRecords.Add(record);
            }
            //이전 스테이지에서 죽은 아군은 재시작해도 죽은 채로 유지한다.
            foreach (var record in battle.StageEntryDeadAllyRecords)
            {
                battle.DeadAllyRecords.Add(record);
            }

            LoadMainParty();
            SetBattle();
            m_Contents.SaveProgress();
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
        }

        m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.SetBattle));
    }

    private void SetMainParty()
    {
        var records = m_Contents.DungeonProgress.Battle.AllyRecords;
        var mainParty = ContentsManager.Instance.Get<PartySettingContents>().PartyInfo.GetMainPartyMembers();
        for (int i = 0; i < mainParty.Count; i++)
        {
            var member = AddressableBundleManager.Instance.AssetInstantiate<AllyUnit>(AllyUnit.AssetID, m_AllyTrs[i]);
            member.Init(mainParty[i].ToString());
            member.TransformIndex = i;
            m_Allies.Add(member);
            records.Add(new DungeonCharacterRecord(member));
        }
        m_Contents.SaveProgress();
        m_Contents.SetAllies(m_Allies);
    }

    private void LoadMainParty()
    {
        var records = m_Contents.DungeonProgress.Battle.AllyRecords;
        for (int i = 0; i < records.Count; i++)
        {
            DungeonCharacterRecord record = records[i];
            var member = AddressableBundleManager.Instance.AssetInstantiate<AllyUnit>(AllyUnit.AssetID, m_AllyTrs[record.TransformIndex]);

            member.Init(record.ID, record);
            m_Allies.Add(member);
        }

        var deadRecords = m_Contents.DungeonProgress.Battle.DeadAllyRecords;
        for (int i = 0; i < deadRecords.Count; i++)
        {
            DungeonCharacterRecord record = deadRecords[i];
            var member = AddressableBundleManager.Instance.AssetInstantiate<AllyUnit>(AllyUnit.AssetID, m_AllyTrs[0]);

            member.Init(record.ID, record);
            m_DeadAllies.Add(member);
            member.gameObject.SetActive(false);
        }
        m_Contents.SetAllies(m_Allies);
    }

    private void SetEnemy()
    {
        var enemyTypes = m_Contents.CurEnemies;
        var records = m_Contents.DungeonProgress.Battle.EnemyRecords;
        if (enemyTypes.Count <= GlobalVariable.PARTY_MEMER_MAX_COUNT)
        {
            m_Enemies = new List<DungeonUnit>(enemyTypes.Count);
        }
        else
        {
            m_Enemies = new List<DungeonUnit>(GlobalVariable.PARTY_MEMER_MAX_COUNT);
            m_ReserveEnemies = new List<EnemyUnit>(enemyTypes.Count - GlobalVariable.PARTY_MEMER_MAX_COUNT);
        }

        for (int i = 0; i < enemyTypes.Count; i++)
        {
            string typeStr = enemyTypes[i].ToString();
            EnemyUnit enemy = GetOrCreateEnemy();
            if (i < GlobalVariable.PARTY_MEMER_MAX_COUNT)
            {
                enemy.transform.SetParent(m_EnemyTrs[i]);
                enemy.transform.localPosition = Vector3.zero;
                enemy.gameObject.SetActive(true);
                enemy.TransformIndex = i;
                m_Enemies.Add(enemy);
            }
            else
            {
                enemy.transform.SetParent(m_ReserveEnemyContainer);
                enemy.transform.localPosition = Vector3.zero;
                enemy.gameObject.SetActive(false);
                enemy.TransformIndex = -1;
                m_ReserveEnemies.Add(enemy);
            }
            enemy.Init(typeStr + i);
            records.Add(new DungeonCharacterRecord(enemy));
            m_Contents.SaveProgress();
        }
        m_PriorityOrder = new List<DungeonUnit>(m_Allies.Count + m_Enemies.Count);
    }

    private void LoadEnemy()
    {
        var records = m_Contents.DungeonProgress.Battle.EnemyRecords;
        m_Enemies = new List<DungeonUnit>(GlobalVariable.PARTY_MEMER_MAX_COUNT);

        for (int i = 0; i < records.Count; i++)
        {
            DungeonCharacterRecord record = records[i];
            EnemyUnit enemy;
            if (record.TransformIndex == -1)
            {
                enemy = AddressableBundleManager.Instance.AssetInstantiate<EnemyUnit>(EnemyUnit.AssetID, m_ReserveEnemyContainer);
                enemy.gameObject.SetActive(false);
                m_ReserveEnemies ??= new List<EnemyUnit>(GlobalVariable.PARTY_MEMER_MAX_COUNT);
                m_ReserveEnemies.Add(enemy);
            }
            else
            {
                enemy = AddressableBundleManager.Instance.AssetInstantiate<EnemyUnit>(EnemyUnit.AssetID, m_EnemyTrs[record.TransformIndex]);
                m_Enemies.Add(enemy);
            }
            enemy.Init(record.ID, record);
        }

        var deadRecords = m_Contents.DungeonProgress.Battle.DeadEnemyRecords;
        for (int i = 0; i < deadRecords.Count; i++)
        {
            DungeonCharacterRecord record = deadRecords[i];
            var enemy = AddressableBundleManager.Instance.AssetInstantiate<EnemyUnit>(EnemyUnit.AssetID, m_ReserveEnemyContainer);
            enemy.Init(record.ID, record);
            m_DeadEnemies.Add(enemy);
            enemy.gameObject.SetActive(false);
        }
        m_PriorityOrder = new List<DungeonUnit>(m_Allies.Count + m_Enemies.Count);
    }

    private void LoadActionOrder()
    {
        var orderRecords = m_Contents.DungeonProgress.Battle.PriorityOrderRecords;
        var allCharacters = m_Allies.Concat(m_Enemies)
            .Concat(m_DeadAllies).Concat(m_DeadEnemies)
            .ToDictionary(c => c.ID);

        foreach (var id in orderRecords)
        {
            if (!allCharacters.TryGetValue(id, out var character))
            {
                HDebug.LogError($"Faild to load action order. ID: {id}");
                continue;
            }
            m_PriorityOrder.Add(character);
        }
    }

    /// <summary>
    /// 모든 유닛 생성이 끝난 뒤 이펙트를 복원한다. Caster 해석에 전체 유닛이 필요하므로
    /// Init 루프 안이 아닌 별도 패스로 처리한다.
    /// </summary>
    private void RestoreEffects()
    {
        var unitDic = new Dictionary<string, DungeonUnit>();
        AddToUnitDic(unitDic, m_Allies);
        AddToUnitDic(unitDic, m_DeadAllies);
        AddToUnitDic(unitDic, m_Enemies);
        AddToUnitDic(unitDic, m_DeadEnemies);
        if (m_ReserveEnemies != null)
        {
            foreach (var unit in m_ReserveEnemies)
                unitDic[unit.ID] = unit;
        }

        var battle = m_Contents.DungeonProgress.Battle;
        RestoreEffectsTo(battle.AllyRecords, unitDic);
        RestoreEffectsTo(battle.DeadAllyRecords, unitDic);
        RestoreEffectsTo(battle.EnemyRecords, unitDic);
        RestoreEffectsTo(battle.DeadEnemyRecords, unitDic);
    }

    private void AddToUnitDic(Dictionary<string, DungeonUnit> unitDic, List<DungeonUnit> units)
    {
        foreach (var unit in units)
            unitDic[unit.ID] = unit;
    }

    private void RestoreEffectsTo(List<DungeonCharacterRecord> records, Dictionary<string, DungeonUnit> unitDic)
    {
        foreach (var record in records)
        {
            if (!unitDic.TryGetValue(record.ID, out var unit))
                continue;

            unit.RestoreEffects(record.Effects, record.IsStun, record.StunResistTier, unitDic);
        }
    }

    private void SetActionOrder()
    {
        m_PriorityOrder.Clear();
        m_PriorityOrder.AddRange(m_Allies);
        m_PriorityOrder.AddRange(m_Enemies);

        var originalIndex = new Dictionary<DungeonUnit, int>(m_PriorityOrder.Count);
        for (int i = 0; i < m_PriorityOrder.Count; i++)
        {
            originalIndex[m_PriorityOrder[i]] = i;
        }

        m_PriorityOrder.Sort((a, b) =>
        {
            //speed가 제일 높은 값 우선
            int speedComparison = b.GetResultSpeed().CompareTo(a.GetResultSpeed());
            if (speedComparison != 0)
                return speedComparison;

            //아군과 적이 같은 speed일때 아군 우선
            int typeComparison = (b is AllyUnit).CompareTo(a is AllyUnit);
            if (typeComparison != 0)
                return typeComparison;

            //인덱스가 제일 낮은 값 우선
            return originalIndex[a].CompareTo(originalIndex[b]);
        });
    }

    private async UniTask NextRound()
    {
        bool hasRemainItem = m_Contents.DungeonProgress.Battle.PriorityOrderRecords.Count > 0;
        SetActionOrder();
        m_Contents.DungeonProgress.Battle.SetPriorityOrder(m_PriorityOrder);
        m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.NextRound));
        float orderUIDelay = BattleUI.GetNewRoundOrderDuration(hasRemainItem, m_PriorityOrder.Count);
        await UniTask.WaitForSeconds(orderUIDelay);
        await UniTask.WaitForSeconds(1f);
        await NextAction();
    }

    private async UniTask NextAction()
    {
        if (!DequeueOrder())
        {
            m_Contents.DungeonProgress.Battle.SetPriorityOrder(new List<DungeonUnit>());
            m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.NextAction));
            await UniTask.WaitForSeconds(1f);
            await NextRound();
            return;
        }

        if (m_CurCharacter.IsDead)
        {
            await SkipOrder();
            return;
        }

        //턴 시작 상태를 기록
        var curOrder = new List<DungeonUnit>(m_PriorityOrder.Count + 1) { m_CurCharacter };
        curOrder.AddRange(m_PriorityOrder);
        m_Contents.DungeonProgress.Battle.SetPriorityOrder(curOrder);
        m_Contents.DungeonProgress.Battle.UpdateState(m_Allies, m_DeadAllies, m_Enemies, m_DeadEnemies, m_ReserveEnemies);
        m_Contents.SaveProgress();

        m_CurCharacter.UpdateEffectStacks();
        await m_CurCharacter.TriggerDot(EffectType.Blight);
        if (m_CurCharacter.IsDead)
        {
            await HandleDotDeath();
            return;
        }

        if (m_CurCharacter.IsStun)
        {
            await SkipOrder();
            return;
        }

        m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.NextAction));
        m_Contents.HudAllShowed.Invoke();
        ShowAllCharacter();

        m_Contents.ItemTarget = null;
        m_CurCharacter.SetUsingSkills();
        if (m_CurCharacter is AllyUnit)
        {
            m_Contents.CurCharacterTransformIndex = m_CurCharacter.TransformIndex;
            var allies = m_Allies.Cast<ISkillTarget>().ToList();
            var enemies = m_Enemies.Cast<ISkillTarget>().ToList();
            m_CurCharacter.TriggerPassive(EffectTriggerType.OnTurnStart, allies, enemies);
            m_CurCharacter.SetHighLight(true);
            CalculateSkillTargets();
            CalculateItemTargets();
            UpdateMoveState(m_CurCharacter.TransformIndex);
            m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.SetSkillUI));
            m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.UpdateBattleItem));
            m_Contents.CanSelectUnitInfo = true;
            m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.SetPreparePhase));
        }
        else
        {
            ExecuteEnemyPattern().Forget();
        }
    }

    private void CalculateSkillTargets()
    {
        var allies = m_Allies.Cast<ISkillTarget>().ToList();
        var enemies = m_Enemies.Cast<ISkillTarget>().ToList();
        foreach (var state in m_Contents.CurSkillTargetStates)
        {
            state.Data.CalculateTargets(m_CurCharacter.TransformIndex, allies, enemies, state);
        }
    }

    private void CalculateItemTargets()
    {
        m_Contents.ItemTarget = m_Allies.Find(a => a.TransformIndex == m_CurCharacter.TransformIndex - 1);
    }

    private bool DequeueOrder()
    {
        if (m_PriorityOrder.Count == 0)
            return false;

        m_CurCharacter = m_PriorityOrder[0];
        m_PriorityOrder.RemoveAt(0);
        return true;
    }

    private void Win()
    {
        m_Contents.CanSelectUnitInfo = false;
        //마지막 일격 이후에는 턴이 돌지 않으므로 승리 UI가 읽을 아군 상태를 여기서 갱신한다.
        m_Contents.DungeonProgress.Battle.UpdateState(m_Allies, m_DeadAllies, m_Enemies, m_DeadEnemies, m_ReserveEnemies);
        m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.Win));
        End();
    }

    private void Lose()
    {
        m_Contents.CanSelectUnitInfo = false;
        m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.Lose));
    }

    private EnemyUnit GetOrCreateEnemy()
    {
        if (m_DeadEnemies.Count > 0)
        {
            var pooled = m_DeadEnemies[m_DeadEnemies.Count - 1] as EnemyUnit;
            m_DeadEnemies.RemoveAt(m_DeadEnemies.Count - 1);
            return pooled;
        }
        return AddressableBundleManager.Instance.AssetInstantiate<EnemyUnit>(EnemyUnit.AssetID, m_EnemyTrs[0]);
    }

    private void End()
    {
        foreach (var ally in m_Allies)
            ally.ClearBattleEffects();
        foreach (var ally in m_DeadAllies)
            ally.ClearBattleEffects();
        m_Enemies = null;
        m_PriorityOrder = null;
        if (m_ReserveEnemies != null)
        {
            foreach (var e in m_ReserveEnemies)
            {
                e.gameObject.SetActive(false);
                m_DeadEnemies.Add(e);
            }
            m_ReserveEnemies = null;
        }
        m_CurCharacter = null;
        m_Contents.DungeonProgress.Battle.End();
    }

    private async UniTask ExecuteSelectedSkill()
    {
        var ally = m_CurCharacter as AllyUnit;
        var skillIndex = m_Contents.SelectedIndex;
        var targets = m_Contents.GetSkillTargets(skillIndex);
        m_CurCharacter.SetHighLight(false);
        FocusTargetsWithCaster(targets);
        m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.SetActionPhase));
        await m_DungeonCamera.MoveToActionPhase();
        m_Contents.SetTostActionMsg(ally.GetActiveSkillName(skillIndex), m_CurCharacter.Name);
        await UniTask.WaitForSeconds(1);
        await m_CurCharacter.TriggerDot(EffectType.Bleed);
        if (m_CurCharacter.IsDead)
        {
            await HandleDotDeath();
            return;
        }

        var allies = m_Allies.Cast<ISkillTarget>().ToList();
        var enemies = m_Enemies.Cast<ISkillTarget>().ToList();
        m_CurCharacter.TriggerPassive(EffectTriggerType.OnAttack, allies, enemies);
        await ally.ExecuteSkill(skillIndex, targets, m_DungeonCamera);
        await CheckEnemy();
    }

    private async UniTask UseSelectedItem()
    {
        var target = m_Contents.ItemTarget;
        if (target == null)
            return;

        var item = m_Contents.SelectedItem;
        m_CurCharacter.SetHighLight(false);
        FocusTargetsWithCaster(new List<DungeonUnit> { target });
        m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.SetActionPhase));
        await m_DungeonCamera.MoveToActionPhase();
        m_Contents.SetTostActionMsg(item.Info.Base.Name, m_CurCharacter.Name);
        await UniTask.WaitForSeconds(1);
        await m_CurCharacter.UseItem(item, target);
        m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.RefreshUsable));
        await UniTask.WaitForSeconds(1f);
        await NextAction();
    }

    private void FocusUnits(List<DungeonUnit> targets)
    {
        foreach (var ally in m_Allies)
            ally.SetFocus(targets.Contains(ally));
        foreach (var enemy in m_Enemies)
            enemy.SetFocus(targets.Contains(enemy));

        m_Contents.HudTargetsFocused.Invoke(targets);
    }

    private void FocusTargetsWithCaster(List<DungeonUnit> targets)
    {
        var focusTargets = new List<DungeonUnit>(targets.Count + 1) { m_CurCharacter };
        foreach (var t in targets)
        {
            if (t.ID != m_CurCharacter.ID)
                focusTargets.Add(t);
        }
        FocusUnits(focusTargets);
    }

    private void ShowAllCharacter()
    {
        var allCharacters = m_Allies.Concat(m_Enemies).ToList();
        allCharacters.ForEach(c => c.SetFocus(true));
    }

    private async UniTask ExecuteEnemyPattern()
    {
        await UniTask.WaitForSeconds(1);

        var skillStates = m_Contents.CurSkillTargetStates;
        var allies = m_Allies.Cast<ISkillTarget>().ToList();
        var enemies = m_Enemies.Cast<ISkillTarget>().ToList();
        foreach (var state in skillStates)
        {
            foreach (var e in enemies)
                state.Data.CalculateTargets(e.TransformIndex, enemies, allies, state);
        }

        m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.SetActionPhase));
        await m_DungeonCamera.MoveToActionPhase();

        var enemy = m_CurCharacter as EnemyUnit;
        var patternInfo = enemy.GetPatternInfo();
        if (patternInfo.targets.Count == 0)
        {
            HDebug.LogWarning($"Enemy Pattern : No target found.");
            await UniTask.WaitForSeconds(1);
            await NextAction();
            return;
        }

        FocusTargetsWithCaster(patternInfo.targets);
        if (patternInfo.newPosIndex != -1)
        {
            await ChangeEnemyPosAsync(patternInfo.newPosIndex);
        }
        m_Contents.SetTostActionMsg(skillStates[enemy.ExecuteSkillIndex].Data.Name, enemy.Name);
        await UniTask.WaitForSeconds(1);
        await m_CurCharacter.TriggerDot(EffectType.Bleed);
        if (m_CurCharacter.IsDead)
        {
            await HandleDotDeath();
            return;
        }
        await enemy.ExecutePattern(patternInfo.targets, m_DungeonCamera);
        await CheckAlly();
    }

    private async UniTask AddEnemyToField()
    {
        if (m_ReserveEnemies == null)
            return;

        int addCount = GlobalVariable.PARTY_MEMER_MAX_COUNT - m_Enemies.Count;
        if (addCount > 0)
        {
            int count = Mathf.Min(addCount, m_ReserveEnemies.Count);
            for (int i = 0; i < count; i++)
            {
                var addEnemy = m_ReserveEnemies[0];
                addEnemy.gameObject.SetActive(true);
                var newTr = m_EnemyTrs[m_Enemies.Count];
                _ = Tween.PositionX(addEnemy.transform, newTr.position.x, GlobalVariable.CHARACTER_CHANGING_SPEED, Ease.Linear);
                addEnemy.TransformIndex = m_Enemies.Count;
                m_Enemies.Add(addEnemy);
                m_Contents.HudNewAdded.Invoke(addEnemy);
                m_ReserveEnemies.RemoveAt(0);
            }
            await UniTask.WaitForSeconds(GlobalVariable.CHARACTER_CHANGING_SPEED + 1f);
        }
    }

    private async UniTask CheckCharacters(List<DungeonUnit> characters, List<DungeonUnit> deadCharList,
    Action onAllDead, Action onSort, Func<UniTask> onAddField = null)
    {
        var deadCharacters = characters.Where(c => c.IsDead).ToList();
        if (deadCharacters.Count == 0)
        {
            NextAction().Forget();
            return;
        }

        foreach (var d in deadCharacters)
        {
            deadCharList.Add(d);
            characters.Remove(d);
        }

        if (characters.Count == 0)
        {
            onAllDead();
            return;
        }

        await UniTask.WaitForSeconds(GlobalVariable.DURATION_TIME_DEAD_ANIM);
        onSort();
        await UniTask.WaitForSeconds(GlobalVariable.CHARACTER_CHANGING_SPEED + 0.5f);
        if (onAddField != null)
        {
            ShowAllCharacter();
            m_Contents.HudAllShowed.Invoke();
            await onAddField();
        }
        NextAction().Forget();
    }

    private async UniTask SkipOrder()
    {
        var curOrder = new List<DungeonUnit>(m_PriorityOrder.Count + 1) { m_CurCharacter };
        curOrder.AddRange(m_PriorityOrder);
        m_Contents.DungeonProgress.Battle.SetPriorityOrder(curOrder);
        m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.NextAction));
        await UniTask.WaitForSeconds(GlobalVariable.CHARACTER_CHANGING_SPEED);
        await UniTask.WaitForSeconds(GlobalVariable.DURATION_TIME_SKIP_TURN);
        await NextAction();
    }

    /// <summary>DoT로 행동 유닛이 죽었을 때. 턴을 진행하지 않고 사망 처리로 넘긴다.</summary>
    private async UniTask HandleDotDeath()
    {
        await UniTask.WaitForSeconds(1);
        if (m_CurCharacter is AllyUnit)
            await CheckAlly();
        else
            await CheckEnemy();
    }

    private async UniTask CheckAlly()
    {
        await CheckCharacters(m_Allies, m_DeadAllies, Lose, SortAlly);
    }

    private async UniTask CheckEnemy()
    {
        bool anyDead = m_Enemies.Exists(c => c.IsDead);
        if (anyDead)
        {
            var allies = m_Allies.Cast<ISkillTarget>().ToList();
            m_Contents.TriggerCards(EffectTriggerType.OnKill, allies);
        }
        await CheckCharacters(m_Enemies, m_DeadEnemies, Win, SortEnemy, AddEnemyToField);
    }

    private void SortCharacter(List<DungeonUnit> characters, List<Transform> transforms)
    {
        characters.Sort((a, b) => a.TransformIndex.CompareTo(b.TransformIndex));
        int moveIndex = 0;
        foreach (var c in characters)
        {
            if (c.TransformIndex == moveIndex)
            {
                moveIndex++;
                continue;
            }

            c.TransformIndex = moveIndex;
            Tween.PositionX(c.transform, transforms[moveIndex].position.x,
                GlobalVariable.CHARACTER_CHANGING_SPEED, Ease.Linear);
            moveIndex++;
        }
    }

    private void SortAlly()
    {
        SortCharacter(m_Allies, m_AllyTrs);
    }

    private void SortEnemy()
    {
        SortCharacter(m_Enemies, m_EnemyTrs);
    }

    private void ChangeAllyPos(int directionIndex)
    {
        if (m_IsChanging)
            return;

        int targetIndex = m_CurCharacter.TransformIndex + directionIndex;
        if (targetIndex < 0 || targetIndex >= m_Allies.Count)
            return;

        var targetCharacter = m_Allies.Find(c => c.TransformIndex == targetIndex);
        m_IsChanging = true;
        m_Contents.CanSelectUnitInfo = false;
        m_Contents.CurCharacterTransformIndex = targetCharacter.TransformIndex;
        SwapCharacterPos(m_CurCharacter, targetCharacter);
        UpdateMoveState(targetIndex);
        CalculateSkillTargets();
        CalculateItemTargets();
        m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.UpdateBattleItem));
    }

    private async UniTask ChangeEnemyPosAsync(int newPosIndex)
    {
        var targetCharacter = m_Enemies.Find(e => e.TransformIndex == newPosIndex);
        if (targetCharacter == null)
        {
            HDebug.LogError($"Faild to change enemyPos. newPosIndex: {newPosIndex}");
            return;
        }
        SwapCharacterPos(m_CurCharacter, targetCharacter);
        await UniTask.WaitForSeconds(GlobalVariable.CHARACTER_CHANGING_SPEED);
    }

    private void UpdateMoveState(int curTransformIndex)
    {
        bool canLeftMove = curTransformIndex < m_Allies.Count - 1;
        bool canRightMove = curTransformIndex > 0;
        if (canLeftMove && canRightMove)
            m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.SetMoveBoth));
        else if (canLeftMove)
            m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.SetLeftMove));
        else if (canRightMove)
            m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.SetRightMove));
        else
            m_Contents.Send(new DungeonContentsParam(DungeonContentsParam.EType.SetMoveNone));
    }

    private void SwapCharacterPos(DungeonUnit character1, DungeonUnit character2)
    {
        float curPosX = character1.transform.position.x;
        float targetPosX = character2.transform.position.x;

        int temp = character1.TransformIndex;
        character1.TransformIndex = character2.TransformIndex;
        character2.TransformIndex = temp;

        Tween.PositionX(character2.transform, curPosX, GlobalVariable.CHARACTER_CHANGING_SPEED, Ease.Linear);
        Tween.PositionX(character1.transform, targetPosX, GlobalVariable.CHARACTER_CHANGING_SPEED, Ease.Linear)
            .OnComplete(target: this, target =>
            {
                //적 이동에서도 호출되므로 아군 교체(m_IsChanging)일 때만 클릭을 되살린다.
                if (target.m_IsChanging)
                    target.m_Contents.CanSelectUnitInfo = true;

                target.m_IsChanging = false;
            });
    }


    public void OnEvent(DungeonContentsParam parameter)
    {
        switch (parameter.Type)
        {
            case DungeonContentsParam.EType.LeftChange:
                ChangeAllyPos(1); //역순
                break;
            case DungeonContentsParam.EType.RightChange:
                ChangeAllyPos(-1); //역순
                break;
            case DungeonContentsParam.EType.SetBattle:
                SetBattle();
                break;
            case DungeonContentsParam.EType.StartBattle:
                _ = StartBattle();
                break;
            case DungeonContentsParam.EType.RestartStage:
                _ = RestartStage();
                break;
            case DungeonContentsParam.EType.ExecuteSkill:
                _ = ExecuteSelectedSkill();
                break;
            case DungeonContentsParam.EType.UseItem:
                _ = UseSelectedItem();
                break;
            case DungeonContentsParam.EType.SetPreparePhase:
                ShowAllCharacter();
                m_Contents.HudAllShowed.Invoke();
                m_DungeonCamera.MoveToPreparePhase();
                break;
            case DungeonContentsParam.EType.SetUnitInfo:
                FocusUnits(new List<DungeonUnit> { m_Contents.SelectedUnit });
                m_DungeonCamera.ZoomInToRightHalf(m_Contents.SelectedUnit.transform.position).Forget();
                break;
        }
    }
}
