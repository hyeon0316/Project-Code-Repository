using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingChest : Interaction
{
   private FadeImage _fade;
   protected override void Awake()
   {
      base.Awake();
      FindObjectOfType<EndingChest>().GetComponent<BoxCollider>().enabled = false;
      _fade = GameObject.Find("Canvas").transform.Find("FadeImage").GetComponent<FadeImage>();
   }

   private void Update()
   {
      StartInteract();
   }

   public override void StartInteract()
   {

      if (GameObject.Find("SummonEnemysTr").transform.childCount == 0 &&
          FindObjectOfType<GameManager>().EnemyPos[3].transform.childCount == 0)
      {
         FindObjectOfType<EndingChest>().GetComponent<BoxCollider>().enabled = true;

         if (CanInteract)
         {
            ActionBtn.transform.position = this.transform.position + new Vector3(0, 2f, 0);
            if (Input.GetKeyDown(KeyCode.Space))
            {
               FindObjectOfType<Inventory>().DeleteMaterial();
               _fade.FadeIn();
               this.GetComponent<Animator>().SetTrigger("Open");
               CanInteract = false;
               FindObjectOfType<SoundManager>().Play("EndingBGM", SoundType.Bgm);
               Invoke("GoEnding", 1f);
            }
         }
      }
   }

   private void GoEnding()
   {
      SceneManager.LoadScene("Ending");
   }
}
