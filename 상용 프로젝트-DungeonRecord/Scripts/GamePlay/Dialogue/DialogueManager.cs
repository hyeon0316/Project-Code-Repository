using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class DialogueManager : SingletonLoadAsset<DialogueManager>
{
    private const float DELAY_SHOW_PANEL = 1f;

    public BubblePanel UI => m_DialogueUI;
    public Actor CurrentActor => m_CurrentActor;

    [SerializeField] private BubblePanel m_DialogueUI;
    [SerializeField] private DialoguePanel m_NarrativeUI;
    [SerializeField] private LetterBox m_LetterBox;
    [SerializeField] private ClickArea m_SkipButton;
    [SerializeField] private float m_DefaultTypingSpeed;

    private DialogueNodeExecutorRegistry m_NodeExecutor = new();
    private DialogueGraph m_Graph;
    private bool m_IsStart;
    private Action m_OnFinished;
    private List<DialogueNodeOption> m_DynamicOptions = new();
    private Actor m_CurrentActor;
    private Action m_CurrentSkipHandler;

    private void Awake()
    {
        m_DialogueUI.gameObject.SetActive(false);
        m_NarrativeUI.gameObject.SetActive(false);
        m_LetterBox.SetVisible(false);
        m_SkipButton.gameObject.SetActive(false);
    }

    private void SetSkipHandler(Action handler)
    {
        if (m_CurrentSkipHandler != null)
            m_SkipButton.OnClick -= m_CurrentSkipHandler;
        m_CurrentSkipHandler = handler;
        if (handler != null)
            m_SkipButton.OnClick += handler;
    }

    public void SetGraph(DialogueGraph graph)
    {
        m_Graph = graph;
        m_Graph.SetFirstNode();
    }

    public void AddFinishedEvent(Action finish)
    {
        m_OnFinished += finish;
    }

    public void SetCurNode(Node curNode)
    {
        m_Graph.SetCurNode(curNode);
    }

    public void SetNextNode()
    {
        var curNode = m_Graph.GetCurNode();
        m_Graph.SetCurNode(curNode.FindNextNode(GlobalVariable.NODE_EXIT_FIELD));
    }

    public void RegisterDynamicOptions(IEnumerable<DialogueNodeOption> options)
    {
        m_DynamicOptions.Clear();
        m_DynamicOptions.AddRange(options);
    }

    public List<DialogueNodeOption> ConsumeDynamicOptions()
    {
        var options = new List<DialogueNodeOption>(m_DynamicOptions);
        m_DynamicOptions.Clear();
        return options;
    }

    public async UniTask StartDialogue(Actor targetActor)
    {
        UIManager.Instance.PageSystem.HideUI(true);
        m_IsStart = true;
        m_CurrentActor = targetActor;
        SetGraph(targetActor.GetDialogue());
        await UniTask.Delay(TimeSpan.FromSeconds(DELAY_SHOW_PANEL));
        ShowPanel();
        SetSkipHandler(QuitDialogue);
        m_SkipButton.gameObject.SetActive(m_Graph.AllowSkip);
        ContinueDialogue();
    }

    private void ShowPanel()
    {
        m_DialogueUI.SetCallback(ContinueDialogue);
        m_DialogueUI.gameObject.SetActive(true);
    }

    public void SetActorSubText()
    {
        UI.SetSubText(m_CurrentActor.Data.Name);
    }

    public void SetPlayerSubText()
    {
        UI.SetSubText("");
    }

    public void MovePanelToWorldPos(Vector3 worldPos)
    {
        var screenPos = Camera.main.WorldToScreenPoint(worldPos);
        m_DialogueUI.SetPos(screenPos);
    }

    public void ContinueDialogue()
    {
        if (!m_IsStart)
        {
            HDebug.LogError("Dialogue not started");
            return;
        }

        if (!m_DialogueUI.IsTyping)
        {
            var curNode = m_Graph.GetCurNode();
            m_DialogueUI.SetActiveDialogueOption(false);
            m_DialogueUI.SetTypingSpeed(m_DefaultTypingSpeed);
            m_NodeExecutor.Execute(curNode as DialogueNode);
        }
        else
        {
            m_DialogueUI.SkipTyping();
        }
    }

    public void QuitDialogue()
    {
        m_DynamicOptions.Clear();
        UIManager.Instance.PageSystem.HideUI(false);
        m_OnFinished?.Invoke();
        m_OnFinished = null;
        GameEventReporter.Report(EGameEventType.TalkNpc, m_CurrentActor.Data.ID);

        ContentsManager.Instance.Get<TownContents>().Send(new TownContentsPararm(TownContentsPararm.EType.FinishDialogue));
        m_IsStart = false;
        m_CurrentActor = null;
        m_DialogueUI.gameObject.SetActive(false);
        SetSkipHandler(null);
        m_SkipButton.gameObject.SetActive(false);
    }

    #region TimeLine
    public void PlayBubbleLine(string localizeKey, float typingSpeed, Vector3 worldPos, BubblePanel.EDirectionType direction, Action onComplete)
    {
        m_DialogueUI.gameObject.SetActive(true);
        m_DialogueUI.SetDirection(direction);
        var screenPos = Camera.main.WorldToScreenPoint(worldPos);
        m_DialogueUI.SetPos(screenPos);

        PlayLine(m_DialogueUI, localizeKey, typingSpeed, () =>
        {
            m_DialogueUI.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    public void PlayNarrativeLine(string localizeKey, float typingSpeed, bool keepVisibleAfter, Action onComplete)
    {
        m_NarrativeUI.gameObject.SetActive(true);

        PlayLine(m_NarrativeUI, localizeKey, typingSpeed, () =>
        {
            if (!keepVisibleAfter)
                m_NarrativeUI.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    private void PlayLine(DialoguePanel panel, string localizeKey, float typingSpeed, Action onComplete)
    {
        panel.SetTypingSpeed(typingSpeed);
        string raw = Localize.Get(localizeKey);
        string text = TextTokenResolver.Resolve(raw);

        panel.TypeTextAsync(text).ContinueWith(() =>
        {
            panel.ReadyNextDialogue(true);
            panel.SetCallback(() =>
            {
                panel.ReadyNextDialogue(false);
                panel.SetText("");
                onComplete?.Invoke();
            });
        }).Forget();
    }

    public UniTask ShowLetterBox()
    {
        return m_LetterBox.Show();
    }

    public UniTask HideLetterBox()
    {
        return m_LetterBox.Hide();
    }

    public void SetLetterBox(bool visible)
    {
        m_LetterBox.SetVisible(visible);
    }

    public void RegisterTimelineSkip(Action onSkip)
    {
        SetSkipHandler(() =>
        {
            m_DialogueUI.SkipTyping();
            m_NarrativeUI.SkipTyping();
            m_DialogueUI.gameObject.SetActive(false);
            m_NarrativeUI.gameObject.SetActive(false);
            onSkip();
        });
        m_SkipButton.gameObject.SetActive(true);
    }

    public void UnregisterTimelineSkip()
    {
        SetSkipHandler(null);
        m_SkipButton.gameObject.SetActive(false);
    }
    #endregion

}
