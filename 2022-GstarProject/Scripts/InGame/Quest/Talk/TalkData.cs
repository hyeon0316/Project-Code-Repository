using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkData : MonoBehaviour
{
    Dictionary<int, string[]> talkData;
    // Start is called before the first frame update
    void Awake()
    {
        talkData = new Dictionary<int, string[]>();
        GenerateData();
    }
    void GenerateData()
    {
        talkData.Add(1, new string[] { "����","���������� ���ο� �մ��� ã�ƿ��̳׿�",
            "�ȳ��ϼ��� �������.\n ���� ���� �ȳ��� ���� ��� �մϴ�.", 
            "Ȥ�� ���� ������ �����ֽǼ� �����Ű���? \n ������ ����� �帮�ڽ��ϴ�." ,
            "�����մϴ� �������!\n �켱 �κ��丮�� ��� �帮�ڽ��ϴ�!. \n �������ο��� ���ּ���!"});

        talkData.Add(2, new string[] { "����","���� �����տ� ���͵��� ħ���߽��ϴ� !..","������� Ȥ�� ��ȭ��ư�� �˰��Ű���?\n �������� ���� �Ͻ� ���� ��ȭ���� �̿��ؼ� \n��� ���õ��� ��ȭ�Ҽ� �ֽ��ϴ�.",
            "���� ������ óġ�Ͻð� ��ġ���� ���ø� ���� �������� �帮�ڽ��ϴ�." });

        talkData.Add(3, new string[] { "���̽�","����ϼ̽��ϴ� �������",
            "�� ���� ��� ������ �Դϴ�. ���Ͱ� �����ϴ� �����Ͻñ�..",});

        talkData.Add(4, new string[] { "����","ū���̾� ū��.",
            "���� ��ó�� ó������ �Ŵ��� ���� ��Ÿ����!.", "�� �������� �ٰ� �� óġ����!" });

        talkData.Add(5, new string[] { "��ġ","���� ���п� ������ �񷽵��� �پ�����.",
            "�������� �����ٰ�! ����տ��ִ� ���̽����� ����."  });
        talkData.Add(101, new string[] { "��������","�ȳ� �Ͻʴϱ� �������.",
            "���󿡰� �̾߱�� ������ϴ� ���� ��ӵ帰 ���ǰ� ��ȭ���Դϴ�.", "������ ���ǰ� ��ȭ���� �ʿ��Ͻø� \n������ ��� ������ư�� �̿��Ͻø� �˴ϴ�" });


    }
    public string[] GetStr(int _s)
    {
        return talkData[_s];
    }
    /*
    public string GetTalk(int id, int talkIndex)
    {
        Debug.Log(id + "," + talkIndex);
        if (!talkData.ContainsKey(id))
        {
            //�ش� ����Ʈ ������ ��簡 ������ �������
            //����Ʈ ��ó�� ��� �������
            if (talkData.ContainsKey(id - id % 10))
            {
                return GetTalk(id - id % 10, talkIndex);
            }
            else
            {
                //����Ʈ ��ó�� ���
                return GetTalk(id - id % 100, talkIndex);
            }
        }
        if (talkIndex == talkData[id].Length)
            return null;
        else
            return talkData[id][talkIndex];
    }
    */
}
