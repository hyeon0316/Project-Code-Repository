using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMove : Interaction
{
    private string _sceneName = "Dungeon";

    private FadeImage _fade;

    protected override void Awake()
    {
        base.Awake();
        _fade = GameObject.Find("Canvas").transform.Find("FadeImage").GetComponent<FadeImage>();
    }

    private void Update()
    {
        StartInteract();
    }
    
    public override void StartInteract()
    {
        if (CanInteract)
        {
            if (ActionBtn.activeSelf)
            {
                ActionBtn.transform.position = this.transform.position + new Vector3(-0.2f, 1f, 0);
            }

            if (Input.GetKeyDown(KeyCode.Space) && !FindObjectOfType<Player>().IsStop)
            {
                FindObjectOfType<Player>().PlayerAnim.SetBool("IsRun", false);
                FindObjectOfType<Player>().IsStop = true;
                ActionBtn.SetActive(false);
                _fade.FadeIn();
            }
            
            if(_fade.FadeCount >= 1f)
            {
                SceneManager.LoadScene(_sceneName);
                FindObjectOfType<SoundManager>().Play("DungeonBGM", SoundType.Bgm);
                CanInteract = false;
            }
        }
    }


   
}
