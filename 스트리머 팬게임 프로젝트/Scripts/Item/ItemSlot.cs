using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum EItemCode
{
    Minimum,
    OldWhip,
    OvaineGyeruik,
    HammerHeadMask,
    WakEnterNameplate,
    PruningAx,
    HongSam,
    Hamburger,
    BadGlasses,
    MaidCostume,
    NunDress,
    BigRibbon,
    FishBread,
    SisterHeart,
    GoodThings,
    DiaSword,
    DoctorTool,
    HairRoll,
    ClownGun,
    VocaloidStunGun,
    Drops,
    SecretWand,
    NobilitySword,
    USB,
    CrowBar,
    Denture,
    MagicianStaff,
    BartenderNecktie,
    OniKatana,
    CounselLicense,
    Broom,
    K2,
    ToothBrush,
    PorkCutlet,
    BugKeyCap,
    ScrapMetal,
    ExplorerBelt,
    HeroCloak,
    GraduateAnger,
    ArmyAnger,
    PoorAnger,
    Charge,
    Yes,
    No,
    PressMachine,
    KwonMinSummons,
    Maximun
}

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Image _image;
    

    /// <summary>
    /// 아이템 생성
    /// </summary>
    public void AddNewItem(EItemCode itemCode)
    {
        AddNewItem(itemCode, 0);
    }
    
    /// <summary>
    /// 업그레이드 수치까지 적용 하여 생성
    /// </summary>
    public void AddNewItem(EItemCode itemCode, byte upgrade)
    {
        Debug.Assert(itemCode > EItemCode.Minimum && itemCode < EItemCode.Maximun);

        GameObject obj = Instantiate(DataManager.Instance.SetItems[(int)itemCode - 1], transform);
        obj.transform.position = transform.position;
        Item newItem = obj.GetComponent<Item>();
        
        newItem.Code = itemCode; 
        newItem.Upgrade = upgrade;
        newItem.ApplyUpgrade();
        newItem.SetEquipEffectText();
        newItem.SetActiveStarObject();
    }

    public void ActiveItem(BattleCharacter player, BattleCharacter opponent)
    {
        if (this.transform.childCount == 1)
        {
            GetItem().Active(player, opponent);
        }
        else
        {
            //todo: 빈슬롯에 대한 이펙트 효과 적용(꽝 효과?)
        }
    }

    public Item GetItem()
    {
        return GetComponentInChildren<Item>();
    }

    /// <summary>
    /// UnUsing 인벤의 슬롯인지 Using 인벤의 슬롯인지 확인하여 반환
    /// </summary>
    /// <returns></returns>
    public int GetSlotIndex()
    {
        return  transform.GetSiblingIndex() + (transform.parent.parent.name.Equals("UnUsingInventory") ? 6 : 0);
    }

    /// <summary>
    /// 슬롯에 2개의 아이템이 들어가 있는 경우
    /// </summary>
    /// <returns></returns>
    public Item OtherInSlotItem()
    {
        if(transform.childCount == 2)
            return transform.GetChild(1).GetComponent<Item>();

        return transform.GetChild(0).GetComponent<Item>();
    }

    /// <summary>
    /// 특정 아이템 반환
    /// </summary>
    public T GetSpecificItem<T>()
    {
        return GetComponentInChildren<T>();
    }

    /// <summary>
    /// 슬롯에 아이템의 존재 유무 확인
    /// </summary>
    public bool IsExistItem()
    {
        return transform.childCount == 1;
    }

    /// <summary>
    /// todo: 임시, 아이템 사용 했다는것 표시
    /// </summary>
    public void SetAlpha(float value)
    {
        GetItem().SetAlpha(value);
    }

    public void SetAlphaSlot(float value)
    {
        Color color = _image.color;
        color.a = value;
        _image.color = color;
    }

    /// <summary>
    /// 아이템 슬롯의 알파값이 제로인지 확인
    /// </summary>
    /// <returns></returns>
    public bool IsAlphaZero()
    {
        Color color = _image.color;
        color.a = 0;
        
        return _image.color == color;
    }

    public void DeleteItem()
    {
        List<GameObject> deleteList = new List<GameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            deleteList.Add(transform.GetChild(i).gameObject);
        }
       
        while (deleteList.Count != 0)
        {
            Destroy(deleteList[0]);
            deleteList.RemoveAt(0);
        }
    }
}
