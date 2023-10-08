using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 햄버거
/// </summary>
public class Hamburger : Item
{
    [SerializeField] private Sprite[] _burgerSprites;

    private int _spriteIndex;

    /// <summary>
    /// 4개의 햄버거 중 결정된 햄버거의 타입
    /// </summary>
    public EHamburgerType SelectType { get; set; }
    
    private int _upgradeValue1 = 3;
    private int _upgradeValue2 = 6;
    private int _upgradeValue3 = 9;
    

    /// <summary>
    /// 햄버거 타입 별 값
    /// 휠렛: 약화, 기네스: 데미지, 화갈: 회복, 밥버거: 방어
    /// </summary>
    private int[] _values = new int[4];

    private void Start()
    {
        if (InGame.CurGameType == EGameType.Ready) //준비단계에서는 이미지를 차례대로 보여준다.
        {
            StartCoroutine(ChangeSprite());
        }
    }

    /// <summary>
    /// 4종류의 햄버거중 하나를 선택
    /// </summary>
    public void SelectBurger(EHamburgerType type)
    {
        SelectType = type;
        _image.sprite = _burgerSprites[(int) type];
    }

    public override void Active(BattleCharacter player, BattleCharacter opponent)
    {
        switch (SelectType)      
        {
            case EHamburgerType.Fillet:
                opponent.Weaken();
                break;
            case EHamburgerType.Guinness:
                player.AttackEnemy();
                break;
            case EHamburgerType.WhiteGarlic:
                player.HealAvatar();
                break;
            case EHamburgerType.Rice:
                player.AddDefense();
                break;
        }
    }

    private IEnumerator ChangeSprite()
    {
        switch (_spriteIndex)
        {
            case 0:
                _image.sprite = _burgerSprites[(int)EHamburgerType.Fillet];
                break;
            case 1:
                _image.sprite = _burgerSprites[(int)EHamburgerType.Guinness];
                break;
            case 2:
                _image.sprite = _burgerSprites[(int)EHamburgerType.WhiteGarlic];
                break;
            case 3:
                _image.sprite = _burgerSprites[(int)EHamburgerType.Rice];
                break;
        }
        yield return new WaitForSeconds(1f);
        _spriteIndex++;
        if (_spriteIndex >= 4)
            _spriteIndex = 0;
        
        StartCoroutine(ChangeSprite());
    }

    public override void ApplyUpgrade()
    {
        switch (Upgrade)
        {
            case 0:
                _values[(int) EHamburgerType.Fillet] = 2;
                _values[(int) EHamburgerType.Guinness] = 7;
                _values[(int) EHamburgerType.WhiteGarlic] = 4;
                _values[(int) EHamburgerType.Rice] = 4;
                break;
            case 1:
                _values[(int) EHamburgerType.Fillet] = 3;
                _values[(int) EHamburgerType.Guinness] = 9;
                _values[(int) EHamburgerType.WhiteGarlic] = 6;
                _values[(int) EHamburgerType.Rice] = 6;
                break;
            case 2:
                _values[(int) EHamburgerType.Fillet] = 4;
                _values[(int) EHamburgerType.Guinness] = 12;
                _values[(int) EHamburgerType.WhiteGarlic] = 9;
                _values[(int) EHamburgerType.Rice] = 9;
                break;
        }

    }
    
    public override void SetEquipEffectText()
    {
        _effect = $"횔렛버거 : <color=yellow>{_values[(int) EHamburgerType.Fillet]}</color>만큼 상대를 <color=red>약화</color> 합니다.\n" +
                  $"기네스버거 : <color=yellow>{_values[(int) EHamburgerType.Guinness]}</color> 데미지를  가합니다.\n" +
                  $"화이트 갈릭 버거 : <color=yellow>{_values[(int) EHamburgerType.WhiteGarlic]}</color> <color=#4aa8d8>체력</color>을 회복합니다.\n" +
                  $"밥버거 : <color=yellow>{_values[(int) EHamburgerType.Rice]}</color> <color=#4aa8d8>방어도</color>를 얻습니다.\n";
    }
    
    public override void SetEquipEffect(int playerID)
    {
        switch (Upgrade)
        {
            case 0:
                PlayerManager.Instance.GetPlayer(playerID).UpdateDefense(_upgradeValue1);
                break;
            case 1:
                PlayerManager.Instance.GetPlayer(playerID).UpdateDefense(_upgradeValue2);
                break;
            case 2:
                PlayerManager.Instance.GetPlayer(playerID).UpdateDefense(_upgradeValue3);
                break;
        }
    }
    
    protected override void ShowEquipEffectPanel()
    {
        switch (Upgrade)
        {
            case 0:
                ToolTipManager.Instance.ShowEquipEffect($"장착효과 : 방어도 + <color=yellow>{_upgradeValue1}</color>");
                break;
            case 1:
                ToolTipManager.Instance.ShowEquipEffect($"장착효과 : 방어도 + <color=yellow>{_upgradeValue2}</color>");
                break;
            case 2:
                ToolTipManager.Instance.ShowEquipEffect($"장착효과 : 방어도 + <color=yellow>{_upgradeValue3}</color>");
                break;
        }
    }

}
