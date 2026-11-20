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
    public DungeonSO CurDungeonData { get; private set; }
    public int CurDifficultyPhase { get; private set; }
    public string CurScalingID { get { return CurDungeonData.GetScalingID(CurDifficultyPhase); } }
    public RuntimeMapTree RuntimeTree { get; private set; }
    public IMapTree ActiveTree { get { return CurDungeonData.IsRandomMap ? (IMapTree)RuntimeTree : CurDungeonData.Tree; } }
    public ECardCategory CurCardCategory { get; set; }
    public List<CardSO> ActiveCards { get; private set; } = new();
    public List<DungeonUnit> Allies { get; private set; } = new();
    public List<EEnemyType> CurEnemies { get; private set; } = new(GlobalVariable.PARTY_LIST_MAX_COUNT);
    public DungeonProgress DungeonProgress { get; private set; }
    public RewardResult LastClearReward { get; private set; }
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
    public Action<DungeonUnit> HudValueUpdated;
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
        DestroyRuntimeTree();
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
            var data = SODatabase.DungeonList.Find(d => d.ID == DungeonProgress.DungeonID);
            if (data == null)
            {
                HDebug.LogError("Failed to finding dungeonSO");
                return;
            }
            IsEntered = true;
            CurDungeonData = data;
            CurDifficultyPhase = DungeonProgress.DifficultyPhase;

            if (data.IsRandomMap)
                RuntimeTree = RandomMapGenerator.Generate(data.RandomMapConfig, DungeonProgress.RandomMapSeed);
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
        DungeonProgress.Clear();
        DestroyRuntimeTree();
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

    public void SetAllies(List<DungeonUnit> allies)
    {
        Allies = allies;
    }

    public void RegisterActiveCard(CardSO card)
    {
        ActiveCards.Add(card);
        DungeonProgress.ActiveCardIDs.Add(card.name);
        DungeonProgress.Save();
        foreach (var ally in Allies)
            card.ApplyEffect(ally);
    }

    public void RemoveActiveCards()
    {
        foreach (var card in ActiveCards)
        {
            foreach (var ally in Allies)
                card.RemoveEffect(ally);
        }
        ActiveCards.Clear();
    }

    public void TriggerCards(EffectTriggerType triggerType, List<ISkillTarget> allies)
    {
        foreach (var card in ActiveCards)
        {
            if (card is ITriggerCard triggerCard && triggerCard.TriggerType == triggerType)
                triggerCard.OnTrigger(allies);
        }
    }

    public void SetEnemies(List<EEnemyType> enemies)
    {
        CurEnemies.Clear();
        foreach (var enemy in enemies)
        {
            CurEnemies.Add(enemy);
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
        CurDungeonData = data;
        CurDifficultyPhase = difficultyPhase;
        if (data.SaveProgressRate && !data.IsRandomMap)
        {
            if (!DungeonExplorationDic.ContainsKey(data.ID))
                DungeonExplorationDic[data.ID] = new DungeonExploration(data);
        }
        DungeonProgress = new DungeonProgress();
        DungeonProgress.DungeonID = data.ID;
        DungeonProgress.DifficultyPhase = CurDifficultyPhase;

        if (data.IsRandomMap)
        {
            int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            DungeonProgress.RandomMapSeed = seed;
            DungeonProgress.RandomMapIndex = 0;
            RuntimeTree = RandomMapGenerator.Generate(data.RandomMapConfig, seed);
        }
    }

    public void NotifyNodeCompleted()
    {
        var node = ActiveTree.FindNode(DungeonProgress.LastNodeID);
        RecordCompltedNode(DungeonProgress.LastNodeID);
        if (node == null || node.Children.Count > 0)
        {
            DungeonProgress.StageType = EDungeonState.Map;
            DungeonProgress.Save();
            Send(new DungeonContentsParam(DungeonContentsParam.EType.SetMap));
            return;
        }

        RecordExplorationState(node.GUID);

        if (CurDungeonData.IsRandomMap
            && DungeonProgress.RandomMapIndex < CurDungeonData.RandomMapConfig.MapCount - 1)
        {
            AdvanceToNextRandomMap();
            return;
        }

        LastClearReward = null;

        RemoveActiveCards();
        GameEventReporter.Report(EGameEventType.ClearDungeon, CurDungeonData.ID);
        Send(new DungeonContentsParam(DungeonContentsParam.EType.End));
        CompleteDungeon();
    }

    public async UniTask<bool> ClaimClearRewardAsync()
    {
        int cost = CurDungeonData.StaminaCost;
        var stamina = ContentsManager.Instance.Get<StaminaContents>();
        if (cost > 0 && !stamina.TryConsumeStamina(cost))
            return false;

        if (IsRewardClaimed())
            return false;

        using (new LoadingScope(ELoadingType.Center))
        {
            string rewardID = GetCurRewardID();
            var param = new Param();
            param.Add("rewardID", rewardID);
            param.Add("RewardTableID", BDatabase.TableIDDic["DungeonRewardTable"]);
            param.Add("inventoryConfigTableID", BDatabase.TableIDDic["InventoryConfigTable"]);
            if (cost != 0)
                param.Add("stamina");

            if (BDatabase.HasWearableReward(rewardID))
            {
                param.Add("isWearable");
                param.Add("wearableDropTableID", BDatabase.TableIDDic["DungeonWearableDropTable"]);
                param.Add("wearableSubStatTableID", BDatabase.TableIDDic["WearableSubStatTable"]);
                param.Add("wearableTypeMainStatTableID", BDatabase.TableIDDic["WearableTypeMainTable"]);
            }

            BackendReturnObject bro = null;
            SendQueue.Enqueue(Backend.BFunc.InvokeFunction, "DungeonReward", param, callback =>
            {
                bro = callback;
            });
            await UniTask.WaitUntil(() => bro != null);

            var result = BFuncResponseHandler.Parse(bro, "DungeonReward/Claim");
            if (result == null)
            {
                if (cost > 0)
                    stamina.AddStamina(cost);
                return false;
            }

            LastClearReward = JsonConvert.DeserializeObject<RewardResult>(result.ToString());
            var inventory = ContentsManager.Instance.Get<InventoryContents>();
            inventory.AddStackableItem(LastClearReward.Inventory.StackableDic);
            inventory.AddEquipItem(LastClearReward.Inventory.EquipItemDic);
            UserManager.Instance.UserData.TotalExp += LastClearReward.GainedExp;
            if (cost == 0)
                UserManager.Instance.UserData.ClaimedDungeonRewardIDs.Add(rewardID);
            return true;
        }
    }

    public string GetCurRewardID()
    {
        return CurDungeonData.GetRewardID(CurDifficultyPhase);
    }

    public bool IsRewardClaimed()
    {
        return UserManager.Instance.UserData.
            ClaimedDungeonRewardIDs.Contains(GetCurRewardID());
    }

    public void RetryDungeon()
    {
        SetCurDungeon(CurDungeonData, CurDifficultyPhase);
        Send(new DungeonContentsParam(DungeonContentsParam.EType.SetMap));
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
        DungeonProgress.RandomMapIndex++;
        int nextSeed = DungeonProgress.RandomMapSeed + DungeonProgress.RandomMapIndex;
        RuntimeTree = RandomMapGenerator.Generate(CurDungeonData.RandomMapConfig, nextSeed);
        DungeonProgress.LastNodeID = RuntimeTree.GetFirstNode().GUID;
        DungeonProgress.StageType = EDungeonState.Map;
        DungeonProgress.ClearedNodeIDs.Clear();
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
        return DungeonExplorationDic[CurDungeonData.ID].NodeExplorationStateDic[id];
    }

    public int GetExplorationRate()
    {
        if (!DungeonExplorationDic.TryGetValue(CurDungeonData.ID, out var exploration))
            return -1;

        return exploration.GetExplorationRate();
    }

    private void RecordCompltedNode(string nodeID)
    {
        if (string.IsNullOrEmpty(nodeID) || DungeonProgress.ClearedNodeIDs.Contains(nodeID))
            return;

        DungeonProgress.ClearedNodeIDs.Add(nodeID);
    }

    private void DestroyRuntimeTree()
    {
        if (RuntimeTree == null)
            return;

        foreach (var node in RuntimeTree.Nodes)
            UnityEngine.Object.Destroy(node);

        RuntimeTree = null;
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
        if (!DungeonExplorationDic.ContainsKey(CurDungeonData.ID) ||
            !DungeonExplorationDic[CurDungeonData.ID].NodeExplorationStateDic.ContainsKey(nodeID))
            return;

        DungeonExplorationDic[CurDungeonData.ID].SetNodeExplorationState(nodeID, true);
        m_IsExplorationChanged = true;
    }
}
