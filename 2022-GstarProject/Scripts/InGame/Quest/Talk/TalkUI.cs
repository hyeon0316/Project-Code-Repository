using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TalkUI : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Talktext;

    public bool isAnim;
    string targetMsg;
    int index;

    private string[] strs;
    private int strInt;
    public void SetText(string[] _str)
    {
        strs = _str;
        Name.text = _str[0];
        strInt = 1;
        isAnim = false;
        SetMsg(_str[1]);
    }
    public void OnsButton()
    {

        if (isAnim)
        {
            EffectEnd();
        }
        else
        {
            strInt++;
            if (strInt < strs.Length)
                SetMsg(strs[strInt]);
            else
            {
                QuestManager.Instance.UI.SetActive(true);
                gameObject.SetActive(false);
            }
        }
    }


    public void SetMsg(string msg)
    {
        if (isAnim)
        {
            CancelInvoke();//�������̴� Invoke�Լ� ĵ��
            EffectEnd();
        }
        else
        {
            targetMsg = msg;
            EffectStart();
        }
    }
    void EffectStart()//��ȭâ�� �ؽ�Ʈ�� �ѱ��ھ� ���
    {
        isAnim = true;
        Talktext.text= "";
        index = 0;
        Invoke("Effecting", 0.05f);
    }
    void Effecting()//�ؽ�Ʈ ��� ���� ��
    {
        if (Talktext.text == targetMsg)
        {
            EffectEnd();
            return;
        }
        Talktext.text += targetMsg[index];
        index++;
        Invoke("Effecting", 0.05f);//����Լ�
    }
    void EffectEnd()//�ؽ�Ʈ ��� ���
    {
        Talktext.text = targetMsg;
        isAnim = false;
    }
}
