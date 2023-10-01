using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragSlot_Equip : MonoBehaviour
{
    static public DragSlot_Equip instance; //(static)게임 내내 메모리에 자기자신 유지

    public E_Slot dragSlot_Equip;

    //아이템 이미지
    [SerializeField]
    private Image e_imageItem;


    void Start()
    {
        instance = this;
    }
    public void DragSetImage(Image e_itemImage) //드래그슬롯에 이미지를 할당해서 복사본만듬
    {
        e_imageItem.sprite = e_itemImage.sprite;
        SetColor(1);
    }

    public void SetColor(float _alpha)
    {
        Color color = e_imageItem.color;
        color.a = _alpha;
        e_imageItem.color = color;
    }

}
