using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHpBar : MonoBehaviour
{
    private RectTransform rectHp; //자신의 rectTransform 저장할 변수

    void Start()
    {
        rectHp = this.gameObject.GetComponent<RectTransform>();
    }

    //LateUpdate는 update 이후 실행함, 적의 움직임은 Update에서 실행되니 움직임 이후에 HpBar를 출력함
    private void LateUpdate()
    {   
        var localPos =new Vector3(0,470,0);        
        rectHp.localPosition = localPos; //그 좌표를 localPos에 저장, 거기에 hpbar를 출력
    }
}
