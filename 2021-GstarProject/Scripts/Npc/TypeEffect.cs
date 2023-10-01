using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TypeEffect : MonoBehaviour
{
    public int CharPerSeconds;
    public GameObject nextText;
    public bool isAnim;
    string targetMsg;
    Text msgText;
    int index;
    float interval;

    private void Awake()
    {
        msgText = GetComponent<Text>();
    }
    public void SetMsg(string msg)
    {
        if (isAnim)
        {
            CancelInvoke();//실행중이던 Invoke함수 캔슬
            EffectEnd();
        }
        else
        { 
            targetMsg = msg;
            EffectStart();
        }
    }
    void EffectStart()//대화창의 텍스트가 한글자씩 출력
    {
        isAnim = true;
        msgText = GetComponent<Text>();
        msgText.text = "";
        index = 0;
        nextText.SetActive(false);
        interval = 1 / CharPerSeconds;
        Invoke("Effecting", 0.1f);
    }
    void Effecting()//텍스트 출력 진행 중
    {
        if(msgText.text == targetMsg)
        {
            EffectEnd();
            return;
        }
        msgText.text += targetMsg[index];
        index++;
        Invoke("Effecting", 0.1f);//재귀함수
    }
    void EffectEnd()//텍스트 모두 출력
    {
        msgText.text = targetMsg;
        isAnim = false;
        nextText.SetActive(true);
    }
}
