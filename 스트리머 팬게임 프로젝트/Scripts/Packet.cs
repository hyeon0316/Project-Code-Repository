using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using MemoryStream;
using Packet;
using UnityEngine;

/// <summary>
/// 패킷 타입<br/>
/// sc : Server to Client<br/>
/// cs : Client to Server
/// </summary>
public enum EPacketType : byte
{
    /// <summary> 클라이언트에게 Room 생성과 접속했음을 알리는 패킷 타입 </summary>
    sc_connectRoom,

    /// <summary> 클라이언트가 서버에게 매칭을 시작했음을 알리는 패킷 타입 </summary>
    cs_startMatching,

    /// <summary> 아이템을 추가했음을 알리는 패킷 타입  </summary>
    sc_addNewItem,

    /// <summary> 아이템을 다른 슬롯으로 이동했음을 알리는 패킷 타입 </summary>
    cs_sc_changeItemSlot,

    /// <summary> 아이템을 업그레이드 했음을 알리는 패킷 타입 </summary>
    sc_upgradeItem,

    /// <summary> 선택 캐릭터가 교체되었음을 알리는 패킷 타입 </summary>
    cs_sc_changeCharacter,

    /// <summary> 클라이언트에게 전투 정보를 알리는 패킷 타입 </summary>
    sc_battleInfo,

    /// <summary> 무언가를 알리는 패킷 타입 </summary>
    cs_sc_notification,

    /// <summary> 이모티콘을 보냈음을 알리는 패킷 타입 </summary>
    cs_sc_useEmoticon,

    /// <summary> 캐릭터 정보를 갱신해주는 패킷 타입 </summary>
    sc_updateCharacterInfo,

    /// <summary> 캐릭터 선택 시간을 설정해주는 패킷 타입 </summary>
    sc_setChoiceCharacterTime,

    /// <summary> 준비시간을 설정해주는 패킷 타입 </summary>
    sc_setReadyTime,

    /// <summary> 아이템 드랍 패킷 타입 </summary>
    cs_sc_dropItem,

    /// <summary> 아이템 조합 요청 패킷 타입 </summary>
    cs_requestCombinationItem,

    /// <summary> 아이템 뽑기권 개수 지정 패킷 타입 </summary>
	sc_setItemTicket,

    /// <summary> 아이템 발동 패킷 타입 </summary>
    sc_activeItem,

    /// <summary> 화면이 서서히 어두어지는 FadeIn 패킷 타입 </summary>
    sc_fadeIn,

    /// <summary> 화면이 서서히 밝아지는 FadeOut 패킷 타입 </summary>
    sc_fadeOut,

    /// <summary> 전투 상대 순서 패킷 타입 </summary>
    sc_battleOpponents,

    /// <summary> 햄버거 타입 지정하는 패킷 타입 </summary>
    sc_setHamburgerType,

    /// <summary> 비밀스런 마법봉 정보 패킷 타입 </summary>
    sc_magicStickInfo,

    /// <summary> 아이템 업그레이드 요청 패킷 타입 </summary>
    cs_requestUpgradeItem,

    /// <summary> 박사의 만능툴 정보 패킷 타입 </summary>
    sc_DoctorToolInfo,

    /// <summary> 크립 라운드 정보 패킷 타입 </summary>
    sc_CreepStageInfo,

    /// <summary> 전투 아바타 정보 패킷 타입 </summary>
    sc_BattleAvatarInfo,

    /// <summary> 인벤토리 정보 패킷 타입 </summary>
    sc_InventoryInfo,

    /// <summary> 현재 매칭 정보 패킷 타입 </summary>
    sc_matchingInfo,

    /// <summary> 현재 전투 시간 정보 패킷 타입 </summary>
    sc_BattleTimeInfo,
}

/// <summary> cs_sc_notification의 알림 타입 </summary>
public enum ENotificationType : byte
{
    /// <summary> 클라이언트가 서버 접속했음을 알리는 패킷 타입 </summary>
    ConnectServer,

