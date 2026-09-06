using Newtonsoft.Json;
using System.Collections.Generic;
using LitJson;
using Cysharp.Threading.Tasks;

public struct TutorialContentsParam
{
    public enum EType
    {
        Show,
        RefreshStep,
        Hide,
    }

    public EType Type;
    public TutorialSO Tutorial;
    public TutorialStep Step;
    public int PageIndex;
    public bool IsFirstStep;

    public TutorialContentsParam(EType type, TutorialSO tutorial = null, TutorialStep step = null, int pageIndex = 0, bool isFirstStep = false)
    {
        Type = type;
        Tutorial = tutorial;
        Step = step;
        PageIndex = pageIndex;
        IsFirstStep = isFirstStep;
    }
}

public class TutorialContents : IManagableContents, GlobalEvent<TutorialContentsParam>.IManagableHandler
{
    private class TutorialSaveData
    {
        public int[] Ids;
    }

    private GlobalEvent<TutorialContentsParam> m_GlobalEvent = new();
    private HashSet<int> m_SeenSet = new();
    private TutorialSO m_Tutorial;
    private int m_StepIndex;
    private int m_PageIndex;
    private ETutorialAnchorID m_PendingAnchorID;
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
        m_SeenSet.Clear();
        SaveManager.Instance.OnSaveTime -= Save;
        ApplicationEventsManager.Instance.OnExit -= Save;
        ApplicationEventsManager.Instance.OnPause -= Save;
        GameEventReporter.OnReported -= OnGameEventReported;
    }

    private void OnGameEventReported(EGameEventType type, string id, int value)
    {
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
        if (m_SeenSet.Contains((int)id))
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
        m_Tutorial = tutorial;
        m_StepIndex = 0;
        m_PageIndex = 0;

        TutorialClickAnchor.OnAnchorClicked += OnAnchorClickedHandler;

        Send(new TutorialContentsParam(TutorialContentsParam.EType.Show, m_Tutorial));
        TryRefreshOrWaitStep();
    }

    public void AdvancePage()
    {
        var step = m_Tutorial.Steps[m_StepIndex];

        m_PageIndex++;
        if (m_PageIndex < step.Pages.Length)
        {
            SendRefreshStep();
            return;
        }

        AdvanceStep();
    }

    public void AdvanceStep()
    {
        m_PageIndex = 0;
        m_StepIndex++;

        if (m_StepIndex < m_Tutorial.Steps.Length)
        {
            TryRefreshOrWaitStep();
        }
        else
        {
            MarkSeen(m_Tutorial.ID);
            HideTutorial();
        }
    }

    private void TryRefreshOrWaitStep()
    {
        var step = m_Tutorial.Steps[m_StepIndex];
        if (TutorialAnchor.IsRegistered(step.AnchorID))
        {
            SendRefreshStep();
            return;
        }

        m_PendingAnchorID = step.AnchorID;
        TutorialAnchor.OnAnchorRegistered += OnAnchorRegisteredHandler;
    }

    private void OnAnchorRegisteredHandler(ETutorialAnchorID id)
    {
        if (id != m_PendingAnchorID)
            return;

        TutorialAnchor.OnAnchorRegistered -= OnAnchorRegisteredHandler;
        SendRefreshStep();
    }

    private void OnAnchorClickedHandler(ETutorialAnchorID id)
    {
        var step = m_Tutorial.Steps[m_StepIndex];
        if (!step.UseHoleMesh || step.AnchorID != id)
            return;

        AdvanceStep();
    }

    private void SendRefreshStep()
    {
        var step = m_Tutorial.Steps[m_StepIndex];
        Send(new TutorialContentsParam(TutorialContentsParam.EType.RefreshStep, m_Tutorial, step, m_PageIndex, m_StepIndex == 0));
    }

    private void HideTutorial()
    {
        TutorialClickAnchor.OnAnchorClicked -= OnAnchorClickedHandler;
        TutorialAnchor.OnAnchorRegistered -= OnAnchorRegisteredHandler;

        Send(new TutorialContentsParam(TutorialContentsParam.EType.Hide));
        m_Tutorial = null;
    }

    private void Save()
    {
        if (!m_IsDataChanged)
            return;


        var data = new TutorialSaveData { Ids = new int[m_SeenSet.Count] };
        m_SeenSet.CopyTo(data.Ids);

        SaveManager.Instance.SaveCloudData(GlobalVariable.CONTENTS_TUTORIAL_KEY, data).ContinueWith(success =>
        {
            if (!success)
                m_IsDataChanged = true;

            m_IsDataChanged = false;
        }).Forget();
    }
}
