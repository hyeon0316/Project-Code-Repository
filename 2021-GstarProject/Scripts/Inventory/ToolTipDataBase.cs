using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTipDataBase : MonoBehaviour
{
    [SerializeField]
    private SlotToolTip theSlotToolTip;

    public void ShowToolTip(Item _item, Vector3 _pos)
    {
        theSlotToolTip.ShowToolTip(_item, _pos);
    }

    // 📜SlotToolTip 👉 📜Slot 징검다리
    public void HideToolTip()
    {
        theSlotToolTip.HideToolTip();
    }
}
