using System.Collections.Generic;

/// <summary>
/// 던전 진행중 게임을 강제종료하는 경우에 대한 진행 기록
/// </summary>
[System.Serializable]
public class DungeonProgress
{
    public EDungeonState StageType;
    public string DungeonID;
    public int DifficultyPhase;
    public string LastNodeID;
    public int RandomMapSeed;
    public int RandomMapIndex;
    public int TerrainIndex;
    public BattleProgress Battle = new();
    public List<string> ActiveCardIDs = new();
    public List<string> ClearedNodeIDs = new();

    public void Save()
    {
        SaveManager.Instance.SavePrefsData(GlobalVariable.PREFS_DUNGEON_PROGRESS, this);
    }

    public void Clear()
    {
        LastNodeID = string.Empty;
        DungeonID = string.Empty;
        DifficultyPhase = 0;
        Battle.Clear();
        ClearedNodeIDs.Clear();
        SaveManager.Instance.DeletePrefsData(GlobalVariable.PREFS_DUNGEON_PROGRESS);
    }
}


