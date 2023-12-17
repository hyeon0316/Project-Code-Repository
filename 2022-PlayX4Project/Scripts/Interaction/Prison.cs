using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Prison : Interaction
{
    [SerializeField] private GameObject _closeDoorObj;
    [SerializeField] private GameObject _openDoorObj;
    [SerializeField] private GameObject _jumpMapLockObj;
    [SerializeField] private Transform _npcEscapePos;

    public override void StartInteract()
    {
        PlayerManager.Instance.Player.StopMove();
        SoundManager.Instance.Play("Object/PrisonOpen", SoundType.Effect);
        FadeManager.Instance.FadeInAsync().ContinueWith(OpenPrison).Forget();
        PlayerManager.Instance.Inventory.ClearMaterial();
    }

    private void OpenPrison()
    {
        DialogueManager.Instance.CurNpc.transform.position = _npcEscapePos.position;
        FadeManager.Instance.FadeOut();
        PlayerManager.Instance.Player.ReMove();
        _jumpMapLockObj.SetActive(false);
        _closeDoorObj.SetActive(false);
        _openDoorObj.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void ActivePrison()
    {
        this.GetComponent<BoxCollider>().enabled = true;
    }

    public void GetPrisonKey()
    {
        PlayerManager.Instance.Inventory.AddMaterial("PrisonKey");
    }
}
