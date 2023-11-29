using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private Tutorial _tutorial;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            for (int i = 0; i < _tutorial.Keys.Length; i++)
            {
                _tutorial.Keys[i].SetActive(false);
            }

            if (this.gameObject.name.Equals("CBtnOn"))
            {
                _tutorial.Keys[(int)KeyIcons.Jump].SetActive(true);
            }
            else if (this.gameObject.name.Equals("ShiftBtnOn"))
            {
                _tutorial.Keys[(int)KeyIcons.Shift].SetActive(true);
            }
            else if (this.gameObject.name.Equals("WallJumpOn"))
            {
                _tutorial.ManualWindow.SetActive(true);
            }
            else if (this.gameObject.name.Equals("CombatOn"))
            {
                PlayerManager.Instance.SetActivePlayerUI(true);

                Vector3 playerPos =PlayerManager.Instance.Player.transform.position;
                Vector3 pos = new Vector3(playerPos.x + 2, -1.8f, playerPos.z);
                Instantiate(_tutorial.PracticeEnemy, pos, this.transform.rotation);
               PlayerManager.Instance.Player.Hp -=PlayerManager.Instance.Player.MaxHp / 2;
                PlayerManager.Instance.Inventory.AddUsed(_tutorial.DropItem, 1);
            }
            else if (this.gameObject.name.Equals("GameStart"))
            {
                FadeManager.Instance.FadeIn();
                Invoke(nameof(DelayLoadScene), 2f);
            }
            this.gameObject.SetActive(false);
        }
    }

   private void DelayLoadScene()
   {
      SoundManager.Instance.Play("TownBGM", SoundType.Bgm);
      SceneManager.LoadScene("Town");
   }
   
}
