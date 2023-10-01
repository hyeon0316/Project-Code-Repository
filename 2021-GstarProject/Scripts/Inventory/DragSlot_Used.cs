using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragSlot_Used : MonoBehaviour
{
    static public DragSlot_Used instance; //(static)게임 내내 메모리에 자기자신 유지

    public Use_Slot dragSlot_Used;

    //아이템 이미지
    [SerializeField]
    private Image u_imageItem;

    void Start()
    {
        instance = this;
    }
    public void DragSetImage(Image u_itemImage) //드래그슬롯에 이미지를 할당해서 복사본만듬
    {
        u_imageItem.sprite = u_itemImage.sprite;
        SetColor(1);
    }

    public void SetColor(float _alpha)
    {
        Color color = u_imageItem.color;
        color.a = _alpha;
        u_imageItem.color = color;
    }
}
