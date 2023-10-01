using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ActiveSlotUI : MonoBehaviour
{
    public Image Icon;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI MainStatName;
    public TextMeshProUGUI MainStat;
    public TextMeshProUGUI SubStat;
    public TextMeshProUGUI EnforceStat;
    public TextMeshProUGUI EnforceNum;
    private ItemStat _itemStat;
    [SerializeField]
    private Button button; 
    public void ShowUI() => gameObject.SetActive(true);
    public void HideUI() => gameObject.SetActive(false);
    public bool PlayFade;

    // Start is called before the first frame update
    void Start()
    {
        PlayFade = false;

        HideUI();
    }
    public void SetStat(ItemStat _stat)
    {
        _itemStat = _stat;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ResetUI()
    {
        _itemStat = null;
        Icon.sprite = null;
        HideUI();
    }
    public void UpdateUI(Item _item)
    {
        
        if (_item is WeaponItem _weaponItem)
        {
            _itemStat = _weaponItem.EquipmentData.GetStat();
            SetMainStat(_itemStat.Attack, 2);
            SetSubStat();
        }
        else if (_item is EquipmentItem _uItem)
        {
            _itemStat = _uItem.EquipmentData.GetStat();
            SetMainStat(_itemStat.Defense, 1);
            SetSubStat();
        }
        else if(_item.Data is PortionItemData _use)
        {
            SetMainStat(_use.Value, 3);
            SubStat.text = _item.Data.Tooltip;
        }
        else 
        {
            SetMainStat(0, 4);
            SubStat.text = _item.Data.Tooltip;
        }
        Name.text = _item.Data.Name;
        SetIcon(_item.Data.IconSprite);
        
        ShowUI();
    }
    public void UpdateEnforceStat(ItemStat _stat,int _num)
    {
        StopCoroutine("Fade");
        SetFade();

        EnforceNum.text = "+" + _num.ToString();
        if (_num >= 15)
            EnforceNum.color = Color.red;
        else if (_num >= 10)
            EnforceNum.color = Color.blue;
        else
            EnforceNum.color = Color.yellow;
        EnforceStat.text = SetEnforceStr(_stat);
    }
    public void SetIcon(Sprite _image)
    {
        Icon.sprite = _image;
    }
    public void SetName(string _str)
    {
        Name.text = _str;
    }
    public void SetMainStat(int _stat,int _i)
    {
        string _str = "";
        string _mainStat = "";
        _mainStat = _stat.ToString();
        if (_i == 1)
        {
            _str = "����";
        }
        if (_i == 2)
        {
            _str = "���ݷ�";
        }
        if(_i == 3)
        {
            _str = "�Ҹ�ǰ";
            _mainStat = "HP ȸ�� :" + _stat.ToString();

        }
        if (_i == 4)
        {
            _str = "���";

            
        }
        MainStatName.text = _str;
        MainStat.text = _mainStat;
    }
    public void SetSubStat()
    {
        SubStat.text = Setstr(_itemStat);
    }
    public void SetEnforceStat(int _stat)
    {
        EnforceStat.text = _stat.ToString();
    }
    private string Setstr(ItemStat itemStat)
    {
        string _str = "";
        if (itemStat.HitPercent != 0)
            _str += "���� : " + itemStat.HitPercent + "\n";
        if (itemStat.SkillDamage != 0)
            _str += "��ų������ : " + itemStat.SkillDamage + "%\n";
        if (itemStat.AllDamge != 0)
            _str += "��絥���� : " + itemStat.AllDamge + "%\n";
        if (itemStat.Dodge != 0)
            _str += "ȸ�� : " + itemStat.Dodge + "\n";
        if (itemStat.ReduceDamage != 0)
            _str += "�޴� ��� ������ ���� : " + itemStat.ReduceDamage + "%\n";
        if (itemStat.MaxHp != 0)
            _str += "�ִ� ä�� : " + itemStat.MaxHp + "\n";
        if (itemStat.MaxMp != 0)
            _str += "�ִ� ���� : " + itemStat.MaxMp + "\n";
        if (itemStat.MaxPostion != 0)
            _str += "���� ���� ���� : " + itemStat.MaxPostion + "\n";
        if (itemStat.RecoveryHp != 0)
            _str += "HPȸ���� : " + itemStat.RecoveryHp + "\n";
        if (itemStat.RecoveryMp != 0)
            _str += "MPȸ���� : " + itemStat.RecoveryMp + "\n";

        return _str;
            
    }
    private string SetEnforceStr(ItemStat itemStat)
    {
        string _str = "";
        if (itemStat.Attack != 0)
            _str += "���ݷ� : " + itemStat.Attack + "\n";
        if (itemStat.HitPercent != 0)
            _str += "���� : " + itemStat.HitPercent + "\n";
        if (itemStat.SkillDamage != 0)
            _str += "��ų������ : " + itemStat.SkillDamage + "%\n";
        if (itemStat.AllDamge != 0)
            _str += "��絥���� : " + itemStat.AllDamge + "%\n";
        if (itemStat.Defense != 0)
            _str += "���� : " + itemStat.Defense + "\n";
        if (itemStat.Dodge != 0)
            _str += "ȸ�� : " + itemStat.Dodge + "\n";
        if (itemStat.ReduceDamage != 0)
            _str += "�޴� ��� ������ ���� : " + itemStat.ReduceDamage + "%\n";
        if (itemStat.MaxHp != 0)
            _str += "�ִ� ä�� : " + itemStat.MaxHp + "\n";
        if (itemStat.MaxMp != 0)
            _str += "�ִ� ���� : " + itemStat.MaxMp + "\n";
        if (itemStat.MaxPostion != 0)
            _str += "���� ���� ���� : " + itemStat.MaxPostion + "\n";
        if (itemStat.RecoveryHp != 0)
            _str += "HPȸ���� : " + itemStat.RecoveryHp + "\n";
        if (itemStat.RecoveryMp != 0)
            _str += "MPȸ���� : " + itemStat.RecoveryMp + "\n";

        return _str;

    }
    private void SetFade()
    {
        Color cl = EnforceStat.color;
        cl.a = 1;
        Color cl1 = EnforceNum.color;
        cl1.a = 1;
        EnforceStat.color = cl;
        EnforceNum.color = cl1;
        PlayFade = false;
        button.interactable = true;
    }
    IEnumerator Fade()
    {
        PlayFade = true;
        button.interactable = false;
        float currentTime = 0f;
        float percent = 0f;
        Color cl = EnforceStat.color;
        cl.a = 0;
        Color cl1 = EnforceNum.color;
        cl1.a = 0;
        EnforceStat.color = cl;
        EnforceNum.color = cl1;
        while (percent < 1)
        {
            currentTime += Time.deltaTime;
            percent = currentTime / 1f;

            Color color = EnforceStat.color;
            color.a = Mathf.Lerp(0f, 1f, percent);
            Color color1 = EnforceNum.color;
            color1.a = Mathf.Lerp(0f, 1f, percent);

            EnforceStat.color = color;
            EnforceNum.color = color1;
            yield return null;

        }
        button.interactable = true;
        PlayFade = false;
    }
    public void FadeInOut()
    {
        if(!PlayFade)
        { }
           StartCoroutine("Fade");
    }
}
