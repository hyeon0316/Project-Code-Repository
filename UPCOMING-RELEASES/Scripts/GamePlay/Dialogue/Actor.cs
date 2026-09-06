using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public abstract class Actor : MonoBehaviour, TownContents.IInteractable
{
    public ActorSO Data => m_Data;

    [SerializeField] protected string m_ActorID;
    [SerializeField] protected Renderer m_Renderer;

    protected ActorSO m_Data;
    protected ActorHUD m_HUD;
    protected Animator m_Animator;

    protected virtual void Awake()
    {
        m_Animator = this.GetComponent<Animator>();
        if (!string.IsNullOrEmpty(m_ActorID))
            Init(m_ActorID);
    }

    public void Init(string actorID)
    {
        m_ActorID = actorID;
        m_Data = SODatabase.GetActor(actorID);
        if (m_Data.Animator != null)
        {
            m_Animator.runtimeAnimatorController = m_Data.Animator;
        }
    }

    public void SetAppearance(Sprite sprite, RuntimeAnimatorController animator)
    {
        if (m_Renderer is SpriteRenderer spriteRenderer)
            spriteRenderer.sprite = sprite;
        m_Animator.runtimeAnimatorController = animator;
    }

    private void OnEnable()
    {
        ContentsManager.Instance.Get<QuestContents>().OnQuestStateChanged += UpdateQuestIcon;
    }

    protected virtual void Start()
    {
        m_HUD = AddressableBundleManager.Instance
            .AssetInstantiate<ActorHUD>(ActorHUD.AssetID, UIManager.Instance.HudLayer);
        m_HUD.Init(m_Data.Name, GetOverHeadPos);
        UpdateQuestIcon();
    }

    private void OnDisable()
    {
        ContentsManager.Instance.Get<QuestContents>().OnQuestStateChanged -= UpdateQuestIcon;
    }

    private void OnDestroy()
    {
        if (m_HUD != null)
        {
            Destroy(m_HUD.gameObject);
            m_HUD = null;
        }
    }

    public Vector3 GetOverHeadPos()
    {
        return CharacterPositionUtil.GetOverHeadPos(m_Renderer);
    }

    public virtual void SetDialogue()
    {
        m_HUD.SetHidden(true);
        ContentsManager.Instance.Get<TownContents>().
            Send(new TownContentsPararm(TownContentsPararm.EType.StartDialogue, this.transform));
        CheckCanStartQuest();
    }

    public virtual void ClearDialgoue()
    {
        CheckTalkStep();
        ContentsManager.Instance.Get<TownContents>().
            Send(new TownContentsPararm(TownContentsPararm.EType.FinishDialogue));
        m_HUD.SetHidden(false);
        UpdateQuestIcon();
    }

    private void UpdateQuestIcon()
    {
        var contents = ContentsManager.Instance.Get<QuestContents>();

        // CurTargetQuestID가 있으면 해당 quest를 우선으로 체크
        if (!string.IsNullOrEmpty(contents.CurTargetQuestID))
        {
            var steps = contents.GetCurProgressTalkSteps(m_Data.ID);
            foreach (var s in steps)
            {
                if (s.QuestID == contents.CurTargetQuestID)
                {
                    var quest = s.GetQuest();
                    m_HUD.UpdateQuestIcon(quest.Info.Type, quest.State);
                    return;
                }
            }
        }

        // InProgress > CanStart 순으로 체크 (Normal 제외)
        var inProgressSteps = contents.GetCurProgressTalkSteps(m_Data.ID);
        foreach (var s in inProgressSteps)
        {
            var quest = s.GetQuest();
            if (quest.Info.Type == EQuestType.Normal)
                continue;
            m_HUD.UpdateQuestIcon(quest.Info.Type, quest.State);
            return;
        }

        var canStart = contents.GetCanStartQuests(m_Data.ID);
        foreach (var q in canStart)
        {
            if (q.Info.Type == EQuestType.Normal)
                continue;
            m_HUD.UpdateQuestIcon(q.Info.Type, q.State);
            return;
        }

        m_HUD.HideQuestIcon();
    }

    public DialogueGraph GetDialogue()
    {
        var step = GetCurTalkStep();
        return step != null ? step.GetDialogue() : m_Data.DefaultDialogue;
    }

    private TalkActorStep GetCurTalkStep()
    {
        var contents = ContentsManager.Instance.Get<QuestContents>();
        var curSteps = contents.GetCurProgressTalkSteps(m_Data.ID);

        // CurTargetQuestID가 있으면 해당 quest step을 우선으로 체크
        if (!string.IsNullOrEmpty(contents.CurTargetQuestID))
        {
            foreach (var s in curSteps)
            {
                if (s.QuestID == contents.CurTargetQuestID)
                    return s as TalkActorStep;
            }
        }

        // InProgress step 중 첫 번째
        if (curSteps.Count != 0)
            return curSteps[0] as TalkActorStep;

        return null;
    }

    public void SetHighlight(bool highlight)
    {
        if (m_HUD != null)
            m_HUD.SetNameFocus(highlight);
    }

    private void CheckCanStartQuest()
    {
        var contents = ContentsManager.Instance.Get<QuestContents>();
        if (contents.GetCurProgressTalkSteps(m_Data.ID).Count != 0)
            return;

        var quests = ContentsManager.Instance.Get<QuestContents>().GetCanStartQuests(m_Data.ID);
        var dynamicOptions = new List<DialogueNodeOption>();
        foreach (var quest in quests)
        {
            var id = quest.Info.ID;
            var acceptDialogue = quest.Info.AcceptInfo.Dialogue;

            dynamicOptions.Add(new DialogueNodeOption(quest.Info.TitleName,
                () =>
                {
                    ContentsManager.Instance.Get<QuestContents>().StartQuest(id);
                    DialogueManager.Instance.SetGraph(acceptDialogue);
                    DialogueManager.Instance.ContinueDialogue();
                }));
        }
        DialogueManager.Instance.RegisterDynamicOptions(dynamicOptions);

    }

    private void CheckTalkStep()
    {
        var talkStep = GetCurTalkStep();
        if (talkStep != null)
            talkStep.CompleteStep();
    }


    public void Interact()
    {
        SetDialogue();
        DialogueManager.Instance.AddFinishedEvent(() => ClearDialgoue());
        DialogueManager.Instance.StartDialogue(this).Forget();
    }
}
