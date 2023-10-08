using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombinationSlot : MonoBehaviour, IPointerClickHandler, ISetAlpha
{
   /// <summary>
   /// 재조합 슬롯에 셋팅이 되어있는지
   /// </summary>
   public bool IsSetting { get; private set; }
   
   /// <summary>
   /// 재조합 될 아이템의 인덱스
   /// </summary>
   public byte ItemIndex { get;  private set; } = Byte.MaxValue;

   [SerializeField] private Image _itemImage;
   

   /// <summary>
   /// 재조합 슬롯의 아이템 설정
   /// </summary>
   public void SetSlotItem(byte itemIndex, Sprite itemSprite)
   {
      WindowManager.Instance.GetInGame().ReadyWindow.ReCombination.CheckDuplicationIndex(itemIndex); //추가 전 중복인덱스의 아이템이 있는지 체크하여 제거
      
      IsSetting = true;
      ItemIndex = itemIndex;
      SetAlpha(1);
      _itemImage.sprite = itemSprite;
   }

   /// <summary>
   /// 재조합 슬롯에서 아이템을 제거한다.
   /// </summary>
   public void DeleteSlotItem()
   {
      if (IsSetting)
      {
         IsSetting = false;
         ItemIndex = Byte.MaxValue;
         SetAlpha(0);
      }
   }
   

   public void OnPointerClick(PointerEventData eventData)
   {
      if (eventData.button == PointerEventData.InputButton.Right) //우클릭으로 아이템 해제
      {
         DeleteSlotItem();
      }
   }

   public void SetAlpha(float value)
   {
      Color color = _itemImage.color;
      color.a = value;
      _itemImage.color = color;
      
      WindowManager.Instance.GetInGame().ReadyWindow.ReCombination.CheckSlot(); //장착 or 해제 될때마다 재조합 가능 여부 확인
   }
}
