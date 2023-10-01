using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class Prison : Interaction
{
    private FadeImage _fade;
    private bool _isPrison;
    protected override void Awake()
    {
        _fade = FindObjectOfType<FadeImage>();
        base.Awake();
    }
    private void Update()
    {
        if (GameObject.Find("EnemyPos_Second").transform.childCount == 0)
        {
            this.GetComponent<BoxCollider>().enabled = true;
        }

        StartInteract();
    }

    public override void StartInteract()
    {
        if (CanInteract)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                FindObjectOfType<Player>().PlayerAnim.SetBool("IsRun", false);
                FindObjectOfType<SoundManager>().Play("Object/PrisonOpen",SoundType.Effect);
                _fade.FadeIn();
                FindObjectOfType<Player>().IsStop = true;
                FindObjectOfType<Inventory>().DeleteMaterial();
                ActionBtn.SetActive(false);
                CanInteract = false;
                _isPrison = true;
            }
        }
        if (_fade.IsFade && _isPrison)
        {
            FindObjectOfType<NpcTalk>().transform.position = GameObject.Find("EscapePos").transform.position;
                
            _fade.FadeOut();
            FindObjectOfType<Player>().IsStop = false;
            GameObject.Find("JumpMapUnlock").SetActive(false);
            GameObject.Find("PrisonDoorClose").SetActive(false);
            GameObject.Find("Prison").transform.Find("PrisonDoorOpen").gameObject.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }
}
