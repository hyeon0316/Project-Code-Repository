using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkManager : MonoBehaviour
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
        talkData.Add(100, new string[] { "들어가기에는 너무 깊어 보인다." });
        talkData.Add(200, new string[] { "마른 나무를 구할수 있을것 같다." });
        talkData.Add(300, new string[] { "여기로 내려가는건 좋은생각 같지 않다." });
        talkData.Add(400, new string[] { "\"마력 제어 목걸이\"를 얻었다." });
        talkData.Add(500, new string[] { "\"대마법사의 모자\"를 얻었다.\n\"대마법사의 하의\"를 얻었다." });
        talkData.Add(600, new string[] { "\"대마법사의 지팡이\"를 얻었다.\n\"대마법사의 책\"을 얻었다." });
        talkData.Add(700, new string[] { "\"대마법사의 로브\"를 얻었다.\n\"대마법사의 부츠\"를 얻었다." });
        talkData.Add(800, new string[] { "빈 상자다." });

        talkData.Add(1000, new string[] { "안녕", "이 곳에 처음 왔구나?." });
        talkData.Add(2000, new string[] { "안녕?", "타지 사람은 오랜만이네~." });
        talkData.Add(3000, new string[] { "반갑네." });
        talkData.Add(4000, new string[] { "어이.", "왼쪽으로는 가지 않는 게 좋을 거야." });
        talkData.Add(5000, new string[] { "너는 누구지?.", "스킬은 배우고 오는게 좋을거야."});
        talkData.Add(6000, new string[] { "흠냐.. 흠냐..zZ", "\"졸고 있는거 같다. 조용히 가자\"" });
        talkData.Add(7000, new string[] { "빨리 재료를 다시 모아야 할텐데...", });
        talkData.Add(8000, new string[] { "텔레포트가 어디가 잘못된 거지..?", "스승님한테 혼나겠다.." });
        talkData.Add(9000, new string[] { "던전은 위험합니다.", "돌아가세요." });
        //Quest

        talkData.Add(8000 + 10 + 0, new string[] { "너...너.. 뭐하는 녀석이야!!!!", "우리가 타야하는 텔레포트 였다고!!!!", "후.. 일단 스승님한테 가봐.." });
        talkData.Add(7000 + 10 + 1, new string[] { "허허...신기한 일을 다 겪는구만..","일단.. 자네도 원래 있던 곳으로 돌아가야 하지 않나...",
            "우리는 지금 바빠서 재료를 다시 모을 수가 없네.","자네가 대신 해줘야 할 거 같은데.",
            "앞으로의 여정에 필요한 물품을 주겠네 인벤토리를 확인해 보게.", "지금은 기본공격 밖에 할 수 없을 거네.",
            "재료를 모아와주면 다른 스킬들을 주겠네.", "광산에 가서 동굴거미의 심장을 5개 가져와 주게.." });
        talkData.Add(7000 + 10 + 2, new string[] { "아직 다 못 구했나?. " });
        talkData.Add(7000 + 10 + 3, new string[] { "고생했네.", "금방 스킬을 부여해 주겠네.", "다시한번 말을 걸어주게." });
        talkData.Add(7000 + 10 + 4, new string[] { "스킬과 적당량의 경험치 일세." });



        talkData.Add(7000 + 20 + 0, new string[] { "소피아양 미안하네.", "텔레포트를 구현하느라 마력이 조금 부족해서 스킬을 전부 주지 못했네.",
                    "광산2에 가서 본그리브를 잡다보면 반지가 나올걸세.","반지좀 구해다 주게나 그럼 나머지 스킬들을 주겠네." });
        talkData.Add(7000 + 20 + 1, new string[] { "다 구했나?." });
        talkData.Add(7000 + 20 + 2, new string[] { "아직 반지는 구하지 못했나 보군." });
        talkData.Add(7000 + 20 + 3, new string[] { "고생했네", "금방 스킬을 만들어 주겠네.", "다시한번 말을 걸어주게." });
        talkData.Add(7000 + 20 + 4, new string[] { "스킬을 다만들었네! 포션도 조금 주겠네 조심하게." });

        talkData.Add(7000 + 30 + 0, new string[] { "소피아양 이제 내가 줄 수 있는건 다 줬네.", "던전 입구에 있는 기사단장 엔 에게 가보게.","그녀가 도움을 줄거야." });

        talkData.Add(9000 + 30 + 1, new string[] { "당신이 소피아양 인가요?.", "던전은 매우 위험합니다 \n 사정은 들었습니다.", "어쩔 수 없네요 숙련자의 장비를 드리겠습니다.", "던전 입구 포탈을 열었습니다. \n무운을 빕니다.", "던전에 대한 정보는 던전 안의 유령이 알고 있을 것입니다." }) ;
        
        
        talkData.Add(4000 + 40 + 0, new string[] { "어서와! 어서와!","나? 나는 예전에 리치에게 \n도전했다가 패배해서 이렇게 변했지 머람~.",
            "리치가 누구냐고?","이 녀석을 쓰러뜨려야 너가 집에 갈 수 있을 걸?\n하지만 지금 당장은 만나진 못할 거야 방법을 알려줄 테니 내 부탁을 들어줄래?.","던전 1번방에가서 고블린을 잡고 광기 가득한 눈 10개를 모아와줘!."});
        talkData.Add(4000 + 40 + 1, new string[] { "아직 광기 가득한 눈을 다 못 구한 거 같은데?." });
        talkData.Add(4000 + 40 + 2, new string[] { "이야 벌써 구해왔어?. ","보상으로 포션이랑 경험치 조금 줄게!.","다시 말 걸어줘!."});
        talkData.Add(4000 + 40 + 3, new string[] { "여기 보상이야!." });


        talkData.Add(4000 + 50 + 0, new string[] {"리치한테 가는법이 궁금하다고 했지?.",
            "리치의 방에 갈려면 보물방에 있는 \"마력 제어 목걸이\"가 있어야 하는데.","지금 보물방은 잠겨 있어. 너가 열쇠랑 재료를 구해와줘.","보물방 열쇠 두개로 나눠져있어.","던전 2,3번방에서 몬스터를 잡다보면 하나씩 나올거야!.","아 그리고 잡는김에 던전2, 3번방 전리품도 10개씩 챙겨와줘!." });

        talkData.Add(4000 + 50 + 1, new string[] { "아직 다 못 구한거 같은데?." });
        talkData.Add(4000 + 50 + 2, new string[] { "열쇠조각을 다 구했구나! 다시 말을 걸어줘 금방 보물방을 열어줄게." });
        talkData.Add(4000 + 50 + 3, new string[] { "보물방이 열렸어!." });

        
        talkData.Add(4000 + 60 + 0, new string[] { "보물방은 던전1에서 앞으로 쭉가면 나올거야. ","보물방을 잘찾아보면 \"마력제어 목걸이\"를 가져와줘!.",
            "\"마력제어목걸이\"를 가져오면 보스방을 열어줄게.","참고로 보물방은 보물방인 이유가 있으니까 다른 귀중품도 잘 찾아보라고!." });
        talkData.Add(4000 + 60 + 1, new string[] { "\"마력제어 목걸이\"는 아직이야?." });
        talkData.Add(4000 + 60 + 2, new string[] { "\"마력제어 목걸이\"구했구나! 잠시후에 말을 걸어줘 금방 보스방을 열어줄게." });
        talkData.Add(4000 + 60 + 3, new string[] { "보스방이 열렸어","조심해.. 리치는 강력해.","리치를 쓰러뜨리고 나한테 다시와.", "건투를 빌게."});

        talkData.Add(4000 + 70 + 0, new string[] { "물약이 부족하면 나한테 말해줘!." });
        talkData.Add(4000 + 70 + 1, new string[] { "리치를 쓰러뜨렸다고?... 맙소사..","고생했어 \"강력한 마력이 깃든 물약\"을 마을 촌장한테 전해 주면 촌장이 마력을 전부 회복할꺼야." });


        talkData.Add(7000 + 80 + 0, new string[] { "고생했네 소피아양","재료를 다 모았네, 잠시만 기다려주게.." });
        talkData.Add(7000 + 80 + 1, new string[] { "수고했네 소피아양","곧 텔레포트를 가동 시킬테니 준비하게." });

    }

    public string GetTalk(int id,int talkIndex)
    {
        Debug.Log(id + "," + talkIndex);
        if(!talkData.ContainsKey(id))
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
}
