using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "New Item/item")]
public class Item : ScriptableObject 
{
    [TextArea]
    public string itemDesc;//아이템 효과 설명
    [TextArea]
    public string itemIntro;//아이템 스토리 설명
    public string itemRank; //아이템의 등급
    public string itemName; //아이템의 이름
    public Sprite itemImage; //아이템의 이미지
    public GameObject itemPrefab; //아이템의 프리팹

    public string EquipType; //무기 유형
    public string itemTypeName;//한글로 표기할 아이템 타입

    public enum ItemType
    {
        Equipment, //장비
        Used, //소모품
        Ingredient, //재료
    }

    public float itemHp;//포션회복량
    public float itemMp;//포션회복량
    public float startingHp;//총 체력량
    public float startingMp;//총 마나량
    public float itemPower;//파워
    public float itemDp;//방어력
    public ItemType itemType;

}
