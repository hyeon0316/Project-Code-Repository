using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public sealed class Npc : Interaction
{
    [SerializeField] private List<string> _sentences;
    [SerializeField] private SpriteRenderer _renderer;

    public override void StartInteract()
    {
        StartTalk();
        HandleTownSceneLogic();
    }

    private void LookPlayer()
    {
        if (this.transform.position.x >PlayerManager.Instance.Player.transform.position.x)
        {
            _renderer.flipX = true;
           PlayerManager.Instance.Player.ChangeDirection(true);
        }
        else
        {
            _renderer.flipX = false;
           PlayerManager.Instance.Player.ChangeDirection(false);
        }
    }

    public void RemoveDataUntilTaget(string target)
    {
        if(_sentences.Count == 0)
        {
            return;
        }

        int index = _sentences.FindIndex(s => s.Equals(target));
        if (index != -1)
        {
            _sentences.RemoveRange(0, index + 1);
        }
    }

    private void StartTalk()
    {
        LookPlayer();
        PlayerManager.Instance.SetActivePlayerUI(false);
        PlayerManager.Instance.Player.StopMove();
        CameraManager.Instance.SetTarget(this.gameObject);
        DialogueManager.Instance.CurNpc = this;
        DialogueManager.Instance.ActiveLetterBox();
        DialogueManager.Instance.OnDialogue(_sentences);
        DialogueManager.Instance.SetPanelPos(this.transform.position + new Vector3(-0.5f, 1.2f, 0.0f));
    }

    private void HandleTownSceneLogic()
    {
        if (SceneManager.GetActiveScene().name.Equals("Town"))
        {
            if (this.transform.Find("QuestionMark").gameObject.activeSelf)
            {
                this.transform.Find("QuestionMark").gameObject.SetActive(false);
            }
        }
    }

    public void ReTalk()
    {
        _canInteract = true;
        _actionButtonObj.SetActive(true);
    }
}

   

