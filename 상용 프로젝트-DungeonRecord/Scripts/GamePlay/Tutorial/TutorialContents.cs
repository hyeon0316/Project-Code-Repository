using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using LitJson;
using Cysharp.Threading.Tasks;
using UnityEngine;

public struct TutorialContentsParam
{
    public enum EType
    {
        RefreshStep,
        Wait,
        Hide,
    }

    public EType Type;
    public TutorialStep Step;
    public int PageIndex;

    public TutorialContentsParam(EType type, TutorialStep step = null, int pageIndex = 0)
    {
        Type = type;
        Step = step;
        PageIndex = pageIndex;
    }
}

public class TutorialContents : IManagableContents, GlobalEvent<TutorialContentsParam>.IManagableHandler
{
    private class TutorialSaveData
    {
        public int[] Ids;
    }

    private TutorialStep CurrentStep
    {
        get { return m_Tutorial.Steps[m_StepIndex]; }
    }

    private GlobalEvent<TutorialContentsParam> m_GlobalEvent = new();
    private HashSet<int> m_SeenSet = new();
    private TutorialSO m_Tutorial;
    private int m_StepIndex;
    private int m_PageIndex;
    private bool m_IsWaiting;
    private bool m_IsDataChanged;

    public void Initialize()
    {
        LoadData();
        SaveManager.Instance.OnSaveTime += Save;
        ApplicationEventsManager.Instance.OnExit += Save;
        ApplicationEventsManager.Instance.OnPause += Save;
        GameEventReporter.OnReported += OnGameEventReported;
    }

    public void UnInitialize()
    {
        //튜토리얼 진행 중 종료되면 static 이벤트 구독이 남는다.
        UnsubscribeAnchorEvents();
        m_Tutorial = null;

        m_SeenSet.Clear();
        SaveManager.Instance.OnSaveTime -= Save;
        ApplicationEventsManager.Instance.OnExit -= Save;
        ApplicationEventsManager.Instance.OnPause -= Save;
        GameEventReporter.OnReported -= OnGameEventReported;
    }

    private void OnGameEventReported(EGameEventType type, string id, int value)
    {
        if (m_Tutorial != null)
            TryCompleteByEvent(type, id);

        var so = SODatabase.FindTutorialByTrigger(type, id);
        if (so == null)
            return;

        TryTrigger(so.ID);
    }

    public void OnUpdate(float deltaTime)
    {
        m_GlobalEvent.OnUpdate();
    }

    public void RegisterHandler(GlobalEvent<TutorialContentsParam>.IEventHandler handler)
    {
        m_GlobalEvent.RegisterHandler(handler);
    }

    public void UnRegisterHandler(GlobalEvent<TutorialContentsParam>.IEventHandler handler)
    {
        m_GlobalEvent.UnRegisterHandler(handler);
    }

    public void Send(TutorialContentsParam parameter)
    {
        m_GlobalEvent.Send(parameter);
    }

    private void LoadData()
    {
        var jsonData = DataManager.Instance.GetJsonData(GlobalVariable.CONTENTS_TUTORIAL_KEY);
        if (jsonData == null)
            return;

        var json = jsonData.ToString();
        var data = JsonConvert.DeserializeObject<TutorialSaveData>(json);
        if (data?.Ids != null)
        {
            foreach (var id in data.Ids)
                m_SeenSet.Add(id);
        }
    }

    public void TryTrigger(ETutorialID id)
    {
        if (m_Tutorial != null || m_SeenSet.Contains((int)id))
            return;

        var so = SODatabase.GetTutorial(id);
        if (so == null)
            return;

        StartTutorial(so);
    }

    public void ForceTrigger(ETutorialID id)
    {
        var so = SODatabase.GetTutorial(id);
        if (so == null)
            return;

        StartTutorial(so);
    }

    public void Abort()
    {
        if (m_Tutorial == null)
            return;

        HideTutorial();
    }

    public void MarkSeen(ETutorialID id)
    {
        if (m_SeenSet.Add((int)id))
        {
            m_IsDataChanged = true;
        }
    }

    public bool IsSeen(ETutorialID id)
    {
        return m_SeenSet.Contains((int)id);
    }

