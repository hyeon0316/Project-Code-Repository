using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BGMType
{
    Title,
    Lobby,
    Loading,
    Select,
    CutScene,
    Ready,
    Battle
}

public enum EffectType
{
    CheckBox,
    MatchStart,
    ItemDrag,
    ItemDragEnd,
    CharacterClick,
    WakgoodSelect,
    IneSelect,
    JingburgerSelect,
    LilpaSelect,
    JururuSelect,
    GoseguSelect,
    ViichanSelect,
    RouletteUse,
    ItemDrop,
    ItemEquip,
    Upgrade,
    Upgrade2,
    ReCombination,
    Page,
    Bag,
    NameInput,
    SelectTimer,
    EntryBattle,
    WakgoodHit,
    IneHit,
    JingburgerHit,
    LilpaHit,
    JururuHit,
    GoseguHit,
    ViichanHit,
    ShrimpHit,
    NegativeManHit,
    HoddHit,
    WakpagoHit,
    ShortAnswerHit,
    ChunSikHit,
    KwonMinHit,
    Seconds,
    Attack,
    Heal,
    Defense,
    Blood,
    CounselLicense,
    Dodge,
    Bomb,
    WakEnter,
    DiaSword,
    Weaken
}
public class SetSounds : MonoBehaviour
{
    [Header("배경음")] 
    public AudioClip TitleBGM;
    public AudioClip LobbyBGM;
    public AudioClip LodingBGM;
    public AudioClip SelectBGM;
    public AudioClip CutSceneBGM;
    public AudioClip ReadyBGM;
    public AudioClip BattleBGM;
    
    [Header("효과음")]
    public AudioClip CheckBox;
    public AudioClip MatchStart;
    public AudioClip ItemDrag;
    public AudioClip ItemDragEnd;
    public AudioClip CharacterClick;
    public AudioClip WakgoodSelect;
    public AudioClip IneSelect;
    public AudioClip JingburgerSelect;
    public AudioClip LilpaSelect;
    public AudioClip JururuSelect;
    public AudioClip GoseguSelect;
    public AudioClip ViichanSelect;
    public AudioClip RouletteUse;
    public AudioClip ItemDrop;
    public AudioClip ItemEquip;
    public AudioClip Upgrade;
    public AudioClip Upgrade2;
    public AudioClip ReCombination;
    public AudioClip Credit;
    public AudioClip Bag;
    public AudioClip NameInput;
    public AudioClip SelectTimer;
    public AudioClip EntryBattle;
    public AudioClip WakgoodHit;
    public AudioClip IneHit;
    public AudioClip JingburgerHit;
    public AudioClip LilpaHit;
    public AudioClip JururuHit;
    public AudioClip GoseguHit;
    public AudioClip ViichanHit;
    public AudioClip ShrimpHit;
    public AudioClip NegativeManHit;
    public AudioClip HoddHit;
    public AudioClip WakpagoHit;
    public AudioClip ShortAnswerHit;
    public AudioClip ChunSikHit;
    public AudioClip KwonMinHit;
    public AudioClip Seconds;
    public AudioClip Attack;
    public AudioClip Heal;
    public AudioClip Defense;
    public AudioClip Blood;
    public AudioClip CounselLicense;
    public AudioClip Dodge;
    public AudioClip Bomb;
    public AudioClip WakEnter;
    public AudioClip DiaSword;
    public AudioClip Weaken;
}
