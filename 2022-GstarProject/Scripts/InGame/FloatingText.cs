using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
   /// <summary>
   /// 한번에 투명화 되면서 사라질 오브젝트 모임
   /// </summary>
   private List<GameObject> _clearTexts = new List<GameObject>(); 

   private float _clearTimer;
   private bool _isTimer;

   private float _moveIndex;
   

   /// <summary>
   /// 텍스트가 사라지기 까지 시간
   /// </summary>
   [SerializeField] private float _clearTime;

  
   public void CreateFloatingText(string damage)
   {
      GameObject floating = ObjectPoolManager.Instance.GetObject(PoolType.DamageText);
      _clearTexts.Add(floating);
      floating.transform.position = transform.position;
      floating.GetComponent<DamageText>().SetDamageText(damage, transform);
      _clearTimer = _clearTime;
      _isTimer = true;

      MoveUpText();
   }

   /// <summary>
   /// 텍스트가 출력될때 마다 차례대로 위로 올림
   /// </summary>
   private void MoveUpText()
   {
      _moveIndex = 0;
      for (int i = _clearTexts.Count - 1; i >= 0; i--)
      {
         _moveIndex += 0.4f;
         _clearTexts[i].GetComponent<DamageText>().transform.DOMove(transform.position + new Vector3(0, _moveIndex,0), 0.5f);
      }
   }

   private void Update()
   {
      SetTimer();
      FadeFromDistance();
   }
   
   /// <summary>
   /// 텍스트 출력 위치에 따른 페이드 조절
   /// </summary>
   private void FadeFromDistance()
   {
      if (_clearTexts.Count != 0)
      {
         for(int i =0; i<_clearTexts.Count; i++)
         {
            DamageText text = _clearTexts[i].GetComponent<DamageText>();
            text.Text.alpha = Vector3.Distance(text.transform.position, transform.position + new Vector3(0,2,0));
            if (text.Text.alpha <= 0.1f)
            {
               _clearTexts.Remove(_clearTexts[i]);
               text.DisableText();
            }
         }
      }
   }

   /// <summary>
   /// 일정시간 이상으로 딜링이 멈췄을때 출력됐던 모든 텍스트를 지워줌
   /// </summary>
   private void SetTimer()
   {
      if (_isTimer)
      {
         _clearTimer -= Time.deltaTime;
         if (_clearTimer <= 0)
         {
            ClearText();
            _isTimer = false;
         }
      }
   }
   

   /// <summary>
   /// 페이드아웃과 함께 텍스트를 제거
   /// </summary>
   public void ClearText()
   {
      foreach (var text in _clearTexts)
      {
         if (Time.timeScale != 0)
         {
            text.GetComponent<DamageText>().FadeOutText();
         }
      }
      _clearTexts.Clear();
   }
   
}
