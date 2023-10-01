using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 지속적인 연결이 필요한 데이터 관리
/// </summary>
public class DataManager : Singleton<DataManager>
{
    public JobType SelectJobType { get; set; } = JobType.Mage;
    public Player Player { get; set; } //todo: 캐릭터 선택할때 같이 적용
    public int Gold;
    public Gold GoldObj;

    public void SetGold(int _a)
    {
        Gold += _a;
        GoldObj.GoldText.text = ":"+ Gold.ToString() ;
    }



}
