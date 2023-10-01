using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Information : MonoBehaviour
{   
    public static bool informationActivated = false;
    public static bool slotClear = false;
    [SerializeField]
    private GameObject go_InformationBase;
    [SerializeField]
    private GameObject go_EslotParent; //슬롯의 부모객체

    public E_Slot[] e_Slots;
    private Inventory theInventory;
  
    // Start is called before the first frame update
    void Start()
    {
        theInventory = FindObjectOfType<Inventory>();
        e_Slots = go_EslotParent.GetComponentsInChildren<E_Slot>();       
    }

    // Update is called once per frame
    void Update()
    {
        TryInformation();
        if (!InputNumber.activated)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Invoke("Exit", 0.01f);
            }
        }
    }

    private void TryInformation()//장비창 On/Off
    {
        if (!SystemBase.gamePaused)
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                informationActivated = !informationActivated;
            }
        }
        if (informationActivated)
            OpenInformation();
        else
            CloseInformation();
    }

    private void OpenInformation()
    {
        go_InformationBase.SetActive(true);
    }

    private void CloseInformation()
    {
        go_InformationBase.SetActive(false);
    }

    public void EquipItem(Item _item)
    {
        SoundManager.inst.SFXPlay("EquipItem", SoundManager.inst.uiList[0]);
        for (int i = 0; i < e_Slots.Length; i++)
        {
            if (e_Slots[i].CompareTag(_item.EquipType))//부위별 방어구가 정해진 자리에 장착되기 위함
            {
                Item _tempItem = e_Slots[i].e_item;
                e_Slots[i].AddEquipItem(_item);

                if (_tempItem != null)//장비를 장착할때 장비슬롯에 이미 같은 부위의 장비가 있을 때 
                {
                    theInventory.AcquireItem(_tempItem);//이미 장착된 장비는 인벤토리로 이동(교체)
                    Player.inst.TakeOffEffect(_tempItem);//장착되어 있었던 장비의 효과를 해제
                }
                else
                    slotClear = true;//인벤슬롯에서 장착후 장비슬롯으로 넘어가기에 인벤슬롯을 초기화

                return;
            }        
        }
    }

    public void Exit()
    {
        informationActivated = false;
    }
}
