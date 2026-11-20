
/// <summary>
/// int값으로 저장 중(순서 유지)
/// </summary>
public enum ETutorialID
{
    None,
    Party,
    Dungeon,
    DungeonBattle,
    DungeonCard,
    DungeonItem
}

/// <summary>
/// int값으로 저장 중(순서 유지). None은 -1로 append해야 기존 0~4가 밀리지 않는다.
/// </summary>
public enum ETutorialAnchorID
{
    //공유 프리팹처럼 ID가 런타임에 정해지는 앵커. 주입 전까지는 등록 대상이 아니다.
    None = -1,

    PartyStart,
    PartyAccessButton,
    PartyMemberItem,
    PartyJoinButton,
    PartySlot,
    DungeonMapView,
    DungeonMapNode,
    BattleTurnOrder,
    BattleMoveButtons,
    BattleSkill,
    SelectCardList,
    SelectCardReroll,
    SelectCardItem,
    SelectCardCommit,
    BattleSpeedButton,
    BattleItemListButton,
    BattleItem,
    BattleSkillInfo
}

public enum ETutorialAdvanceType
{
    ClickAnywhere,
    ClickTarget,
    Pages,
    GameEvent
}
