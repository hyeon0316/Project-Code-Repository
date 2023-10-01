using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class NpcTalk : Interaction
{
    public string[] Sentences;
    private DialogueManager _dialogueManager;

    protected override void Awake()
    {
        base.Awake();
        _dialogueManager = GameObject.Find("Canvas").GetComponent<DialogueManager>();
    }

    private void Update()
    {
        StartInteract();
    }

    public override void StartInteract()
    {
        if (CanInteract)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                FindObjectOfType<Player>().GetComponent<Rigidbody>().velocity = Vector3.zero;
                LookPlayer();
                if (SceneManager.GetActiveScene().name.Equals("Dungeon"))
                {
                    GameObject.Find("PlayerUICanvas").SetActive(false);
                }
                else if (SceneManager.GetActiveScene().name.Equals("Town"))
                {
                    if (this.transform.Find("QuestionMark").gameObject.activeSelf)
                    {
                        this.transform.Find("QuestionMark").gameObject.SetActive(false);
                    }
                }

                _dialogueManager.Npc = this.GetComponent<NpcTalk>();
                _dialogueManager.TalkPanel.transform.position = this.transform.position + new Vector3(0.8f, 1.2f, 0.5f);

                FindObjectOfType<Player>().IsStop = true;
                FindObjectOfType<Player>().PlayerAnim.SetBool("IsRun", false);
                FindObjectOfType<CameraManager>().Target = this.gameObject;
                ActionBtn.SetActive(false);
                _dialogueManager.TalkStart();
                _dialogueManager.OnDialogue(Sentences);

                CanInteract = false;
            }
        }
    }

    private void LookPlayer()
    {
        if (transform.position.x > GameObject.Find("Player").transform.position.x)
        {
            GetComponentInChildren<SpriteRenderer>().flipX = true;
            FindObjectOfType<Player>().ChangeDirection();
        }
        else
        {
            GetComponentInChildren<SpriteRenderer>().flipX = false;
            FindObjectOfType<Player>().ChangeDirection(false);
        }
    }
}

   

