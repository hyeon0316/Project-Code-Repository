using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interaction : MonoBehaviour
{
   public bool CanInteract;
   public GameObject ActionBtn;

   protected virtual void Awake()
   {
      ActionBtn = GameObject.Find("UICanvas").transform.Find("ActionBtn").gameObject;
   }

   /// <summary>
   /// 상호작용을 시작하는 함수
   /// </summary>
   public abstract void StartInteract();

   private void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("Player"))
      {
         Debug.Log("상호작용 가능");
         ActionBtn.transform.position = this.transform.position + new Vector3(0f, 1f, 0.5f);
         CanInteract = true;
         ActionBtn.SetActive(true);
      }
   }

   private void OnTriggerExit(Collider other)
   {
      if (other.CompareTag("Player"))
      {
         Debug.Log("상호작용 불가능");
         CanInteract = false;
         ActionBtn.SetActive(false);
      }
   }
}