    private void StartTutorial(TutorialSO tutorial)
    {
        //진행 중 재트리거(ForceTrigger 등) 시 중복 구독을 막는다.
        UnsubscribeAnchorEvents();

        m_Tutorial = tutorial;
        m_StepIndex = 0;
        m_PageIndex = 0;

        TutorialClickAnchor.OnAnchorClicked += OnAnchorClickedHandler;
        TutorialAnchor.OnAnchorAdded += OnAnchorChangedHandler;
        TutorialAnchor.OnAnchorRemoved += OnAnchorChangedHandler;

        EnterStep();
    }

    /// <summary>
    /// 오버레이 클릭(AdvanceArea, 다음 버튼). ClickAnywhere/Pages 스텝에서만 진행한다.
    /// </summary>
    public void AdvancePage()
    {
        if (m_Tutorial == null || m_IsWaiting)
            return;

        var step = CurrentStep;
        if (step.Advance == ETutorialAdvanceType.Pages)
        {
            m_PageIndex++;
            if (m_PageIndex < step.Pages.Length)
            {
                SendRefreshStep();
                return;
            }

            AdvanceStep();
            return;
        }

        if (step.Advance == ETutorialAdvanceType.ClickAnywhere)
            AdvanceStep();
    }

    public void AdvanceStep()
    {
        m_PageIndex = 0;
        m_StepIndex++;

        if (m_StepIndex < m_Tutorial.Steps.Length)
        {
            EnterStep();
        }
        else
        {
            MarkSeen(m_Tutorial.ID);
            HideTutorial();
        }
    }

    /// <summary>
    /// 강조 대상이 화면에 없으면 오버레이를 숨기고 입력을 풀어둔 채 대기한다. 대상이 나타나면 다시 표시한다.
    /// </summary>
    private void EnterStep()
    {
        var step = CurrentStep;
        if (step.HasHighlight && TutorialAnchor.Find(step.HighlightAnchor) == null)
        {
            m_IsWaiting = true;
            Send(new TutorialContentsParam(TutorialContentsParam.EType.Wait));
            return;
        }

        m_IsWaiting = false;
        SendRefreshStep();
    }

    private void OnAnchorChangedHandler(ETutorialAnchorID id)
    {
        if (CurrentStep.HighlightAnchor != id)
            return;

        EnterStep();
    }

    private void OnAnchorClickedHandler(ETutorialAnchorID id)
    {
        var step = CurrentStep;
        if (step.Advance != ETutorialAdvanceType.ClickTarget || step.HighlightAnchor != id)
            return;

        AdvanceStep();
    }

    private void TryCompleteByEvent(EGameEventType type, string id)
    {
        var step = CurrentStep;
        if (step.Advance != ETutorialAdvanceType.GameEvent || step.CompleteEvent != type)
            return;

        if (!string.IsNullOrEmpty(step.CompleteParam) && step.CompleteParam != id)
            return;

        AdvanceStep();
    }

    private void SendRefreshStep()
    {
        Send(new TutorialContentsParam(TutorialContentsParam.EType.RefreshStep, CurrentStep, m_PageIndex));
    }

    private void UnsubscribeAnchorEvents()
    {
        TutorialClickAnchor.OnAnchorClicked -= OnAnchorClickedHandler;
        TutorialAnchor.OnAnchorAdded -= OnAnchorChangedHandler;
        TutorialAnchor.OnAnchorRemoved -= OnAnchorChangedHandler;
    }

    private void HideTutorial()
    {
        UnsubscribeAnchorEvents();

        Send(new TutorialContentsParam(TutorialContentsParam.EType.Hide));
        m_Tutorial = null;
        m_IsWaiting = false;
    }

    private void Save()
    {
        if (!m_IsDataChanged)
            return;


        var data = new TutorialSaveData { Ids = new int[m_SeenSet.Count] };
        m_SeenSet.CopyTo(data.Ids);

        m_IsDataChanged = false;
        SaveManager.Instance.SaveCloudData(GlobalVariable.CONTENTS_TUTORIAL_KEY, data).ContinueWith(success =>
        {
            if (!success)
                m_IsDataChanged = true;
        }).Forget();
    }
}
