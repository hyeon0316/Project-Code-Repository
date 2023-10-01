using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerStatUI : MonoBehaviour
{
    private string _str;
    public TextMeshProUGUI PlayerStatText;
    private void Start()
    {
        _str = "";
    }
    void Update()
    {
        UpdateStat(DataManager.Instance.Player.Stat);
        PlayerStatText.text = _str;
    }
    private void UpdateStat(Stat _stat)
    {
        _str = "";
        _str += "--------공격------\n";
        _str += "공격력 : " + _stat.Attack + "\n";
        _str += "명중 : " + _stat.HitPercent + "\n";
        _str += "스킬데미지 : " + _stat.SkillDamage + "%\n";
        _str += "모든데미지 : " + _stat.AllDamge + "%\n";
        _str += "--------방어------\n";
        _str += "방어력 :" + _stat.Defense + "\n";
        _str += "회피 :  " + _stat.Dodge + "\n";
        _str += "받는 모든 데미지 감소 : " + _stat.ReduceDamage + "%\n";
        _str += "--------기타------\n";
        _str += "최대 채력 : " + _stat.MaxHp + "\n";
        _str += "최대 엠피 : " + _stat.MaxMp + "\n";
        _str += "물약 소지 개수 : " + _stat.MaxPostion + "\n";
        _str += "HP회복량 : " + _stat.RecoveryHp + "\n";
        _str += "MP회복량 : " + _stat.RecoveryMp + "\n";
    }
}
