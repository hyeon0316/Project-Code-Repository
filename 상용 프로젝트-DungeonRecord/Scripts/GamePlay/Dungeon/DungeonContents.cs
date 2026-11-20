using BackEnd;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public struct DungeonContentsParam
{
    public enum EType
    {
        LeftChange,
        RightChange,
        Win,
        Lose,
        End,
        SetBattle,
        StartBattle,
        RestartStage,
        SelectCard,
        SetMap,
        SetNextMap,
        SetStory,
        NextRound,
        ExecuteSkill,
        UseItem,
        SetSkillUI,
        UpdateBattleItem,
        SetLeftMove,
        SetRightMove,
        SetMoveBoth,
        SetMoveNone,
        SetPreparePhase,
        SetActionPhase,
        NextAction,
        ShowToastMsg,
        RefreshUsable,
        SetUnitInfo,
        RestartBattle,
        ChangeBattleSpeed,
        RetryDungeon,
    }

    public EType Type;

    public DungeonContentsParam(EType type)
    {
        Type = type;
    }
}

public class DungeonContents : IManagableContents, GlobalEvent<DungeonContentsParam>.IManagableHandler
{
    public bool IsEntered { get; set; }
    public string StoryCutSceneKey { get; private set; }
    public ECardCategory CurCardCategory { get; set; }
    public List<DungeonUnit> Allies { get; private set; } = new();
    public List<DungeonUnit> DeadAllies { get; private set; } = new();
    public DungeonProgress DungeonProgress { get; private set; }
    public TransactionResult LastClearReward { get; private set; }
    public int LastClearRewardBeforeLevel { get; private set; }
    public long LastClearRewardBeforeExp { get; private set; }
    public Dictionary<string, DungeonExploration> DungeonExplorationDic { get; private set; } = new();
    public List<ActiveSkillState> CurSkillTargetStates { get; private set; } = new();
    public DungeonUnit ItemTarget { get; set; }
    public int SelectedIndex { get; private set; }
    public ConsumableItem SelectedItem { get; private set; }
    public DungeonUnit SelectedUnit { get; private set; }
    public bool CanSelectUnitInfo { get; set; }
    public (string main, string sub) ToastActionMsg { get; private set; }
    public int CurCharacterTransformIndex { get; set; }
    public List<UnityEngine.Vector3> EnemyScreenPos { get; private set; } = new();
    public Action<List<DungeonUnit>, List<DungeonUnit>> HudInitialized;
    public Action<DungeonUnit> HudNewAdded;
    public Action<DungeonUnit> HudUnInitialized;
    public Action<DungeonUnit> HudHidden;
    public Action<DungeonUnit> HudValueUpdated;
    public Action<DungeonUnit, EffectType> HudResisted;
    public Action<DungeonUnit, int, bool> HudDamaged;
    public Action<DungeonUnit, int> HudHealed;
    public Action<DungeonUnit, int> HudMpRestored;
    public Action<List<DungeonUnit>> HudTargetsFocused;
    public Action HudAllShowed;

    private GlobalEvent<DungeonContentsParam> m_GlobalEvent = new();
    private bool m_IsExplorationChanged;
    private bool m_IsInProcessSaveExploration;
    public void Initialize()
    {
        LoadProgress();
        LoadExploration();

        SaveManager.Instance.OnSaveTime += SaveExploration;
        ApplicationEventsManager.Instance.OnPause += SaveExploration;
        ApplicationEventsManager.Instance.OnExit += SaveExploration;

    }

    public void UnInitialize()
    {
        SaveManager.Instance.OnSaveTime -= SaveExploration;
        ApplicationEventsManager.Instance.OnPause -= SaveExploration;
        ApplicationEventsManager.Instance.OnExit -= SaveExploration;
        DungeonExplorationDic.Clear();
        DungeonProgress?.DestroyRuntimeTree();
    }

    public void OnUpdate(float deltaTime)
    {
        m_GlobalEvent.OnUpdate();
    }

    public void RegisterHandler(GlobalEvent<DungeonContentsParam>.IEventHandler handler)
    {
        m_GlobalEvent.RegisterHandler(handler);
    }

    public void Send(DungeonContentsParam parameter)
    {
        m_GlobalEvent.Send(parameter);
    }

    public void UnRegisterHandler(GlobalEvent<DungeonContentsParam>.IEventHandler handler)
    {
        m_GlobalEvent.UnRegisterHandler(handler);
    }

