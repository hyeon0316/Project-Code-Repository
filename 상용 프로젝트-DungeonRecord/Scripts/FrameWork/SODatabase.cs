using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class SODatabase
{
    public static Dictionary<ECharacterType, CharacterBaseInfoSO> CharacterBaseInfoDic;
    public static Dictionary<EEnemyType, EnemyBaseInfoSO> EnemyBaseInfoDic;
    public static List<QuestSO> QuestList;
    public static List<DungeonSO> DungeonList;
    public static Dictionary<ETutorialID, TutorialSO> TutorialDic;
    public static Dictionary<(EGameEventType, string), TutorialSO> TutorialTriggerDic;
    public static Dictionary<string, ActorSO> ActorDic;
    private static bool m_IsInit;

    public static async UniTask InitAsync()
    {
        if (m_IsInit)
            return;

        var characterHandle = Addressables.LoadAssetAsync<CharacterBaseInfoParentSO>("CharacterBaseInfo");
        var questHandle = Addressables.LoadAssetAsync<QuestParentSO>("Quest");
        var dungeonHandle = Addressables.LoadAssetAsync<DungeonParentSO>("Dungeon");
        var cardHandle = Addressables.LoadAssetAsync<CardParentSO>("Card");
        var tutorialHandle = Addressables.LoadAssetAsync<TutorialParentSO>("Tutorial");
        var actorHandle = Addressables.LoadAssetAsync<ActorParentSO>("Actor");

        try
        {
            await UniTask.WhenAll(
                characterHandle.ToUniTask(),
                questHandle.ToUniTask(),
                dungeonHandle.ToUniTask(),
                cardHandle.ToUniTask(),
                tutorialHandle.ToUniTask(),
                actorHandle.ToUniTask());

            SetCharacterBaseInfo(characterHandle.Result);
            QuestList = questHandle.Result.QuestList;
            DungeonList = dungeonHandle.Result.List;
            CardManager.Init(cardHandle.Result.CardList);
            SetTutorial(tutorialHandle.Result);
            SetActor(actorHandle.Result);
        }
        finally
        {
            Addressables.Release(characterHandle);
            Addressables.Release(questHandle);
            Addressables.Release(dungeonHandle);
            Addressables.Release(cardHandle);
            Addressables.Release(tutorialHandle);
            Addressables.Release(actorHandle);
        }

        m_IsInit = true;
    }

    private static void SetCharacterBaseInfo(CharacterBaseInfoParentSO asset)
    {
        CharacterBaseInfoDic = new();
        foreach (var c in asset.CharacterList)
        {
            CharacterBaseInfoDic[c.Type] = c;
        }

        EnemyBaseInfoDic = new();
        foreach (var e in asset.EnemyList)
        {
            EnemyBaseInfoDic[e.Type] = e;
        }
    }

    private static void SetTutorial(TutorialParentSO asset)
    {
        TutorialDic = new();
        TutorialTriggerDic = new();
        foreach (var t in asset.TutorialList)
        {
            TutorialDic[t.ID] = t;

            var key = (t.Trigger, t.TriggerParamter);
            if (TutorialTriggerDic.ContainsKey(key))
                HDebug.LogError($"Duplicate tutorial trigger key. {t.Trigger}, {t.TriggerParamter}");
            TutorialTriggerDic[key] = t;
        }
    }

    private static void SetActor(ActorParentSO asset)
    {
        ActorDic = new();
        foreach (var a in asset.ActorList)
        {
            ActorDic[a.ID] = a;
        }
    }

    public static CharacterBaseInfoSO GetCharacterBaseInfo(ECharacterType type)
    {
        if (CharacterBaseInfoDic.TryGetValue(type, out var info))
            return info;

        HDebug.LogError($"Failed to find characterBase info. {type}");
        return null;
    }

    public static EnemyBaseInfoSO GetEnemyBaseInfo(EEnemyType type)
    {
        if (EnemyBaseInfoDic.TryGetValue(type, out var info))
            return info;

        HDebug.LogError($"Failed to find enemyBase info. {type}");
        return null;
    }

    public static TutorialSO GetTutorial(ETutorialID id)
    {
        if (TutorialDic.TryGetValue(id, out var so))
            return so;

        HDebug.LogError($"Failed to find tutorial SO. {id}");
        return null;
    }

    public static TutorialSO FindTutorialByTrigger(EGameEventType type, string param)
    {
        TutorialTriggerDic.TryGetValue((type, param), out var so);
        return so;
    }

    public static ActorSO GetActor(string id)
    {
        if (ActorDic.TryGetValue(id, out var so))
            return so;

        HDebug.LogError($"Failed to find actor SO. {id}");
        return null;
    }
}
