using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pages_Achievement : PageBehaviour, GlobalEvent<AchievementContentsParam>.IEventHandler
{
    public static AssetID AssetID = new("Assets/Prefabs/UI/Page/AchievementPage.prefab");

    [SerializeField] private AchievementTypeSO m_TypeSO;
    [SerializeField] private ToggleGroup m_TypeContainer;
    [SerializeField] private Transform m_MissionContainer;
    [SerializeField] private GameObject m_ClaimAllButton;

    private UIItemPool<AchievementItem> m_ItemPool = new();
    private List<AchievementTypeItem> m_TypeItems = new();
    private AchievementContents m_Contents;
    private EMissionType m_CurrentType;

    public override UniTask OnCreate()
    {
        m_Contents = ContentsManager.Instance.Get<AchievementContents>();
        m_Contents.RegisterHandler(this);
        m_ItemPool.Init(m_MissionContainer, AchievementItem.AssetID);
        InitTypeItems();
        return base.OnCreate();
    }

    public override UniTask OnLoad()
    {
        if (m_TypeItems.Count > 0)
        {
            m_TypeItems[0].SetOn(true);
            m_CurrentType = m_TypeItems[0].Type;
        }
        RefreshList();
        return base.OnLoad();
    }

    public override UniTask OnFinish()
    {
        m_Contents.UnRegisterHandler(this);
        m_ItemPool.Clear();
        foreach (var typeItem in m_TypeItems)
            Destroy(typeItem.gameObject);
        m_TypeItems.Clear();
        return base.OnFinish();
    }

    private void InitTypeItems()
    {
        foreach (var entry in m_TypeSO.Entries)
        {
            var item = UIManager.Instance.CreateUIItem<AchievementTypeItem>(m_TypeContainer.transform, AchievementTypeItem.AssetID);
            item.Set(entry, m_TypeContainer, OnTypeSelected);
            m_TypeItems.Add(item);
        }
    }

    private void OnTypeSelected(EMissionType type)
    {
        m_CurrentType = type;
        RefreshList();
    }

    private void RefreshList()
    {
        var missions = MissionProgressHelper.GetMissions(EMissionCategory.Achievement, m_CurrentType);

        m_ItemPool.Set(missions.Count, (item, i) =>
        {
            var mission = missions[i];
            var progress = MissionProgressHelper.GetProgress(mission.MissionID);
            item.Set(mission, progress, m_Contents.GetCompletedDate(mission.MissionID)
            , () => Claim(mission));
        });
        m_ClaimAllButton.SetActive(m_Contents.HasAnyMissionClaimable());
    }

    private void Claim(MissionEntry mission)
    {
        m_Contents.ClaimReward(mission);
        RefreshList();
    }

    public void OnClickEvent_ClaimAll()
    {
        m_Contents.ClaimAllReward();
        RefreshList();
    }

    public void OnEvent(AchievementContentsParam parameter)
    {
        RefreshList();
    }
}
