
public static class GlobalVariable
{
    public static readonly string USER_DATA_KEY = "UserData";
    public static readonly string USER_PROFILE_KEY = "UserProfile";

    public static readonly string CONTENTS_INVENTORY_STACK_KEY = "Inventory_Stack";
    public static readonly string CONTENTS_INVENTORY_EQUIP_KEY = "Inventory_Equip";
    public static readonly string CONTENTS_OWNED_CHARACTERS_KEY = "OwnedCharacterData";
    public static readonly string CONTENTS_EQUIPPED_ITEMS_KEY = "EquippedItemData";
    public static readonly string CONTENTS_QUEST_KEY = "QuestData";
    public static readonly string CONTENTS_QUEST_ACTIVE_KEY = "QuestActiveData";
    public static readonly string CONTENTS_SHOP_KEY = "ShopRecordData";
    public static readonly string CONTENTS_PARTYSETTING_KEY = "PartyData";
    public static readonly string CONTENTS_STAMINA_KEY = "StaminaData";
    public static readonly string CONTENTS_BATTLEPASS_KEY = "BattlePassData";
    public static readonly string CONTENTS_TUTORIAL_KEY = "TutorialData";
    public static readonly string CONTENTS_ADVENTURE_GUIDE_KEY = "AdventureGuideData";
    public static readonly string CONTENTS_ADVENTURE_GUIDE_PROGRESS_KEY = "AdventureGuideProgressData";
    public static readonly string CONTENTS_ACHIEVEMENT_KEY = "AchievementData";
    public static readonly string CONTENTS_ACHIEVEMENT_PROGRESS_KEY = "AchievementProgressData";
    public static readonly string CONTENTS_BATTLEPASS_PROGRESS_KEY = "BattlePassProgressData";

    public static readonly string PREFS_USER_LOCAL_DATA_KEY = "CharacterPos";
    public static readonly string PREFS_CONTENTS_NEWSTATE_KEY = "TownNewStates";
    public static readonly string PREFS_POLICY_KEY = "policy";
    public static readonly string PREFS_MAIL_READ_KEY = "MailRead";
    public static readonly string PREFS_QUEST_KEY = "QuestLocal";
    public static string PREFS_DUNGEON_EXPLORATION_NODE_STATE = "DungeonExplorationData";
    public static readonly string PREFS_DUNGEON_PROGRESS = "DungeonProgress";

    public static readonly int DAILY_RESET_HOUR = 20;

    public static readonly string NEW_INVENTORY_MARK_KEY = "NewInventory";
    public static readonly string NEW_QUEST_MARK_KEY = "NewQuest";
    public static readonly string NEW_CASHSHOP_MARK_KEY = "NewCashShop";
    public static readonly string NEW_GACHASHOP_MARK_KEY = "NewGachaShop";
    public static readonly string NEW_EVENT_MARK_KEY = "NewEvent";
    public static readonly string NEW_MAIL_MARK_KEY = "NewMail";
    public static readonly string NEW_TOWN_CHARACTER_SELECTION_MARK_KEY = "NewTownCharacter";
    public static readonly string NEW_PARTY_MARK_KEY = "NewParty";
    public static readonly string NEW_FRIEND_MARK_KEY = "NewFriend";
    public static readonly string NEW_NOTICE_MARK_KEY = "NewNotice";
    public static readonly string NEW_CHARACTER_INFO_MARK_KEY = "NewCharacterInfo";

    public static readonly string EMAIL_REGISTERED_KEY = "EmailRegistered";
    public static readonly string QUEST_READ_KEY = "QuestRead";
    public static readonly string NODE_ENTRY_FIELD = "Entry";
    public static readonly string NODE_EXIT_FIELD = "Exit";

    public static readonly int PARTY_MEMER_MAX_COUNT = 4;
    public static readonly int PARTY_LIST_MAX_COUNT = 4;
    public static readonly int SKILL_MAX_LEVEL = 10;
    public static readonly int DUPE_MAX_PHASE = 6;

    public static readonly string SCENE_TOWN_START_KEY = "Town_Start";
    public static readonly string SCENE_DUNGEON_KEY = "Dungeon";
    public static readonly string SCENE_RESTAREA_KEY = "RestArea";

    public static readonly string ANIMATION_CLIP_IDLE = "Idle";
    public static readonly string ANIMATION_CLIP_RUN = "Run";
    public static readonly string ANIMATION_CLIP_DEAD = "Dead";
    public static readonly string ANIMATION_CLIP_HURT = "Hurt";
    public static readonly string ANIMATION_CLIP_ATTACK1 = "Attack1";
    public static readonly string ANIMATION_CLIP_ATTACK2 = "Attack2";
    public static readonly string ANIMATION_CLIP_ATTACK3 = "Attack3";
    public static readonly string ANIMATION_CLIP_CAST1 = "Cast1";
    public static readonly string ANIMATION_CLIP_CAST2 = "Cast2";
    public static readonly string ANIMATION_CLIP_CAST3 = "Cast3";
    public static readonly float ANIMATION_SAMPLE_RATE = 60;
    public static readonly float DURATION_TIME_DEAD_ANIM = 1;

    public static readonly float DUNGEON_MAP_NODE_SPACING = 200;
    public static readonly float CHARACTER_CHANGING_SPEED = 0.3f;
    public static readonly float DURATION_TIME_SKIP_TURN = 1f;
    public static readonly float BLEED_ATK_RATIO = 0.3f;
    public static readonly int MAX_STACK_BLEED = 10;
    public static readonly int MAX_STACK_BLIGHT = 50;

    public static readonly string[] STUN_SYSTEM_RESIST_BUFF_IDS =
    {
        "EFF_SYSTEM_STUN_BUFF_1",
        "EFF_SYSTEM_STUN_BUFF_2",
        "EFF_SYSTEM_STUN_BUFF_3",
        "EFF_SYSTEM_STUN_BUFF_4",
    };
    public static readonly float TURN_ORDER_STEP_DURATION = 0.1f;

    public static readonly float CHECK_INTERVAL = 30;
}
