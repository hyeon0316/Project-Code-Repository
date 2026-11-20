using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class DungeonEntryMember
{
    public ECharacterType Type;
    public float HpRate;
    public float MpRate;
}

/// <summary>
/// 던전 진행중 게임을 강제종료하는 경우에 대한 진행 기록
/// </summary>
[System.Serializable]
public class DungeonProgress
{
    [JsonIgnore] public RuntimeMapTree RuntimeTree { get; private set; }
    [JsonIgnore] public EndContentSO EndContent { get; private set; }
    [JsonIgnore]
    public IMapTree ActiveTree
    {
        get
        {
            if (EndContent is TowerSO tower)
                return tower.GetFloor(Floor).Tree;

            return DungeonData.IsRandomMap ? (IMapTree)RuntimeTree : DungeonData.Tree;
        }
    }
    [JsonIgnore]
    public string ScalingID
    {
        get
        {
            if (EndContent is TowerSO tower)
                return tower.GetScalingID(Floor);

            return DungeonData.GetScalingID(DifficultyPhase);
        }
    }
    [JsonIgnore]
    public float ScaleMultiplier
    {
        get
        {
            if (EndContent is TowerSO tower)
                return tower.GetScaleMultiplier(Floor);
            if (EndContent is LabyrinthSO labyrinth)
                return labyrinth.GetScaleMultiplier(RandomMapIndex);

            return 1f;
        }
    }

    public EDungeonState StageType;
    [JsonConverter(typeof(DungeonSOConverter))]
    public DungeonSO DungeonData;
    public int DifficultyPhase;
    public string LastNodeID;
    public int RandomMapSeed;
    public int RandomMapIndex;
    public int TerrainIndex;
    public BattleProgress Battle = new();
    [JsonProperty(ItemConverterType = typeof(CardSOConverter))]
    public List<CardSO> ActiveCards = new();
    public List<string> ClearedNodeIDs = new();
    public string ContentID;
    public int Floor;
    public int Score;
    public int EntryPartyIndex;
    public List<DungeonEntryMember> EntryParty = new();

    public void Init(DungeonSO data, int difficultyPhase)
    {
        DungeonData = data;
        DifficultyPhase = difficultyPhase;
        StageType = EDungeonState.Map;

        if (data.IsRandomMap)
        {
            RandomMapSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            RandomMapIndex = 0;
            GenerateRuntimeTree();
        }
    }

    public void SetEndContent(EndContentSO content, int floor)
    {
        EndContent = content;
        ContentID = content.ContentID;
        Floor = floor;
    }

    public bool Restore()
    {
        if (DungeonData == null)
            return false;

        if (!string.IsNullOrEmpty(ContentID))
        {
            EndContent = SODatabase.FindEndContent(ContentID);
            if (EndContent == null)
                return false;
        }

        ActiveCards.RemoveAll(c => c == null);
        if (DungeonData.IsRandomMap)
            GenerateRuntimeTree();
        return true;
    }

    public void AdvanceRandomMap()
    {
        RandomMapIndex++;
        GenerateRuntimeTree();
        LastNodeID = RuntimeTree.GetFirstNode().GUID;
        StageType = EDungeonState.Map;
        ClearedNodeIDs.Clear();
    }

    private void GenerateRuntimeTree()
    {
        RuntimeTree = RandomMapGenerator.Generate(DungeonData.RandomMapConfig, RandomMapSeed + RandomMapIndex);
    }

    public void DestroyRuntimeTree()
    {
        if (RuntimeTree == null)
            return;

        foreach (var node in RuntimeTree.Nodes)
            UnityEngine.Object.Destroy(node);

        RuntimeTree = null;
    }

    public void Save()
    {
        SaveManager.Instance.SavePrefsData(GlobalVariable.PREFS_DUNGEON_PROGRESS, this);
    }

    public void Clear()
    {
        LastNodeID = string.Empty;
        Battle.Clear();
        ActiveCards.Clear();
        ClearedNodeIDs.Clear();
        DestroyRuntimeTree();
        SaveManager.Instance.DeletePrefsData(GlobalVariable.PREFS_DUNGEON_PROGRESS);
    }
}
