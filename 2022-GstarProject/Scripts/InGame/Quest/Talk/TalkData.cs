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
        talkData.Add(1, new string[] { "벨라","땅끝마을에 새로운 손님이 찾아오셨네요",
            "안녕하세요 마법사님.\n 저는 마을 안내원 벨라 라고 합니다.", 
            "혹시 저희 마을을 도와주실수 있으신가요? \n 보상은 충분히 드리겠습니다." ,
            "감사합니다 마법사님!\n 우선 인벤토리에 장비를 드리겠습니다!. \n 마을상인에게 가주세요!"});

        talkData.Add(2, new string[] { "릴리","지금 마을앞에 몬스터들이 침입했습니다 !..","마법사닙 혹시 강화버튼을 알고계신가요?\n 아이템을 장착 하신 다음 강화석을 이용해서 \n장비 숙련도를 강화할수 있습니다.",
            "몬스터 무리를 처치하시고 루치에게 가시면 좋은 아이템을 드리겠습니다." });

        talkData.Add(3, new string[] { "루이스","고생하셨습니다 마법사님",
            "이 앞은 고블린 서식지 입니다. 몬스터가 강력하니 조심하시길..",});

        talkData.Add(4, new string[] { "조이","큰일이야 큰일.",
            "마을 근처에 처음보는 거대한 골렘이 나타났어!.", "내 아이템을 줄게 꼭 처치해줘!" });

        talkData.Add(5, new string[] { "루치","고마워 덕분에 마을앞 골렘들이 줄어들었어.",
            "아이템을 나눠줄게! 언덕앞에있는 루이스에게 가봐."  });
        talkData.Add(101, new string[] { "마을상인","안녕 하십니까 마법사님.",
            "벨라에게 이야기는 들었습니다 여기 약속드린 포션과 강화석입니다.", "앞으로 포션과 강화석이 필요하시면 \n오른쪽 상단 상점버튼을 이용하시면 됩니다" });


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
            //해당 퀘스트 진행중 대사가 없을때 진행순서
            //퀘스트 맨처음 대사 가지고옴
            if (talkData.ContainsKey(id - id % 10))
            {
                return GetTalk(id - id % 10, talkIndex);
            }
            else
            {
                //퀘스트 맨처음 대사
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
