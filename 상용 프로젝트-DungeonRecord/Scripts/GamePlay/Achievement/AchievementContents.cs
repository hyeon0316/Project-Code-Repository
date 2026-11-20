using Newtonsoft.Json;
using System.Collections.Generic;
using BackEnd;
using Cysharp.Threading.Tasks;

public struct AchievementContentsParam
{
    public enum EType { Completed, RewardClaimed }

    public EType Type;

    public AchievementContentsParam(EType type)
    {
        Type = type;
    }
}

public class AchievementContents : IManagableContents, GlobalEvent<AchievementContentsParam>.IManagableHandler
{
    private GlobalEvent<AchievementContentsParam> m_GlobalEvent = new();
    private HashSet<string> m_ClaimedSet;
    private AchievementProgressRecord m_ProgressRecord;
    private bool m_IsProgressDataChanged;

    public void Initialize()
    {
        LoadClaimedData();
        LoadProgressData();
        MissionProgressHelper.RegisterStore(EMissionCategory.Achievement, m_ProgressRecord.ProgressDic, m_ClaimedSet);
        MissionProgressHelper.OnProgressChanged += OnProgressChanged;
        MissionProgressHelper.OnMissionCompleted += OnMissionCompleted;
        ApplicationEventsManager.Instance.OnExit += SaveProgressData;
        ApplicationEventsManager.Instance.OnPause += SaveProgressData;
        SaveManager.Instance.OnSaveTime += SaveProgressData;
    }

    public void UnInitialize()
    {
        MissionProgressHelper.OnProgressChanged -= OnProgressChanged;
        MissionProgressHelper.OnMissionCompleted -= OnMissionCompleted;
        MissionProgressHelper.UnregisterStore(EMissionCategory.Achievement);
        ApplicationEventsManager.Instance.OnExit -= SaveProgressData;
        ApplicationEventsManager.Instance.OnPause -= SaveProgressData;
        SaveManager.Instance.OnSaveTime -= SaveProgressData;
    }

    public void OnUpdate(float deltaTime)
    {
        m_GlobalEvent.OnUpdate();
    }

    public void RegisterHandler(GlobalEvent<AchievementContentsParam>.IEventHandler handler)
    {
        m_GlobalEvent.RegisterHandler(handler);
    }

    public void UnRegisterHandler(GlobalEvent<AchievementContentsParam>.IEventHandler handler)
    {
        m_GlobalEvent.UnRegisterHandler(handler);
    }

    public void Send(AchievementContentsParam parameter)
    {
        m_GlobalEvent.Send(parameter);
    }

    private void LoadClaimedData()
    {
        var jsonData = DataManager.Instance.GetJsonData(GlobalVariable.CONTENTS_ACHIEVEMENT_KEY);
        if (jsonData != null)
            m_ClaimedSet = JsonConvert.DeserializeObject<HashSet<string>>(jsonData["records"].ToJson());
        else
            m_ClaimedSet = new HashSet<string>();
    }

    private void LoadProgressData()
    {
        var jsonData = DataManager.Instance.GetJsonData(GlobalVariable.CONTENTS_ACHIEVEMENT_PROGRESS_KEY);
        if (jsonData != null)
            m_ProgressRecord = JsonConvert.DeserializeObject<AchievementProgressRecord>(jsonData.ToString());
        else
            m_ProgressRecord = new AchievementProgressRecord();
    }

    private void SaveProgressData()
    {
        if (!m_IsProgressDataChanged)
            return;

        SaveManager.Instance.SaveCloudData(GlobalVariable.CONTENTS_ACHIEVEMENT_PROGRESS_KEY, m_ProgressRecord).ContinueWith(success =>
        {
            if (!success)
            {
                HDebug.LogError("[Achievement] SaveProgress failed");
                return;
            }
            m_IsProgressDataChanged = false;
        }).Forget();
    }

    public bool HasAnyMissionClaimable()
    {
        return MissionProgressHelper.HasAnyClaimable(EMissionCategory.Achievement);
    }

    public string GetCompletedDate(string missionID)
    {
        m_ProgressRecord.CompletedDateDic.TryGetValue(missionID, out var date);
        return date;
    }