    private void LoadProgress()
    {
        DungeonProgress = SaveManager.Instance.LoadPrefsData<DungeonProgress>(GlobalVariable.PREFS_DUNGEON_PROGRESS);
        if (DungeonProgress != null)
        {
            if (!DungeonProgress.Restore())
            {
                HDebug.LogError("Failed to restore dungeon progress");
                SaveManager.Instance.DeletePrefsData(GlobalVariable.PREFS_DUNGEON_PROGRESS);
                return;
            }
            IsEntered = true;
        }
    }

    private void LoadExploration()
    {
        var jsonData = DataManager.Instance.GetJsonData(GlobalVariable.PREFS_DUNGEON_EXPLORATION_NODE_STATE);
        if (jsonData != null)
        {
            DungeonExplorationDic = JsonConvert.DeserializeObject
                <Dictionary<string, DungeonExploration>>(jsonData.ToString());
        }
    }

    public void SaveProgress()
    {
        DungeonProgress.Save();
    }

    public void DeleteProgress()
    {
        IsEntered = false;
        RemoveActiveCards();
        DungeonProgress.Clear();
    }

    /// <summary>
    /// 던전을 클리어한 상태로 마무리한다.
    /// 저장본만 지우고 진행 데이터는 남겨 결과 화면이 읽을 수 있게 한다.
    /// 실제 파기는 마을로 나갈 때 DeleteProgress()가 담당한다.
    /// </summary>
    public void CompleteDungeon()
    {
        IsEntered = false;
        SaveManager.Instance.DeletePrefsData(GlobalVariable.PREFS_DUNGEON_PROGRESS);
    }

    public void SetSkill(List<ActiveSkill> skills)
    {
        CurSkillTargetStates.ForEach(skillSet => skillSet.Clear());
        CurSkillTargetStates.Clear();

        foreach (var skill in skills)
        {
            var skillState = new ActiveSkillState();
            skillState.Data = skill.Data;
            CurSkillTargetStates.Add(skillState);
        }
    }

    public void SelectSkill(int skillIndex)
    {
        CanSelectUnitInfo = false;
        SelectedIndex = skillIndex;
        Send(new DungeonContentsParam(DungeonContentsParam.EType.ExecuteSkill));
        GameEventReporter.Report(EGameEventType.UseSkill, DungeonProgress.DungeonData.ID);
    }

    public void SelectItem(ConsumableItem item)
    {
        CanSelectUnitInfo = false;
        SelectedItem = item;
        Send(new DungeonContentsParam(DungeonContentsParam.EType.UseItem));
    }

    public void SelectUnitInfo(DungeonUnit unit)
    {
        if (!CanSelectUnitInfo)
            return;

        CanSelectUnitInfo = false;
        SelectedUnit = unit;
        Send(new DungeonContentsParam(DungeonContentsParam.EType.SetUnitInfo));
    }

    public IReadOnlyList<ECharacterType> GetPartyMembers()
    {
        var entryParty = DungeonProgress.EntryParty;
        if (entryParty.Count == 0)
            return ContentsManager.Instance.Get<PartySettingContents>().PartyInfo.GetMainPartyMembers();

        return entryParty.ConvertAll(m => m.Type);
    }

    public void SetAllies(List<DungeonUnit> allies, List<DungeonUnit> deadAllies)
    {
        Allies = allies;
        DeadAllies = deadAllies;
    }

    public void RegisterActiveCard(CardSO card)
    {
        DungeonProgress.ActiveCards.Add(card);
        DungeonProgress.Save();
        foreach (var ally in Allies)
            card.ApplyEffect(ally);
    }

    public void ApplyActiveCards()
    {
        foreach (var card in DungeonProgress.ActiveCards)
        {
            foreach (var ally in Allies)
                card.ApplyEffect(ally);
        }
    }

    public void RemoveActiveCards()
    {
        foreach (var card in DungeonProgress.ActiveCards)
        {
            foreach (var ally in Allies)
                card.RemoveEffect(ally);
        }
        DungeonProgress.ActiveCards.Clear();
    }

    public void SetEnemies(List<EEnemyType> enemies)
    {
        var stageEnemyTypes = DungeonProgress.Battle.StageEnemyTypes;
        stageEnemyTypes.Clear();
        foreach (var enemy in enemies)
        {
            stageEnemyTypes.Add(enemy);
        }

        Send(new DungeonContentsParam(DungeonContentsParam.EType.SetBattle));
    }

