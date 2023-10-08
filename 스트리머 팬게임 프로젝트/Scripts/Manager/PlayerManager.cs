using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
   [SerializeField] private Transform _enemyParent;
   [SerializeField] private GameObject _enemyPrefab;
   public const int PLAYER_COUNT = 8;

   public Player[] Players = new Player[8];


   public Player GetPlayer(int networkID)
   {
      foreach(var player in Players)
      {
         if(player.ID == networkID)
            return player;
      }
      return null;
   }

   private void InitEnemy()
   {
      if (_enemyParent.childCount != 0)
      {
         for (int i = 0; i < 7; i++)
         {
            Destroy(_enemyParent.transform.GetChild(i).gameObject);
         }
      }
   }

   public void CreateEnemy(Packet.UserInfo[] userInfos)
   {
      InitEnemy();
      int index = 1;
        for (int i = 0; i < PLAYER_COUNT; i++)
        {
            if (userInfos[i].networkID == Players[0].ID)
            {
                continue;
            }

            GameObject enemy = Instantiate(_enemyPrefab, _enemyParent);
            enemy.transform.localPosition = new Vector3(0, 3000, 0);
            enemy.name = $"Enemy{userInfos[i].networkID}";
            Players[index] = enemy.GetComponent<Player>();
            Players[index].SetID((int)userInfos[i].networkID);
            Players[index].SetName(Encoding.Unicode.GetString(userInfos[i].name));
            Debug.Log(Players[index].NickName);
            ++index;
        }
   }
   

   /// <summary>
   /// 사망처리된 플레이어의 수를 반환
   /// </summary>
   /// <returns></returns>
   public int GetPlayerDeadCount()
   {
      int count = 0;
      foreach (var player in Players)
      {
         if (player.IsDead)
            count++;
      }

      return count;
   }

   /// <summary>
   /// 모든 플레이어의 준비동안 아바타 체력 갱신
   /// </summary>
   /// <param name="value"></param>
   public void PlayersUpdateAvatarHp(int value)
   {
      foreach (var player in Players)
      {
         player.RoundMaxAvatarHp = value;
      }
   }
}