    /// <summary> 캐릭터를 선택했음을 알리는 패킷 타입 </summary>
    ChoiceCharacter,

    /// <summary> 캐릭터 선택 다 끝났을 때 </summary>
    ChoiceAllCharacter,

    /// <summary> 클라이언트가 서버 해제했음을 알리는 패킷 타입 </summary>
    DisconnectServer,

    /// <summary> 서버에 계속 연결되는 상태를 알리는 패킷 타입 </summary>
    ConnectCheck,

    /// <summary> 준비 스테이지에 진입했음을 알리는 패킷 타입 </summary>
    EnterReadyStage,

    /// <summary> 컷신 스테이지에 진입했음을 알리는 패킷 타입 </summary>
    EnterCutSceneStage,

    /// <summary> 컷신 스테이지에 끝났음을 알리는 패킷 타입 </summary>
	FinishCutSceneStage,

    /// <summary> 전투 스테이지에 진입했음을 알리는 패킷 타입 </summary>
    EnterBattleStage,

    /// <summary> 크립라운드 스테이지에 진입했음을 알리는 패킷 타입 </summary>
    EnterCreepStage,

    /// <summary> 캐릭터 선택이 끝났음을 알리는 패킷 타입 </summary>
	FinishChoiceCharacterTime,

    /// <summary> 사이클 당 전투 슬롯 아이템 초기화 패킷 타입 </summary>
    InitBattleSlot,

    /// <summary> 일반 아이템 뽑기권 사용을 알리는 패킷 타입 </summary>
    UseNormalItemTicket,

    /// <summary> 고급 아이템 뽑기권 사용을 알리는 패킷 타입 </summary>
    UseAdvancedItemTicket,

    /// <summary> 최고급 아이템 뽑기권 사용을 알리는 패킷 타입 </summary>
    UseTopItemTicket,

    /// <summary> 지존 아이템 뽑기권 사용을 알리는 패킷 타입 </summary>
    UseSupremeItemTicket,

    /// <summary> Using이 비었을때 자동 장착 됬음을 알리는 패킷 타입 </summary>
    SetAutoUsingItem,

    /// <summary> 크립 아이템이 발동됬음을 알리는 패킷 타입 </summary>
    EffectCreepItem,
}

/// <summary> 이모티콘 타입 </summary>
public enum EEmoticonType : byte
{
    Woowakgood,
    Ine,
    Jinburger,
    Lilpa,
    Jururu,
    Gosegu,
    Viichan,
}

public enum EItemTicketType : byte
{
    /// <summary>
    /// 일반
    /// </summary>
    Normal,
    /// <summary>
    /// 고급
    /// </summary>
    Advanced,
    /// <summary>
    /// 최고급
    /// </summary>
    Top,
    /// <summary>
    /// 지존
    /// </summary>
    Supreme
};

public enum EHamburgerType : byte
{
    Fillet,
    Guinness,
    WhiteGarlic,
    Rice
};

public enum ECreepType : byte
{
    Shrimp,
    NegativeMan,
    Hodd,
    Wakpago,
    ShortAnswer,
    ChunSik,
    KwonMin
};