    public void SetStory(string sceneKey)
    {
        StoryCutSceneKey = sceneKey;
        Send(new DungeonContentsParam(DungeonContentsParam.EType.SetStory));
    }

    public void SetCurDungeon(DungeonSO data, int difficultyPhase = 0)
    {
        if (data.SaveProgressRate && !data.IsRandomMap)
        {
            if (!DungeonExplorationDic.ContainsKey(data.ID))
                DungeonExplorationDic[data.ID] = new DungeonExploration(data);
        }
        DungeonProgress = new DungeonProgress();
        DungeonProgress.Init(data, difficultyPhase);
    }

    public void NotifyNodeCompleted()
    {
        var node = DungeonProgress.ActiveTree.FindNode(DungeonProgress.LastNodeID);
        var labyrinth = DungeonProgress.EndContent as LabyrinthSO;
        if (labyrinth != null && node is EnemyStageNode enemyNode && !DungeonProgress.ClearedNodeIDs.Contains(node.GUID))
        {
            DungeonProgress.Score += UnityEngine.Mathf.RoundToInt(labyrinth.GetGradePoint(enemyNode.Grade)
                * enemyNode.Enemies.Count * DungeonProgress.ScaleMultiplier);
        }
        RecordCompltedNode(DungeonProgress.LastNodeID);
        if (node == null || node.Children.Count > 0)
        {
            DungeonProgress.StageType = EDungeonState.Map;
            DungeonProgress.Save();
            Send(new DungeonContentsParam(DungeonContentsParam.EType.SetMap));
            return;
        }

        RecordExplorationState(node.GUID);

        var dungeonData = DungeonProgress.DungeonData;
        if (labyrinth != null)
        {
            DungeonProgress.Score += labyrinth.MapClearPoint;
            AdvanceToNextRandomMap();
            return;
        }

        if (dungeonData.IsRandomMap
            && DungeonProgress.RandomMapIndex < dungeonData.RandomMapConfig.MapCount - 1)
        {
            AdvanceToNextRandomMap();
            return;
        }

        FinishDungeon();
    }

    public void FinishDungeon()
    {
        var dungeonData = DungeonProgress.DungeonData;
        LastClearReward = null;

        RemoveActiveCards();
        GameEventReporter.Report(EGameEventType.ClearDungeon, dungeonData.ID);
        if (dungeonData.MapType is EMapType.Labyrinth or EMapType.Trial or EMapType.Tower)
            GameEventReporter.Report(EGameEventType.ClearEndContent, dungeonData.MapType.ToString());
        Send(new DungeonContentsParam(DungeonContentsParam.EType.End));
        CompleteDungeon();
    }

    public async UniTask<bool> ClaimClearRewardAsync()
    {
        int cost = DungeonProgress.DungeonData.StaminaCost;
        var stamina = ContentsManager.Instance.Get<StaminaContents>();
        if (cost > 0 && !stamina.TryConsumeStamina(cost))
            return false;

        if (IsRewardClaimed())
            return false;

        string rewardID = GetCurRewardID();
        if (!await RequestRewardAsync(rewardID, cost, rewardID))
        {
            if (cost > 0)
                stamina.AddStamina(cost);
            return false;
        }

        if (cost != 0)
            GameEventReporter.Report(EGameEventType.SpendStamina, "", cost);
        return true;
    }

    public async UniTask<bool> ClaimOneTimeRewardAsync(string rewardID)
    {
        return await ClaimOneTimeRewardByKeyAsync(rewardID, rewardID);
    }

    public async UniTask<bool> ClaimTrialRewardAsync(string rewardID)
    {
        var endContent = ContentsManager.Instance.Get<EndContentContents>();
        return await ClaimOneTimeRewardByKeyAsync(rewardID, endContent.GetTrialClaimKey(rewardID));
    }

    private async UniTask<bool> ClaimOneTimeRewardByKeyAsync(string rewardID, string claimKey)
    {
        if (UserManager.Instance.UserData.ClaimedDungeonRewardIDs.Contains(claimKey))
            return false;

        return await RequestRewardAsync(rewardID, 0, claimKey);
    }

