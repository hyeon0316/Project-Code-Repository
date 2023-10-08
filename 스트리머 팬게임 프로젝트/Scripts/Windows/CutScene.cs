using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class CutScene : MonoBehaviour
{
    [SerializeField] private Transform[] _spawnPos;

    private void Start()
    {
        SetPickCharacter();
    }

    /// <summary>
    /// 플레이어들이 선택한 캐릭터들을 셋팅
    /// </summary>
    private void SetPickCharacter()
    {
        for (int i = 0; i < Global.MaxRoomPlayer; i++)
        {
            Player player = PlayerManager.Instance.Players[i];
            GameObject obj = Instantiate(DataManager.Instance.CharacterPrefabs[(int)player.Type], _spawnPos[i]);
            CharacterController characterController = obj.GetComponent<CharacterController>();
            characterController.SetNickName(player.NickName);
            
            if(i >= 4)
                characterController.ReverseImage();
        }
    }
}