    public void ClaimReward(MissionEntry entry)
    {
        if (!IsClaimable(entry.MissionID))
            return;

        var snapshot = new MissionProgress
        {
            State = m_ProgressRecord.ProgressDic[entry.MissionID].State,
            IsClaimed = false
        };

        var grantDic = new Dictionary<int, int> { { (int)ECostType.GEM, entry.RewardValue } };
        ContentsManager.Instance.Get<InventoryContents>().AddStackableItem(grantDic);

        MissionProgressHelper.RemoveTracker(entry.MissionID);
        m_ProgressRecord.ProgressDic.Remove(entry.MissionID);
        m_ClaimedSet.Add(entry.MissionID);
        m_IsProgressDataChanged = true;

        ClaimReward(new List<string> { entry.MissionID },
            new Dictionary<string, MissionProgress> { { entry.MissionID, snapshot } },
            grantDic
        ).Forget();

        Send(new AchievementContentsParam(AchievementContentsParam.EType.RewardClaimed));
    }

    public void ClaimAllReward()
    {
        var toClaim = new List<string>();
        foreach (var missionID in new List<string>(m_ProgressRecord.ProgressDic.Keys))
        {
            if (IsClaimable(missionID))
                toClaim.Add(missionID);
        }

        if (toClaim.Count == 0)
            return;

        var snapshots = new Dictionary<string, MissionProgress>();
        var totalGrantDic = new Dictionary<int, int> { { (int)ECostType.GEM, 0 } };

        foreach (var missionID in toClaim)
        {
            var progress = m_ProgressRecord.ProgressDic[missionID];
            snapshots[missionID] = new MissionProgress { State = progress.State, IsClaimed = progress.IsClaimed };

            if (BDatabase.MissionDic.TryGetValue(missionID, out var entry))
                totalGrantDic[(int)ECostType.GEM] += entry.RewardValue;

            MissionProgressHelper.RemoveTracker(missionID);
            m_ProgressRecord.ProgressDic.Remove(missionID);
            m_ClaimedSet.Add(missionID);
        }

        ContentsManager.Instance.Get<InventoryContents>().AddStackableItem(totalGrantDic);
        m_IsProgressDataChanged = true;

        ClaimReward(toClaim, snapshots, totalGrantDic).Forget();
        Send(new AchievementContentsParam(AchievementContentsParam.EType.RewardClaimed));
    }

    private async UniTask ClaimReward(List<string> missionIDs, Dictionary<string, MissionProgress> snapshots, Dictionary<int, int> grantDic)
    {
        var param = new Param();
        param.Add("missionIDsJson", JsonConvert.SerializeObject(missionIDs));
        param.Add("missionTableID", BDatabase.TableIDDic["MissionTableID"]);
        param.Add("inventoryConfigTableID", BDatabase.TableIDDic["InventoryConfigTableID"]);

        GamePopupTemplate.ShowRewardPopup(new InventoryChangeResult { StackableDic = grantDic });

        BackendReturnObject bro = null;
        SendQueue.Enqueue(Backend.BFunc.InvokeFunction, "Achievement", param, callback =>
        {
            bro = callback;
        });
        await UniTask.WaitUntil(() => bro != null);

        var result = BFuncResponseHandler.Parse(bro, "Achievement/ClaimReward");
        if (result == null)
        {
            var inventory = ContentsManager.Instance.Get<InventoryContents>();
            inventory.RemoveStackableItem(grantDic);

            foreach (var missionID in missionIDs)
            {
                m_ClaimedSet.Remove(missionID);
                if (snapshots.TryGetValue(missionID, out var snapshot))
                    MissionProgressHelper.RestoreTracker(missionID, snapshot);
            }

            m_IsProgressDataChanged = true;
            Send(new AchievementContentsParam(AchievementContentsParam.EType.RewardClaimed));
            return;
        }
    }

    private bool IsClaimable(string missionID)
    {
        return m_ProgressRecord.ProgressDic.ContainsKey(missionID)
            && !m_ClaimedSet.Contains(missionID)
            && MissionProgressHelper.IsCompleted(missionID);
    }


    private void OnProgressChanged(EMissionCategory category)
    {
        if (category != EMissionCategory.Achievement)
            return;
        m_IsProgressDataChanged = true;
    }

    private void OnMissionCompleted(MissionEntry entry)
    {
        if (entry.Category != EMissionCategory.Achievement)
            return;

        m_ProgressRecord.CompletedDateDic[entry.MissionID] = BServerTime.Get().ToString("yyyy-MM-dd");
        m_IsProgressDataChanged = true;

        UIManager.Instance.ShowAchievementToast(entry.Name);
        Send(new AchievementContentsParam(AchievementContentsParam.EType.Completed));
    }
}