    private async UniTask<bool> RequestRewardAsync(string rewardID, int cost, string claimKey)
    {
        if (cost == 0 && !BDatabase.HasWearableReward(rewardID))
        {
            GrantOptimistic(rewardID, claimKey);
            return true;
        }

        using (new LoadingScope(ELoadingType.Center))
        {
            var bro = await BFuncInvoker.InvokeAsync("DungeonReward", BuildRewardParam(rewardID, cost, claimKey));
            var result = BFuncResponseHandler.Parse(bro, "DungeonReward/Claim");
            if (result == null)
                return false;

            LastClearReward = JsonConvert.DeserializeObject<TransactionResult>(result.ToString());
            var inventory = ContentsManager.Instance.Get<InventoryContents>();
            inventory.AddStackableItem(LastClearReward.StackableDic);
            inventory.AddEquipItem(LastClearReward.EquipItemDic);
            //AddExp는 만렙에서 초과분을 버리므로 되돌릴 수 없다. 연출 시작점은 여기서 찍어 둔다.
            var expTranslation = UserManager.Instance.ExpTranslation;
            LastClearRewardBeforeLevel = expTranslation.CurLevel;
            LastClearRewardBeforeExp = expTranslation.CurExp;
            if (expTranslation.AddExp(LastClearReward.GainedExp))
                ContentsManager.Instance.Get<QuestContents>().RefreshQuestAvailability();
            if (cost == 0)
                UserManager.Instance.UserData.ClaimedDungeonRewardIDs.Add(claimKey);
            return true;
        }
    }

    private Param BuildRewardParam(string rewardID, int cost, string claimKey)
    {
        var param = new Param();
        param.Add("rewardID", rewardID);
        if (claimKey != rewardID)
            param.Add("isTrial");
        param.Add("RewardTableID", BDatabase.TableIDDic["DungeonRewardTable"]);
        param.Add("inventoryConfigTableID", BDatabase.TableIDDic["InventoryConfigTable"]);
        //서버가 유저 exp 상한을 검사해 GainedExp를 잘라 내려준다.
        param.Add("expTableID", BDatabase.TableIDDic["ExpTable"]);
        if (cost != 0)
            param.Add("stamina");

        if (BDatabase.HasWearableReward(rewardID))
        {
            param.Add("isWearable");
            param.Add("wearableDropTableID", BDatabase.TableIDDic["DungeonWearableDropTable"]);
            param.Add("wearableSubStatTableID", BDatabase.TableIDDic["WearableSubStatTable"]);
            param.Add("wearableTypeMainStatTableID", BDatabase.TableIDDic["WearableTypeMainTable"]);
        }
        return param;
    }

    private void GrantOptimistic(string rewardID, string claimKey)
    {
        var grant = new TransactionResult();
        foreach (var entry in BDatabase.DungeonRewardDic[rewardID])
        {
            if (ItemID.IsExp(entry.ItemID))
                grant.GainedExp += entry.Min;
            else
                grant.StackableDic[entry.ItemID] = grant.StackableDic.GetValueOrDefault(entry.ItemID) + entry.Min;
        }

        var appliedDic = ContentsManager.Instance.Get<InventoryContents>().AddStackableItem(grant.StackableDic);
        var expTranslation = UserManager.Instance.ExpTranslation;
        LastClearRewardBeforeLevel = expTranslation.CurLevel;
        LastClearRewardBeforeExp = expTranslation.CurExp;
        grant.GainedExp = Math.Min(grant.GainedExp, expTranslation.ExpRemainingToMax());
        if (expTranslation.AddExp(grant.GainedExp))
            ContentsManager.Instance.Get<QuestContents>().RefreshQuestAvailability();

        UserManager.Instance.UserData.ClaimedDungeonRewardIDs.Add(claimKey);
        LastClearReward = grant;
        SendClaim(rewardID, claimKey, grant.GainedExp, appliedDic).Forget();
    }

    private async UniTask SendClaim(string rewardID, string claimKey, long gainedExp, Dictionary<int, int> appliedDic)
    {
        var bro = await BFuncInvoker.InvokeAsync("DungeonReward", BuildRewardParam(rewardID, 0, claimKey), BFuncInvoker.RETRY_MAX);
        bool isRetryable = BFuncResponseHandler.IsRetryable(bro);
        if (BFuncResponseHandler.Parse(bro, "DungeonReward/ClaimOptimistic") != null || isRetryable)
            return;

        ContentsManager.Instance.Get<InventoryContents>().RemoveStackableItem(appliedDic);
        if (gainedExp > 0)
        {
            var expTranslation = UserManager.Instance.ExpTranslation;
            expTranslation.SetTotalExp(expTranslation.GetTotalAccumulatedExp() - gainedExp);
        }
        UserManager.Instance.UserData.ClaimedDungeonRewardIDs.Remove(claimKey);
    }

