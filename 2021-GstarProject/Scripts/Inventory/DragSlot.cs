using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragSlot : MonoBehaviour //이미지가 슬롯보다 아래에서 겹치는것을 방지
{
    static public DragSlot instance; //(static)게임 내내 메모리에 자기자신 유지

    public Slot dragSlot;

    //아이템 이미지
    [SerializeField]
    private Image imageItem;


    void Start()
    {
        instance = this;
    }
    public void DragSetImage(Image _itemImage) //드래그슬롯에 이미지를 할당해서 복사본만듬
    {
        imageItem.sprite = _itemImage.sprite;
        SetColor(1);
    }

    public void SetColor(float _alpha)
    {
        Color color = imageItem.color;
        color.a = _alpha;
        imageItem.color = color;
    }
   
}
