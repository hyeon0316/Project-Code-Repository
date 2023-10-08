using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameController : MonoBehaviour
{
    private bool _isQuickEquip;

    private IEnumerator DelayQuickEquip(float delay)
    {
        float timer = delay;
        while (true)
        {
            if (timer <= 0)
            {
                _isQuickEquip = false;
                break;
            }

            timer -= Time.deltaTime;
            yield return null;
        }
    }

    private void Update()
   {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (InGame.CurGameType == EGameType.Ready)
            {
                Debug.Log($"Input.GetKeyDown(KeyCode.W) - _isQuickEquip : {_isQuickEquip}");
                if (!_isQuickEquip)
                {
                    _isQuickEquip = true;
                    byte _slotIndex1;
                    byte _slotIndex2;
                    Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Ray2D ray = new Ray2D(pos, Vector2.zero);
                    RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity,
                        LayerMask.GetMask("ItemSlot"));
                    StartCoroutine(DelayQuickEquip(0.1f));

                    Debug.Log($"Input.GetKeyDown(KeyCode.W) - hit.collider : {hit.collider}");
                    if (hit.collider != null)
                    {
                        ItemSlot slot = hit.collider.GetComponent<ItemSlot>();
                        Player player = PlayerManager.Instance.Players[0];
                        Debug.Log($"Input.GetKeyDown(KeyCode.W) - hit.collider != null : {hit.collider}");
                        if (slot.IsExistItem())
                        {
                            Debug.Log("아이템 퀵 장착");
                            _slotIndex1 = (byte) slot.GetSlotIndex();
                            if (_slotIndex1 < 6) //드래그하는 슬롯이 Using쪽일때
                            {
                                _slotIndex2 = (byte) player.UnUsingInventory.GetEmptySlot();
                                if (_slotIndex2 == Byte.MaxValue) //빈슬롯이 없었을 경우
                                    return;
                            }
                            else //드래그하는 슬롯이 UnUsing쪽일때
                            {
                                _slotIndex2 = (byte) player.UsingInventory.GetEmptySlot();

                                if (_slotIndex2 == Byte.MaxValue) //빈슬롯이 없었을 경우
                                    return;

                                if (player.UsingInventory.CheckDuplicationItem(player.UnUsingInventory
                                        .ItemSlots[_slotIndex1 - 6].GetItem().Code))
                                {
                                    Debug.Log($"드래그하는 슬롯이 UnUsing쪽일때 : UsingInventory.CheckDuplicationItem");
                                    return;
                                }
                            }

                            NetworkManager.Instance.SendChangeItemSlotPacket(player.ID, (byte) _slotIndex1,
                                (byte) _slotIndex2);
                            player.SwapItemNetwork((byte) _slotIndex1, (byte) _slotIndex2);
                            SoundManager.Instance.PlayEffect(EffectType.ItemDragEnd);
                            Debug.Log($"W 전송");
                        }
                    }
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (InGame.CurGameType == EGameType.Ready)
            {
                ToolTipManager.Instance.CloseToolTip();
            }
        }
   }
}