    public string GetCurRewardID()
    {
        return DungeonProgress.DungeonData.GetRewardID(DungeonProgress.DifficultyPhase);
    }

    public bool IsRewardClaimed()
    {
        return UserManager.Instance.UserData.
            ClaimedDungeonRewardIDs.Contains(GetCurRewardID());
    }

    public void RetryDungeon()
    {
        RemoveActiveCards();
        SetCurDungeon(DungeonProgress.DungeonData, DungeonProgress.DifficultyPhase);
        Send(new DungeonContentsParam(DungeonContentsParam.EType.RetryDungeon));
    }

    /// <summary>
    /// 패배한 전투 스테이지를 진입 시점 상태로 되돌려 다시 시작한다.
    /// </summary>
    public void RestartStage()
    {
        DungeonProgress.Battle.EnemyRecords.Clear();
        DungeonProgress.Battle.DeadEnemyRecords.Clear();
        DungeonProgress.Battle.PriorityOrderRecords.Clear();
        DungeonProgress.Battle.DamageRecords.Clear();
        Send(new DungeonContentsParam(DungeonContentsParam.EType.RestartStage));
    }

    public void AdvanceToNextRandomMap()
    {
        DungeonProgress.AdvanceRandomMap();
        DungeonProgress.Save();
        Send(new DungeonContentsParam(DungeonContentsParam.EType.SetNextMap));
    }

    public void SetEnemyScreenPos(List<UnityEngine.Transform> transoforms)
    {
        if (EnemyScreenPos.Count != 0)
            return;

        foreach (var tr in transoforms)
        {
            var screenPos = UnityEngine.Camera.main.WorldToScreenPoint(tr.position);
            EnemyScreenPos.Add(screenPos);
        }
    }

    public void SetTostActionMsg(string main, string sub)
    {
        ToastActionMsg = (main, sub);
        Send(new DungeonContentsParam(DungeonContentsParam.EType.ShowToastMsg));
    }

    public List<DungeonUnit> GetSkillTargets(int skillIndex)
    {
        var targets = CurSkillTargetStates[skillIndex].TargetsByPosDic[CurCharacterTransformIndex];
        var result = new List<DungeonUnit>(targets.Count);
        foreach (var t in targets)
            result.Add((DungeonUnit)t);
        return result;
    }

    public bool GetNodeExplorationState(string id)
    {
        if (DungeonProgress.DungeonData.IsRandomMap)
            return id == DungeonProgress.LastNodeID || DungeonProgress.ClearedNodeIDs.Contains(id);

        return DungeonExplorationDic[DungeonProgress.DungeonData.ID].NodeExplorationStateDic[id];
    }

    public int GetExplorationRate()
    {
        if (!DungeonExplorationDic.TryGetValue(DungeonProgress.DungeonData.ID, out var exploration))
            return -1;

        return exploration.GetExplorationRate();
    }

    private void RecordCompltedNode(string nodeID)
    {
        if (string.IsNullOrEmpty(nodeID) || DungeonProgress.ClearedNodeIDs.Contains(nodeID))
            return;

        DungeonProgress.ClearedNodeIDs.Add(nodeID);
    }

    private void SaveExploration()
    {
        if (!m_IsExplorationChanged || m_IsInProcessSaveExploration)
            return;

        m_IsInProcessSaveExploration = true;

        SaveManager.Instance.SaveCloudData(GlobalVariable.PREFS_DUNGEON_EXPLORATION_NODE_STATE,
            DungeonExplorationDic).ContinueWith((bool isSuccess) =>
             {
                 m_IsExplorationChanged = false;
                 m_IsInProcessSaveExploration = false;
             }).Forget();
    }

    public void RecordExplorationState(string nodeID)
    {
        string dungeonID = DungeonProgress.DungeonData.ID;
        if (!DungeonExplorationDic.ContainsKey(dungeonID) ||
            !DungeonExplorationDic[dungeonID].NodeExplorationStateDic.ContainsKey(nodeID))
            return;

        DungeonExplorationDic[dungeonID].SetNodeExplorationState(nodeID, true);
        m_IsExplorationChanged = true;
    }
}
