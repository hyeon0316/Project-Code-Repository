using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SlotToolTip : MonoBehaviour
{
    [SerializeField]
    private GameObject go_Base;
    [SerializeField]
    private Text txt_ItemName;
    [SerializeField]
    private Text txt_ItemDesc;
    [SerializeField]
    private Image itemImage;
    [SerializeField]
    private Text txt_ItemRank;
    [SerializeField]
    private Text txt_ItemType;
    [SerializeField]
    private Text txt_ItemIntro;

    public void ShowToolTip(Item _item, Vector3 _pos)
    {
        go_Base.SetActive(true);
        _pos += new Vector3(go_Base.GetComponent<RectTransform>().rect.width * 0.5f,
                            -go_Base.GetComponent<RectTransform>().rect.height * 0.5f,
                            0);
        go_Base.transform.position = _pos;

        txt_ItemName.text = _item.itemName;
        txt_ItemDesc.text = _item.itemDesc;
        itemImage.sprite = _item.itemImage;
        txt_ItemRank.text = _item.itemRank;
        txt_ItemType.text = _item.itemTypeName;
        txt_ItemIntro.text = _item.itemIntro;
    }

    public void HideToolTip()
    {
        go_Base.SetActive(false);
    }
}
