using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneMove : Interaction
{
    [SerializeField] private string _sceneName = "Dungeon";

    public override void StartInteract()
    {
        PlayerManager.Instance.Player.StopMove();
        FadeManager.Instance.FadeInAsync().ContinueWith(MoveScene).Forget();
    }

    private void MoveScene()
    {
        SceneManager.LoadScene(_sceneName);
        SoundManager.Instance.Play("DungeonBGM", SoundType.Bgm);
        PlayerManager.Instance.Player.ReMove();
    }
}
