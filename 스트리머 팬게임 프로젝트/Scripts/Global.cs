public static class Global
{
    private const int _usingItemCount = 6;          // 배틀에 사용되는 아이템 최대 갯수
    private const int _itemQueueRepeat = 5;         // 아이템 순서 반복 횟수
    public const int ItemQueueLength = _usingItemCount * _itemQueueRepeat * 2;
    public const int MaxRoomPlayer = 8;

    public const int MaxUserNameSizeByByte = 22;

    public const float AllReadyTime = 5f;

    public const int BattleMapCount = 8;

    public const int MaxUsingItemCount = 6;
    public const int MaxUnUsingItemCount = 10;

    public const int CreepIDNumber = 1000000;
    
    public const string CircleFade = "_Radius";

    // 애니메이션 트리거 변수
    public const string UseTrigger = "Use";
    public const string HitTrigger = "Hit";
    public const string VictoryTrigger = "Victory";
    public const string DefeatTrigger = "Defeat";

    // 창 애니메이션
    public const string OpenBool = "Open";
    
    //단답용 애니메이션
    public const string UseYesTrigger = "Use_Yes";
    public const string UseNoTrigger = "Use_No";


}
