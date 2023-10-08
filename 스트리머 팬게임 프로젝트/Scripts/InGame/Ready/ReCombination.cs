using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ReCombination : MonoBehaviour
{
  [SerializeField] private GameObject _combinationButton;

  /// <summary>
  /// 재조합 슬롯
  /// </summary>
  public CombinationSlot[] ItemSlots;

  
  /// <summary>
  /// 3개의 슬롯 모두 아이템이 셋팅되어 있는지 체크
  /// </summary>
  public void CheckSlot()
  {
    int checkCount = 0;

    foreach (var slot in ItemSlots)
    {
      if (slot.IsSetting)
        checkCount++;
    }

    _combinationButton.SetActive(checkCount == ItemSlots.Length);
  }

  
  /// <summary>
  /// 해당 인덱스가 재조합 슬롯에 있는지 확인되면 그 아이템 제거
  /// </summary>
  public void CheckDuplicationIndex(byte checkIndex)
  {
    foreach (var slot in ItemSlots)
    {
      if (checkIndex == slot.ItemIndex)
      {
        slot.DeleteSlotItem();
        break;
      }
    }
  }

  /// <summary>
  /// 재조합 슬롯의 아이템 전부 제거
  /// </summary>
  public void ClearSlot()
  {
    foreach (var slot in ItemSlots)
    {
      slot.DeleteSlotItem();
    }
  }


  /// <summary>
  /// 준비된 3개의 아이템을 재조합(버튼 이벤트)
  /// </summary>
  public void CombinationItems()
  {
    SoundManager.Instance.PlayEffect(EffectType.ReCombination);
    NetworkManager.Instance.SendRequestCombinationItemPacket(PlayerManager.Instance.Players[0].ID,
      ItemSlots[0].ItemIndex,
      ItemSlots[1].ItemIndex,
      ItemSlots[2].ItemIndex);

    ClearSlot();
  }

    public void OnMouseEnter()
    {
        ToolTipManager.Instance.ShowCombinationToolTip();
    }

    public void OnMouseExit()
    {
        ToolTipManager.Instance.CloseCombination();
    }
}