namespace Packet
{
    /// <summary> 유저의 정보 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct UserInfo
    {
        public UInt32 networkID;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Global.MaxUserNameSizeByByte)]
        public Byte[] name;
    }

    /// <summary> 서버가 클라이언트에게 Room 생성과 접속했음을 알리는 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_ConnectRoomPacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;

        /// <summary>
        /// Room에 속한 유저들 정보<br/>
        /// - 유저의 네트워크 아이디와 닉네임이 들어있다
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Global.MaxRoomPlayer)]
        public UserInfo[] users;

        public void ToData(ReadMemoryStream readStream)
        {
            this = readStream.ReadByStruct<sc_ConnectRoomPacket>();
        }
    }

    /// <summary> 클라이언트가 서버에게 매칭을 시작했음을 알리는 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct cs_StartMatchingPacket : IConvertToBytes
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;

        /// <summary>
        /// 유저의 이름을 서버에 알려주는 용도로 사용<br/>
        /// 글자수가 10자를 넘기면 안 된다
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Global.MaxUserNameSizeByByte)]
        public Byte[] name;

        public void ToBytes(WriteMemoryStream writeStream)
        {
            writeStream.WriteByStruct(this);
        }
    }

    /// <summary> 어느쪽으로든 아이템을 추가했음을 알리는 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_AddNewItemPacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;

        public Byte itemSlot;

        /// <summary> 추가되는 아이템 코드 </summary>
        public Byte itemCode;

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            networkID = readStream.ReadByInt32();
            itemSlot = readStream.ReadByByte();
            itemCode = readStream.ReadByByte();
        }
    }

    /// <summary> 어느쪽으로든 아이템을 다른 슬롯으로 이동했음을 알리는 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct cs_sc_ChangeItemSlotPacket : IConvertToBytes, IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;

        /// <summary> 아이템이 원래 있던 슬롯 번호 </summary>
        public Byte slot1;

        /// <summary> 아이템이 이동되는 슬롯 번호 </summary>
        public Byte slot2;

        public cs_sc_ChangeItemSlotPacket(Int32 networkID, Byte slot1, Byte slot2)
        {
            size = (UInt16)Marshal.SizeOf<cs_sc_ChangeItemSlotPacket>();
            type = EPacketType.cs_sc_changeItemSlot;
            this.networkID = networkID;
            this.slot1 = slot1;
            this.slot2 = slot2;
        }

        public void ToBytes(WriteMemoryStream writeStream)
        {
            writeStream.WriteByUInt16(size);
            writeStream.WriteByByte((byte)type);
            writeStream.WriteByInt32(networkID);
            writeStream.WriteByByte(slot1);
            writeStream.WriteByByte(slot2);
        }

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            networkID = readStream.ReadByInt32();
            slot1 = readStream.ReadByByte();
            slot2 = readStream.ReadByByte();
        }
    }

    /// <summary> 어느쪽으로든 아이템을 업그레이드 했음을 알리는 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_UpgradeItemPacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;

        /// <summary> 업그레이드 할 아이템이 있는 슬롯 번호 </summary>
        public Byte slot;

        // 서버에서 업그레이드된 수치를 받는데 사용
        // 굳이 클라이언트에서 upgrade를 수정해서 보낼 필요가 없음
        // upgrade 수치를 해당 슬롯 아이템에 바로 적용하면 된다
        /// <summary> 업그레이드 수치 </summary>
        public Byte upgrade;

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            networkID = readStream.ReadByInt32();
            slot = readStream.ReadByByte();
            upgrade = readStream.ReadByByte();
        }
    }

    /// <summary> 어느쪽으로든 선택 캐릭터가 교체되었음을 알리는 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct cs_sc_ChangeCharacterPacket : IConvertToBytes, IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;

        /// <summary> 변경할 캐릭터 타입 </summary>
        public ECharacterType characterType;

        public cs_sc_ChangeCharacterPacket(Int32 networkID, ECharacterType characterType)
        {
            size = (UInt16)Marshal.SizeOf<cs_sc_ChangeCharacterPacket>();
            type = EPacketType.cs_sc_changeCharacter;
            this.networkID = networkID;
            this.characterType = characterType;
        }

        public void ToBytes(WriteMemoryStream writeStream)
        {
            writeStream.WriteByUInt16(size);
            writeStream.WriteByByte((byte)type);
            writeStream.WriteByInt32(networkID);
            writeStream.WriteByByte((byte)characterType);
        }

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            networkID = readStream.ReadByInt32();
            characterType = (ECharacterType)readStream.ReadByByte();
        }
    }

    /// <summary> 아이템 발동 순서가 담긴 정보 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct ItemQueueInfo
    {
        /// <summary> 아이템 발동 순서의 유저 네트워크 아이디 </summary>
        public readonly Int32 networkID;

        /// <summary>
        /// UsingInventory 아이템 슬롯의 발동 순서와 발동 여부가 반복횟수 만큼 들어있다.<br/>
        /// 발동 여부는 서버에 저장되어 있는 아이템 발동 확률에 의해 결정되어서 클라이언트로 전달된다.<br/>
        /// 발동 슬롯 번호에 255 값이 들어있으면 더 이상 발동될 수 있는 아이템이 없는 것이다.<br/>
        /// 발동 슬롯 번호에 254 값이 들어있으면 해당 슬롯이 Lock 된것이다.<br/>
        /// 발동 여부의 값이 1인 경우 발동되는 아이템이고 0인 경우 발동되지 않는다.<br/>
        /// 
        /// 형태) [발동 슬롯 번호, 발동 여부, 발동 슬롯 번호, 발동 여부, 발동 슬롯 번호, 발동 여부, ... ]<br/>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Global.ItemQueueLength)]
        public readonly Byte[] itemQueue;
    }

    /// <summary> 서버가 클라이언트에게 전투 정보를 알리는 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_BattleInfoPacket : IConvertToData
    {
        public readonly UInt16 size;
        public readonly EPacketType type;

        /// <summary>
        /// 전투 순서가 담겨져 있는 배열<br/>
        /// 배열 요소들은 유저의 네트워크 아이디로 이루어져 있다.<br/>
        /// 값이 Int32 최대값이 들어올 경우 뒷 요소들은 플레이어가 더이상 없는 경우이다.<br/>
        /// 
        /// 형태) [전투A유저1, 전투A유저2, 전투B유저1, 전투B유저2, 전투C유저1, 전투C유저2, 전투D유저1, 전투D유저2]<br/>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Global.MaxRoomPlayer)]
        public readonly Int32[] battleOpponents;


        /// <summary> 모든 Room 유저의 아이템 발동 순서가 담겨져 있는 배열 </summary>
        // 전투는 해당 배열만 활용해서 진행해야 한다. 절대 수정하면 안 된다.
        // 만약 배열을 수정하면 다른 클라이언트랑 결과가 달라지게 된다
        // 자세한 내용은 구조체 설명을 참고
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Global.MaxRoomPlayer)]
        public readonly ItemQueueInfo[] itemQueueInfos;

        public void ToData(ReadMemoryStream readStream)
        {
            // todo : 서버 배열 계산 로직 추가 
            this = readStream.ReadByStruct<sc_BattleInfoPacket>();
        }
    }

    /// <summary> 어느쪽으로든 이모티콘을 보냈음을 알리는 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct cs_sc_UseEmoticonPacket : IConvertToBytes, IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;
        public EEmoticonType emoticonType;

        public cs_sc_UseEmoticonPacket(Int32 networkID, EEmoticonType emoticonType)
        {
            size = (UInt16)Marshal.SizeOf<cs_sc_UseEmoticonPacket>();
            type = EPacketType.cs_sc_useEmoticon;
            this.networkID = networkID;
            this.emoticonType = emoticonType;
        }

        public void ToBytes(WriteMemoryStream writeStream)
        {
            writeStream.WriteByUInt16(size);
            writeStream.WriteByByte((byte)type);
            writeStream.WriteByInt32(networkID);
            writeStream.WriteByByte((byte)emoticonType);
        }

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            networkID = readStream.ReadByInt32();
            emoticonType = (EEmoticonType)readStream.ReadByByte();
        }
    }

    /// <summary> 무언가를 알리는 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct cs_sc_NotificationPacket : IConvertToBytes, IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;
        public ENotificationType notificationType;

        public cs_sc_NotificationPacket(Int32 networkID, ENotificationType notificationType)
        {
            size = (UInt16)Marshal.SizeOf<cs_sc_NotificationPacket>();
            type = EPacketType.cs_sc_notification;
            this.networkID = networkID;
            this.notificationType = notificationType;
        }

        public void ToBytes(WriteMemoryStream writeStream)
        {
            writeStream.WriteByUInt16(size);
            writeStream.WriteByByte((byte)type);
            writeStream.WriteByInt32(networkID);
            writeStream.WriteByByte((byte)notificationType);
        }

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            networkID = readStream.ReadByInt32();
            notificationType = (ENotificationType)readStream.ReadByByte();
        }
    }

    /// <summary> 슬롯 아이템 정보 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct SlotItemInfo
    {
        /// <summary> 아이템 타입 </summary>
        public byte type;

        /// <summary> 강화 수치 </summary>
        public byte upgrade;
    };

    /// <summary> 캐릭터 정보를 갱신해주는 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_UpdateCharacterInfoPacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;

        /// <summary> 캐릭터 체력 </summary>
        public Int16 hp;
        /// <summary> 전투 아바타 체력 </summary>
        public Int16 avatarHp;

        /// <summary>usingInventory 아이템 정보 </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Global.MaxUsingItemCount)]
        public SlotItemInfo[] usingInventoryInfos;

        /// <summary>unUsingInventory 아이템 정보 </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Global.MaxUnUsingItemCount)]
        public SlotItemInfo[] unUsingInventoryInfos;

        public void ToData(ReadMemoryStream readStream)
        {
            this = readStream.ReadByStruct<sc_UpdateCharacterInfoPacket>();
        }
    }

    /// <summary> 준비시간을 설정해주는 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_SetReadyTimePacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;

        /// <summary> 설정 준비 시간 </summary>
        public byte time;

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            time = readStream.ReadByByte();
        }
    }

    /// <summary> 아이템 드랍 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct cs_sc_DropItemPacket : IConvertToData, IConvertToBytes
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;

        /// <summary> 드랍할 아이템 인덱스 </summary>
        public byte itemIndex;

        public cs_sc_DropItemPacket(Int32 networkID, byte itemIndex)
        {
            size = (UInt16)Marshal.SizeOf<cs_sc_DropItemPacket>();
            type = EPacketType.cs_sc_dropItem;
            this.networkID = networkID;
            this.itemIndex = itemIndex;
        }

        public void ToBytes(WriteMemoryStream writeStream)
        {
            writeStream.WriteByUInt16(size);
            writeStream.WriteByByte((byte)type);
            writeStream.WriteByInt32(networkID);
            writeStream.WriteByByte(itemIndex);
        }

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            networkID = readStream.ReadByInt32();
            itemIndex = readStream.ReadByByte();
        }
    }

    /// <summary> 아이템 조합 요청 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct cs_RequestCombinationItemPacket : IConvertToBytes
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;

        /// <summary> 조합에 필요한 아이템 인덱스 </summary>
        public byte itemIndex1;
        public byte itemIndex2;
        public byte itemIndex3;

        public cs_RequestCombinationItemPacket(Int32 networkID, byte itemIndex1, byte itemIndex2, byte itemIndex3)
        {
            size = (UInt16)Marshal.SizeOf<cs_RequestCombinationItemPacket>();
            type = EPacketType.cs_requestCombinationItem;
            this.networkID = networkID;
            this.itemIndex1 = itemIndex1;
            this.itemIndex2 = itemIndex2;
            this.itemIndex3 = itemIndex3;
        }

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            networkID = readStream.ReadByInt32();
            itemIndex1 = readStream.ReadByByte();
            itemIndex2 = readStream.ReadByByte();
            itemIndex3 = readStream.ReadByByte();
        }

        public void ToBytes(WriteMemoryStream writeStream)
        {
            writeStream.WriteByUInt16(size);
            writeStream.WriteByByte((byte)type);
            writeStream.WriteByInt32(networkID);
            writeStream.WriteByByte(itemIndex1);
            writeStream.WriteByByte(itemIndex2);
            writeStream.WriteByByte(itemIndex3);
        }
    }

    /// <summary> 캐릭터 선택 시간을 설정해주는 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_SetChoiceCharacterTimePacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;

        /// <summary> 설정 준비 시간 </summary>
        public byte time;

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            time = readStream.ReadByByte();
        }
    }

    /// <summary> 아이템 뽑기권 개수 지정 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_SetItemTicketPacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;
        public EItemTicketType itemTicketType;  // 뽑기권 종류
        public byte count;  // 뽑기권 갯수

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            networkID = readStream.ReadByInt32();
            itemTicketType = (EItemTicketType)readStream.ReadByByte();
            count = readStream.ReadByByte();
        }
    }

    /// <summary> 아이템 발동 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_ActiveItemPacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;
        public byte slot;   // 발동 슬롯 번호

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            networkID = readStream.ReadByInt32();
            slot = readStream.ReadByByte();
        }
    }

    /// <summary> 화면이 서서히 어두어지는 FadeIn 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_FadeInPacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        public byte seconds;    // Fade 시간

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            seconds = readStream.ReadByByte();
        }
    }

    /// <summary> 화면이 서서히 밝아지는 FadeOut 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_FadeOutPacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        public byte seconds;    // Fade 시간

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            seconds = readStream.ReadByByte();
        }
    }

    /// <summary> 전투 상대 순서 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_BattleOpponentsPacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;

        /// <summary> 전투 상대 순서 </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Global.MaxRoomPlayer)]
        public Int32[] battleOpponentQueue;

        public void ToData(ReadMemoryStream readStream)
        {
            this = readStream.ReadByStruct<sc_BattleOpponentsPacket>();
        }
    }

    /// <summary> 햄버거 타입 지정하는 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_SetHamburgerTypePacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;
        /// <summary>
        /// 몇번째 칸에 있는 햄버거인지
        /// </summary>
        public byte slot;
        /// <summary>
        /// 햄버거 타입은 어떤거 인지
        /// </summary>
        public EHamburgerType burgerType;

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            networkID = readStream.ReadByInt32();
            slot = readStream.ReadByByte();
            burgerType = (EHamburgerType)readStream.ReadByByte();
        }
    }

    /// <summary> 비밀스런 마법봉 정보 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_MagicStickInfoPacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;
        /// <summary>
        /// 데미지인지
        /// </summary>
        public bool isDamage;

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            networkID = readStream.ReadByInt32();
            isDamage = readStream.ReadByBoolean();
        }
    }

    /// <summary> 무언가를 알리는 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct cs_RequestUpgradeItemPacket : IConvertToBytes
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;
        /// <summary>
        /// 업그레이드 할 슬롯
        /// </summary>
        public byte slot1;
        /// <summary>
        /// 재료 아이템 슬롯
        /// </summary>
        public byte slot2;

        public cs_RequestUpgradeItemPacket(Int32 networkID, byte slot1, byte slot2)
        {
            size = (UInt16)Marshal.SizeOf<cs_RequestUpgradeItemPacket>();
            type = EPacketType.cs_requestUpgradeItem;
            this.networkID = networkID;
            this.slot1 = slot1;
            this.slot2 = slot2;
        }

        public void ToBytes(WriteMemoryStream writeStream)
        {
            writeStream.WriteByUInt16(size);
            writeStream.WriteByByte((byte)type);
            writeStream.WriteByInt32(networkID);
            writeStream.WriteByByte(slot1);
            writeStream.WriteByByte(slot2);
        }
    }

    /// <summary> 박사의 만능툴 정보 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_DoctorToolInfoPacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;
        /// <summary>
        /// 박사의 만능툴이 있는 슬롯 번호
        /// </summary>
        public byte slot;
        /// <summary>
        /// 교체할 아이템 인덱스 번호
        /// </summary>
        public byte itemType;
        /// <summary>
        /// 교체할 아이템 강화 수치
        /// </summary>
        public byte upgrade;

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            networkID = readStream.ReadByInt32();
            slot = readStream.ReadByByte();
            itemType = readStream.ReadByByte();
            upgrade = readStream.ReadByByte();
        }
    }

    /// <summary> 크립 라운드 정보 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_CreepStageInfoPacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        /// <summary>
        /// 크립 라운드 고멤 타입
        /// </summary>
        public ECreepType creepType;

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            creepType = (ECreepType)readStream.ReadByByte();
        }
    }

    /// <summary>
    /// 전투 아바타 정보 패킷
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_BattleAvatarInfoPacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;
        public byte playerHp;                   //플레이어 실제 체력
        public UInt16 maxHp;                    // 최대 체력
        public UInt16 hp;                       // 체력
        public byte firstAttackState;           // 선공 수치
        public byte offensePower;               // 공격력
        public UInt32 defensive;                // 방어도
        public byte additionDefensive;          // 추가 방어도
        public byte weakening;                  // 약화
        public UInt16 bleeding;                 // 출혈
        public byte reducedHealing;             // 치유량 감소
        public bool isEffectHeal;               // 유물 사용할 때 마다 회복 가능한지
        public byte effectHeal;                 // 유물 사용할 때 마다 회복 수치
        public bool isInstallBomb;              // 언니의 마음 폭탄이 설치되어 있는지
        public byte installBombDamage;          // 언니의 마음 폭탄 데미지
        public bool isIgnoreNextDamage;         // 다음 피해 무시
        public bool canDefendNegativeEffect;    // 부정적인 효과 방어
        public bool isCounterAttack;            // 반격 데미지가 있는지
        public byte counterAttackDamage;        // 반격 데미지 수치
        public bool isCounterHeal;              // 반격 힐이 가능한지
        public byte counterHeal;                // 반격 힐 수치

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            networkID = readStream.ReadByInt32();
            playerHp = readStream.ReadByByte();
            maxHp = readStream.ReadByUInt16();
            hp = readStream.ReadByUInt16();
            firstAttackState = readStream.ReadByByte();
            offensePower = readStream.ReadByByte();
            defensive = readStream.ReadByUInt32();
            additionDefensive = readStream.ReadByByte();
            weakening = readStream.ReadByByte();
            bleeding = readStream.ReadByUInt16();
            reducedHealing = readStream.ReadByByte();
            isEffectHeal = readStream.ReadByBoolean();
            effectHeal = readStream.ReadByByte();
            isInstallBomb = readStream.ReadByBoolean();
            installBombDamage = readStream.ReadByByte();
            isIgnoreNextDamage = readStream.ReadByBoolean();
            canDefendNegativeEffect = readStream.ReadByBoolean();
            isCounterAttack = readStream.ReadByBoolean();
            counterAttackDamage = readStream.ReadByByte();
            isCounterHeal = readStream.ReadByBoolean();
            counterHeal = readStream.ReadByByte();
        }
    }

    /// <summary> 캐릭터 정보를 갱신해주는 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_InventoryInfoPacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        public Int32 networkID;

        /// <summary>usingInventory 아이템 정보 </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Global.MaxUsingItemCount)]
        public SlotItemInfo[] usingInventoryInfos;

        /// <summary>unUsingInventory 아이템 정보 </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Global.MaxUnUsingItemCount)]
        public SlotItemInfo[] unUsingInventoryInfos;

        public void ToData(ReadMemoryStream readStream)
        {
            this = readStream.ReadByStruct<sc_InventoryInfoPacket>();
        }
    }

    /// <summary> 크립 라운드 정보 패킷 </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_MatchingInfoPacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        /// <summary>
        /// 현재 매칭 몇명인지
        /// </summary>
        public byte matchingCount;

        /// <summary>
        /// 현재 몇명 접속 중인지
        /// </summary>
        public UInt16 currConnectionCount;

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            matchingCount = readStream.ReadByByte();
            currConnectionCount = readStream.ReadByUInt16();
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct sc_BattleTimeInfoPacket : IConvertToData
    {
        public UInt16 size;
        public EPacketType type;
        /// <summary>
        /// 현재 몇 초 남았는지지
        /// </summary>
        public byte time;

        public void ToData(ReadMemoryStream readStream)
        {
            size = readStream.ReadByUInt16();
            type = (EPacketType)readStream.ReadByByte();
            time = readStream.ReadByByte();
        }
    }
}
