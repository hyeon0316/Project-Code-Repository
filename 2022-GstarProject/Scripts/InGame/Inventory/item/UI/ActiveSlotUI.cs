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
            _str = "방어력";
        }
        if (_i == 2)
        {
            _str = "공격력";
        }
        if(_i == 3)
        {
            _str = "소모품";
            _mainStat = "HP 회복 :" + _stat.ToString();

        }
        if (_i == 4)
        {
            _str = "재료";

            
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
            _str += "명중 : " + itemStat.HitPercent + "\n";
        if (itemStat.SkillDamage != 0)
            _str += "스킬데미지 : " + itemStat.SkillDamage + "%\n";
        if (itemStat.AllDamge != 0)
            _str += "모든데미지 : " + itemStat.AllDamge + "%\n";
        if (itemStat.Dodge != 0)
            _str += "회피 : " + itemStat.Dodge + "\n";
        if (itemStat.ReduceDamage != 0)
            _str += "받는 모든 데미지 감소 : " + itemStat.ReduceDamage + "%\n";
        if (itemStat.MaxHp != 0)
            _str += "최대 채력 : " + itemStat.MaxHp + "\n";
        if (itemStat.MaxMp != 0)
            _str += "최대 엠피 : " + itemStat.MaxMp + "\n";
        if (itemStat.MaxPostion != 0)
            _str += "물약 소지 개수 : " + itemStat.MaxPostion + "\n";
        if (itemStat.RecoveryHp != 0)
            _str += "HP회복량 : " + itemStat.RecoveryHp + "\n";
        if (itemStat.RecoveryMp != 0)
            _str += "MP회복량 : " + itemStat.RecoveryMp + "\n";

        return _str;
            
    }
    private string SetEnforceStr(ItemStat itemStat)
    {
        string _str = "";
        if (itemStat.Attack != 0)
            _str += "공격력 : " + itemStat.Attack + "\n";
        if (itemStat.HitPercent != 0)
            _str += "명중 : " + itemStat.HitPercent + "\n";
        if (itemStat.SkillDamage != 0)
            _str += "스킬데미지 : " + itemStat.SkillDamage + "%\n";
        if (itemStat.AllDamge != 0)
            _str += "모든데미지 : " + itemStat.AllDamge + "%\n";
        if (itemStat.Defense != 0)
            _str += "방어력 : " + itemStat.Defense + "\n";
        if (itemStat.Dodge != 0)
            _str += "회피 : " + itemStat.Dodge + "\n";
        if (itemStat.ReduceDamage != 0)
            _str += "받는 모든 데미지 감소 : " + itemStat.ReduceDamage + "%\n";
        if (itemStat.MaxHp != 0)
            _str += "최대 채력 : " + itemStat.MaxHp + "\n";
        if (itemStat.MaxMp != 0)
            _str += "최대 엠피 : " + itemStat.MaxMp + "\n";
        if (itemStat.MaxPostion != 0)
            _str += "물약 소지 개수 : " + itemStat.MaxPostion + "\n";
        if (itemStat.RecoveryHp != 0)
            _str += "HP회복량 : " + itemStat.RecoveryHp + "\n";
        if (itemStat.RecoveryMp != 0)
            _str += "MP회복량 : " + itemStat.RecoveryMp + "\n";

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
