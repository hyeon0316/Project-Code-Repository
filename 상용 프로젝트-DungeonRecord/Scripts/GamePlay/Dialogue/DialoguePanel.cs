using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialoguePanel : MonoBehaviour
{
    public bool IsTyping { get; private set; }

    [SerializeField] private Text m_Text;
    [SerializeField] private Text m_SubText;
    [SerializeField] private GameObject m_NextArrowObj;
    [SerializeField] private Button m_NextButton;
    [SerializeField] private DialogueOptionParent m_OptionParent;

    private float m_CurTypingSpeed;
    private Action m_ProgressCallback;
    private CancellationTokenSource m_TypingCts;


    public void SetTypingSpeed(float speed)
    {
        m_CurTypingSpeed = speed;
    }

    public void SetText(string text)
    {
        m_Text.text = text;
    }

    public void SetSubText(string text)
    {
        m_SubText.text = text;
    }

    public async UniTask TypeTextAsync(string text)
    {
        m_NextArrowObj.SetActive(false);
        IsTyping = true;

        m_TypingCts?.Dispose();
        m_TypingCts = new CancellationTokenSource();
        var token = m_TypingCts.Token;

        var sb = new StringBuilder();
        SetText("");
        foreach (var letter in text.ToCharArray())
        {
            if (token.IsCancellationRequested)
                break;

            sb.Append(letter);
            SetText(sb.ToString());
            await UniTask.Delay(TimeSpan.FromSeconds(m_CurTypingSpeed), cancellationToken: token).SuppressCancellationThrow();
        }

        if (token.IsCancellationRequested)
            SetText(text);

        m_NextArrowObj.SetActive(true);
        IsTyping = false;
    }

    public void SkipTyping()
    {
        if (!IsTyping)
            return;

        m_TypingCts?.Cancel();
    }

    public void ReadyNextDialogue(bool isActive)
    {
        m_NextArrowObj.SetActive(isActive);
        SetInteractableNextButton(isActive);
    }

    public void SetInteractableNextButton(bool isActive)
    {
        m_NextButton.interactable = isActive;
    }

    public void SetCallback(Action callback)
    {
        m_ProgressCallback = callback;
    }

    public void SetActiveDialogueOption(bool isActive)
    {
        m_OptionParent.gameObject.SetActive(isActive);
    }

    public void SetOption(List<DialogueNodeOption> options)
    {
        SetActiveDialogueOption(true);
        m_OptionParent.Set(options);
    }

    public void OnClickEvent_Progress()
    {
        m_ProgressCallback?.Invoke();
    }
}
