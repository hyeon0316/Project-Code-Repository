using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class EndingChest : Interaction
{

    private void Awake()
    {
        this.GetComponent<BoxCollider>().enabled = false;
        GameEvent.EnableEndingChestEvent += EnableChest;
    }

    public override void StartInteract()
    {
        SoundManager.Instance.Play("EndingBGM", SoundType.Bgm);
        PlayerManager.Instance.Inventory.ClearMaterial();
        this.GetComponent<Animator>().SetTrigger("Open");
        FadeManager.Instance.FadeInAsync().ContinueWith(GoEnding).Forget();
    }

    private void GoEnding()
    {
        GameEvent.EnableEndingChestEvent -= EnableChest;
        SceneManager.LoadScene("Ending");
        Destroy(PlayerManager.Instance.gameObject);
    }

    private void EnableChest()
    {
        this.GetComponent<BoxCollider>().enabled = true;
    }

}
