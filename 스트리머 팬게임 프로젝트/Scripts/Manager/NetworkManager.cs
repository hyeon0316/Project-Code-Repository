using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using MemoryStream;
using Packet;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class NetworkManager : Singleton<NetworkManager>
{
    private Socket _socket;
    private IPAddress _serverIp;
    private const string ServerDomain = "zecocostudio.com";
    private const int ServerPort = 47675;
    //private const int ServerPort = 51341;
    private const int ServerLocalPort = 24961;
    private IPEndPoint _serverIpEndPoint;
    private bool _isFinishConnectServer = false;
    private bool _isSuccessConnectServer = false;
    [SerializeField] private TMP_Text _matchingLogText;
    [SerializeField] private TMP_Text _battleTimerText;
    [SerializeField] private GameObject _disconnectPanel;
    [SerializeField] private GameOver _gameOver;
    private const int MaxPacketSize = 1500;

    void OnApplicationQuit()
    {
        DisconnectServer();
    }

    // 서버 컴퓨터 서버
    public void StartConnectServer()
    {
        ConnectServer(ServerPort);
    }

    // 개인 컴퓨터 서버
    public void StartConnectLocalServer()
    {
        ConnectServer(ServerLocalPort);
    }

    private void ConnectServer(int port)
    {
        try
        {
            _serverIp = Dns.GetHostEntry(ServerDomain).AddressList[0];
            //_serverIp = IPAddress.Parse("192.168.219.100");
            //_serverIp = IPAddress.Parse("172.30.1.54");
        }
        catch (Exception)
        {
            Debug.LogError("해당 도메인을 찾을 수 없습니다");
            _matchingLogText.text = "서버와 연결이 실패하였습니다. 카페에서 새로운 버전의 패치가 있는지 확인해주세요.";
            return;
            throw;
        }

        _serverIpEndPoint = new IPEndPoint(_serverIp, port);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
        _socket.SendTimeout = 2000;
        _socket.ReceiveTimeout = 2000;
        _isFinishConnectServer = false;
        StartCoroutine(LoadingConnectServerCoroutine());
        _socket.BeginConnect(_serverIpEndPoint, new AsyncCallback(Connected), _socket);
    }

    private IEnumerator LoadingConnectServerCoroutine()
    {
        string text = "서버에 연결 시도 중";
        while (!_socket.Connected && !_isFinishConnectServer)
        {
            text += ".";
            Debug.Log(text);
            _matchingLogText.text = text;
            yield return new WaitForSeconds(1);
        }

        if (!_isSuccessConnectServer)
        {
            _matchingLogText.text = "서버와 연결이 실패하였습니다. 카페에서 새로운 버전의 패치가 있는지 확인해주세요.";
        }
    }

    private void Connected(IAsyncResult iar)
    {
        _socket = (Socket)iar.AsyncState;
        try
        {
            _socket.EndConnect(iar);
            _isSuccessConnectServer = true;
        }
        catch (SocketException)
        {
            Debug.Log("연결 실패");
        }
        finally
        {
            _isFinishConnectServer = true;
        }
    }

    private void Update()
    {
        Receive();
        if (_isSuccessConnectServer)
        {
            StopAllCoroutines();
            _isSuccessConnectServer = false;
            StartCoroutine(SendConnectCheckPacketCoroutine());
            Invoke("SendStartMatchingPacket", 1);
        }
    }

    void SendPacket(object packet)
    {
        if (packet == null)
        {
            Debug.LogError("패킷이 없습니다");
        }

        try
        {
            byte[] sendPacket = StructToByteArray(packet);
            _socket.Send(sendPacket, 0, sendPacket.Length, SocketFlags.None);
            Debug.Log($"{sendPacket.Length.ToString()}byte 사이즈 데이터 전송");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
            if (!_socket.Connected && !_gameOver.IsGameFinish)
            {
                _disconnectPanel.SetActive(true);
            }
        }
    }

    private void SendStartMatchingPacket()
    {
        cs_StartMatchingPacket packet;
        packet.size = (UInt16)Marshal.SizeOf<cs_StartMatchingPacket>();
        packet.type = EPacketType.cs_startMatching;
        packet.networkID = PlayerManager.Instance.Players[0].ID;
        byte[] nameBuf = new byte[22];
        var encodingStrBytes = Encoding.Unicode.GetBytes(PlayerManager.Instance.Players[0].NickName.Trim((char)8203));
        Array.Copy(encodingStrBytes, nameBuf, encodingStrBytes.Length);
        nameBuf[20] = 0;
        nameBuf[21] = 0;
        packet.name = nameBuf;
        SendPacket(packet);
    }

    public void SendChangeCharacterPacket(int networkID, int characterType)
    {
        cs_sc_ChangeCharacterPacket packet = new cs_sc_ChangeCharacterPacket(networkID, (ECharacterType)characterType);
        SendPacket(packet);
    }


    /// <summary>
    /// 업그레이드
    /// </summary>
    /// <param name="networkID"></param>
    /// <param name="slot1">재료 아이템 슬롯</param>
    /// <param name="slot2">업그레이드 할 슬롯</param>
    public void SendUpgradeItemPacket(int networkID, Byte slot1, Byte slot2)
    {
        var packet = new cs_RequestUpgradeItemPacket(networkID, slot2, slot1);
        SendPacket(packet);
    }

    public void SendChangeItemSlotPacket(Int32 networkID, Byte slot1, Byte slot2)
    {
        if (slot1 == slot2)
        {
            PlayerManager.Instance.GetPlayer(networkID).GetItemSlot(slot1).SetAlpha(1);
            Debug.Log("SendChangeItemSlotPacket : 둘이 같은 슬롯");
            return;
        }

        var packet = new cs_sc_ChangeItemSlotPacket(networkID, slot1, slot2);
        SendPacket(packet);
    }

    public void SendDropItemPacket(Int32 networkID, byte itemIndex)
    {
        cs_sc_DropItemPacket packet = new cs_sc_DropItemPacket(networkID, itemIndex);
        SendPacket(packet);
    }


    public void SendRequestNormalItemTicket(Int32 networkID)
    {
        var packet = new cs_sc_NotificationPacket(networkID, ENotificationType.UseNormalItemTicket);
        SendPacket(packet);
    }

    public void SendRequestAdvancedItemTicket(Int32 networkID)
    {
        var packet = new cs_sc_NotificationPacket(networkID, ENotificationType.UseAdvancedItemTicket);
        SendPacket(packet);
    }
    public void SendRequestTopItemTicket(Int32 networkID)
    {
        var packet = new cs_sc_NotificationPacket(networkID, ENotificationType.UseTopItemTicket);
        SendPacket(packet);
    }

    public void SendRequestSupremeItemTicket(Int32 networkID)
    {
        var packet = new cs_sc_NotificationPacket(networkID, ENotificationType.UseSupremeItemTicket);
        SendPacket(packet);
    }

    private bool _isEmote;
    public void SendUseEmoticonPacket(Int32 networkID, EEmoticonType emoticonType)
    {
        if (_isEmote)
            return;

        cs_sc_UseEmoticonPacket packet = new cs_sc_UseEmoticonPacket(networkID, emoticonType);
        SendPacket(packet);
        StartCoroutine(CheckUseEmotion(4));
    }

    //public void Send
    public void SendChoiceCharacterPacket(Int32 networkID)
    {
        cs_sc_NotificationPacket packet = new cs_sc_NotificationPacket(networkID, ENotificationType.ChoiceCharacter);
        SendPacket(packet);
    }

    public void SendRequestCombinationItemPacket(Int32 networkID, byte itemIndex1, byte itemIndex2, byte itemIndex3)
    {
        cs_RequestCombinationItemPacket packet = new cs_RequestCombinationItemPacket(networkID, itemIndex1, itemIndex2, itemIndex3);
        SendPacket(packet);
    }

    private IEnumerator CheckUseEmotion(float coolDown)
    {
        _isEmote = true;
        float timer = coolDown;
        while (true)
        {
            if (timer <= 0)
            {
                _isEmote = false;
                break;
            }

            timer -= Time.deltaTime;
            yield return null;
        }
    }


    private IEnumerator SendConnectCheckPacketCoroutine()
    {
        WaitForSeconds waitTenSeconds = new WaitForSeconds(10f);
        while (!(_socket == null || !_socket.Connected))
        {
            cs_sc_NotificationPacket packet = new cs_sc_NotificationPacket(PlayerManager.Instance.Players[0].ID, ENotificationType.ConnectCheck);
            SendPacket(packet);
            Debug.Log("ConnectCheckPacket 보냄");
            yield return waitTenSeconds;
        }

        // 서버 끊겼을 때


    }

    void Receive()
    {
        int receive = 0;
        if (_socket == null || !_socket.Connected)
        {
            return;
        }

        if (_socket.Available == 0)
        {
            return;
        }

        byte[] buffer = new byte[MaxPacketSize];
        try
        {
            receive = _socket.Receive(buffer);
        }
        catch (Exception)
        {
            //Debug.Log(ex.ToString());
            return;
        }

        if (receive <= 0)
        {
            return;
        }

        byte[] packetBuffer = new byte[receive];
        Array.Copy(buffer, packetBuffer, receive);
        ReadMemoryStream readStream = new ReadMemoryStream(packetBuffer);

        Debug.Log($"총 패킷({receive}byte)을 받았습니다");

        while (readStream.CanMakePacket)
        {
            Debug.Log($"남은 byte : {readStream.RestByte}byte");
            EPacketType packetType = readStream.GetPacketType();
            Debug.Log($"{packetType} 패킷({readStream.CurPacketSize}byte)을 받았습니다");

            try
            {
                switch (packetType)
                {
                    case EPacketType.sc_connectRoom:
                        {
                            sc_ConnectRoomPacket packet = new sc_ConnectRoomPacket();
                            packet.ToData(readStream);
                            Debug.Log("room에 입장");
                            PlayerManager.Instance.CreateEnemy(packet.users);
                            WindowManager.Instance.SetWindow(WindowType.Select);
                            WindowManager.Instance.GetSelect().SetUserInfo(packet.users);
                            foreach (Player player in PlayerManager.Instance.Players)
                            {
                                player.UsingInventory.InitItem();
                                player.UnUsingInventory.InitItem();
                            }
                        }
                        break;
                    case EPacketType.sc_addNewItem:
                        {
                            sc_AddNewItemPacket packet = new sc_AddNewItemPacket();
                            packet.ToData(readStream);
                            Player player = PlayerManager.Instance.GetPlayer(packet.networkID);
                            player.AddNewItemNetwork(packet.itemSlot, (EItemCode)packet.itemCode);
                            Debug.Log($"{packet.networkID} 번 유저가 새로운 아이템 {packet.itemCode} 을 추가하였습니다");
                        }
                        break;
                    case EPacketType.cs_sc_changeItemSlot:
                        {
                            cs_sc_ChangeItemSlotPacket packet = new cs_sc_ChangeItemSlotPacket();
                            packet.ToData(readStream);

                        }
                        break;
                    case EPacketType.sc_upgradeItem:
                        {
                            sc_UpgradeItemPacket packet = new sc_UpgradeItemPacket();
                            packet.ToData(readStream);

                        }
                        break;
                    case EPacketType.cs_sc_changeCharacter:
                        {
                            cs_sc_ChangeCharacterPacket packet = new cs_sc_ChangeCharacterPacket();
                            packet.ToData(readStream);
                            Debug.Log($"{packet.networkID} -> {(int)packet.characterType}");
                            WindowManager.Instance.GetSelect()
                                .ChangeCharacterImage(packet.networkID, (int)packet.characterType);
                        }
                        break;
                    case EPacketType.sc_battleOpponents:
                        {
                            sc_BattleOpponentsPacket packet = new sc_BattleOpponentsPacket();
                            packet.ToData(readStream);

                            WindowManager.Instance.GetInGame().BattleWindow.BattlePlayerList.Clear();
                            foreach (var player in packet.battleOpponentQueue)
                            {
                                if (player == Int32.MaxValue)
                                    break;

                                if (player < 0)
                                {
                                    WindowManager.Instance.GetInGame().BattleWindow.BattlePlayerList.Add(player);
                                    continue;
                                }

                                if (PlayerManager.Instance.GetPlayer(player).IsDisconnect)
                                {
                                    continue;
                                }

                                WindowManager.Instance.GetInGame().BattleWindow.BattlePlayerList.Add(player);

                                //todo: 안끊어진 플레이어만 보내줌
                            }
                        }
                        break;
                    case EPacketType.sc_setHamburgerType:
                        {
                            sc_SetHamburgerTypePacket packet = new sc_SetHamburgerTypePacket();
                            packet.ToData(readStream);

                            WindowManager.Instance.GetInGame().BattleWindow
                                .FindHamburgerNetwork(packet.networkID, packet.slot, packet.burgerType);
                        }
                        break;
                    case EPacketType.sc_DoctorToolInfo:
                        {
                            sc_DoctorToolInfoPacket packet = new sc_DoctorToolInfoPacket();
                            packet.ToData(readStream);

                            WindowManager.Instance.GetInGame().BattleWindow
                                .FindDoctorToolNetwork(packet.networkID, packet.slot, (EItemCode)packet.itemType,
                                    packet.upgrade);
                        }
                        break;
                    case EPacketType.sc_magicStickInfo:
                        {
                            sc_MagicStickInfoPacket packet = new sc_MagicStickInfoPacket();
                            packet.ToData(readStream);
                            WindowManager.Instance.GetInGame().BattleWindow
                                .FindBattleItem(packet.networkID, EItemCode.SecretWand, packet.isDamage);
                        }
                        break;
                    case EPacketType.cs_sc_notification:
                        {
                            cs_sc_NotificationPacket packet = new cs_sc_NotificationPacket();
                            packet.ToData(readStream);
                            switch (packet.notificationType)
                            {
                                case ENotificationType.ConnectServer:
                                    PlayerManager.Instance.Players[0].SetID(packet.networkID);
                                    Debug.Log($"플레이어 네트워크ID : {packet.networkID} 로 접속");
                                    break;
                                case ENotificationType.ChoiceCharacter:
                                    WindowManager.Instance.GetSelect().SelectDone(packet.networkID);
                                    Debug.Log($"플레이어 네트워크ID : {packet.networkID}가 캐릭터 선택을 완료하였습니다");
                                    break;
                                case ENotificationType.ChoiceAllCharacter:
                                    WindowManager.Instance.GetSelect().ReadyAllPlayer();
                                    break;
                                case ENotificationType.DisconnectServer: //나갈때 or 죽을때 타는 케이스문
                                    if (packet.networkID == PlayerManager.Instance.Players[0].ID) // 연결이 해제된 클라이언트가 나라면
                                    {
                                        if (!PlayerManager.Instance.Players[0].IsDisconnect && !InGame.IsEndGame)
                                        {
                                            Invoke("DisconnectServer", 1);
                                            WindowManager.Instance.GetInGame().GameOverController.ShowGameOver(GameOverType.Lose);
                                            WindowManager.Instance.GetInGame().BattleWindow.SetBattle(); //다음 게임을 위해 다시 초기 셋팅
                                            PlayerManager.Instance.Players[0].IsDisconnect = true;
                                            Debug.Log("연결이 해제되었습니다");
                                        }
                                    }
                                    else // 다른 클라이언트라면
                                    {
                                        if (WindowManager.Instance.GetSelect().gameObject.activeSelf)
                                        {
                                            DisconnectServer();
                                            WindowManager.Instance.GetSelect().CancelSelect();
                                            DataManager.Instance.FadeManager.StopFade();
                                            DataManager.Instance.FadeManager.SetRadius(1);
                                            WindowManager.Instance.SetActiveCamera(true);
                                            WindowManager.Instance.GetLobby().SetActiveSettingPanel(true);
                                            SceneManager.UnloadSceneAsync("CutScene");
                                        }
                                        else
                                        {
                                            if (!PlayerManager.Instance.GetPlayer(packet.networkID).IsDisconnect)
                                            {
                                                if (InGame.CurGameType == EGameType.Ready) //준비단계에서 나갔을떄
                                                {
                                                    PlayerManager.Instance.GetPlayer(packet.networkID).UpdateHp(-999);
                                                    WindowManager.Instance.GetInGame().SelectWinner();
                                                    PlayerManager.Instance.GetPlayer(packet.networkID).IsDisconnect = true;
                                                    Debug.Log(
                                                        $"[DisconnectServer/EGameType.Ready] {packet.networkID}의 플레이어 체력을 {PlayerManager.Instance.GetPlayer(packet.networkID).Hp}로 설정");
                                                }
                                                else //전투단계
                                                {
                                                    //todo: 전투에서 죽었을때or나갔을때 준비로 오면 결과 출력
                                                    PlayerManager.Instance.GetPlayer(packet.networkID).UpdateHp(-999);
                                                    WindowManager.Instance.GetInGame().SelectWinner();
                                                    PlayerManager.Instance.GetPlayer(packet.networkID).IsDisconnect = true;
                                                    Debug.Log($"[DisconnectServer/EGameType.Battle] {packet.networkID}의 플레이어 체력을 {PlayerManager.Instance.GetPlayer(packet.networkID).Hp}로 설정");
                                                }
                                            }
                                        }
                                        Debug.Log($"{packet.networkID}의 접속이 해제되었습니다.");
                                    }

                                    break;
                                case ENotificationType.EnterReadyStage:
                                    Debug.Log("준비 스테이지 입장");
                                    WindowManager.Instance.GetInGame().OpenReady();
                                    WindowManager.Instance.GetInGame().BattleWindow.InitBattleField();
                                    WindowManager.Instance.GetInGame().BattleWindow.DeleteBattleMap();
                                    WindowManager.Instance.GetInGame().Round.ProceedRound();
                                    break;
                                case ENotificationType.EnterCutSceneStage:
                                    Debug.Log("컷신 스테이지 입장");
                                    //todo: 셋팅창 잠깐 끄기
                                    WindowManager.Instance.GetLobby().SetActiveSettingPanel(false);
                                    WindowManager.Instance.SetActiveCamera(false);
                                    WindowManager.Instance.GetInGame().PlayersMap.InitPlayersInfo();
                                    WindowManager.Instance.GetSelect().SetPlayersDelay();
                                    SceneManager.LoadScene("CutScene", LoadSceneMode.Additive);
                                    DataManager.Instance.FadeCanvas.transform.SetAsLastSibling();
                                    break;
                                case ENotificationType.FinishCutSceneStage:
                                    Debug.Log("컷신 스테이지 퇴장");
                                    WindowManager.Instance.GetLobby().SetActiveSettingPanel(true);
                                    WindowManager.Instance.SetActiveCamera(true);
                                    WindowManager.Instance.SetWindow(WindowType.InGame);
                                    WindowManager.Instance.GetInGame().Round.Init();
                                    SceneManager.UnloadSceneAsync("CutScene");
                                    break;
                                case ENotificationType.EnterBattleStage:
                                    Debug.Log("전투 스테이지 입장");
                                    if (!InGame.IsEndGame)
                                    {
                                        SoundManager.Instance.PlayEffect(EffectType.EntryBattle);
                                        WindowManager.Instance.GetInGame().BattleWindow.SetBattleRound();
                                        WindowManager.Instance.GetInGame().OpenBattle();
                                    }
                                    break;
                                case ENotificationType.EnterCreepStage:
                                    Debug.Log("크립 스테이지 입장");
                                    if (!InGame.IsEndGame)
                                    {
                                        WindowManager.Instance.GetInGame().OpenBattle();
                                    }

                                    break;
                                case ENotificationType.FinishChoiceCharacterTime:
                                    WindowManager.Instance.GetSelect().SetCharacterButtons(false);
                                    Debug.Log("캐릭터 선택 시간 종료");
                                    break;
                                case ENotificationType.InitBattleSlot:
                                    WindowManager.Instance.GetInGame().BattleWindow.SetNextRound();
                                    break;
                                case ENotificationType.SetAutoUsingItem:
                                    SoundManager.Instance.PlayEffect(EffectType.ItemEquip);
                                    break;
                                case ENotificationType.EffectCreepItem:
                                    if (!InGame.IsEndGame)
                                    {
                                        WindowManager.Instance.GetInGame().BattleWindow.FindActiveCreepItemNetwork();
                                    }

                                    break;
                                case ENotificationType.ConnectCheck:
                                case ENotificationType.UseNormalItemTicket:
                                case ENotificationType.UseAdvancedItemTicket:
                                case ENotificationType.UseTopItemTicket:
                                case ENotificationType.UseSupremeItemTicket:
                                    Debug.LogError($"[{packet.notificationType}] 받으면 안되는 패킷");
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                        break;
                    case EPacketType.sc_setItemTicket:
                        {
                            sc_SetItemTicketPacket packet = new sc_SetItemTicketPacket();
                            packet.ToData(readStream);

                            switch (packet.itemTicketType)
                            {
                                case EItemTicketType.Normal:
                                    if (InGame.CurGameType == EGameType.Battle) //배틀일때 심리상담가 아이템 효과
                                        WindowManager.Instance.GetInGame().BattleWindow
                                            .FindBattleItem(packet.networkID, EItemCode.CounselLicense, true);
                                    PlayerManager.Instance.GetPlayer(packet.networkID)
                                        .SetRouletteCountNetwork(RouletteType.Normal, packet.count);
                                    break;
                                case EItemTicketType.Advanced:
                                    PlayerManager.Instance.GetPlayer(packet.networkID)
                                        .SetRouletteCountNetwork(RouletteType.Advanced, packet.count);
                                    break;
                                case EItemTicketType.Top:
                                    PlayerManager.Instance.GetPlayer(packet.networkID)
                                        .SetRouletteCountNetwork(RouletteType.Top, packet.count);
                                    break;
                                case EItemTicketType.Supreme:
                                    PlayerManager.Instance.GetPlayer(packet.networkID)
                                        .SetRouletteCountNetwork(RouletteType.Supreme, packet.count);
                                    break;
                            }
                        }
                        break;
                    case EPacketType.cs_sc_useEmoticon:
                        {
                            cs_sc_UseEmoticonPacket packet = new cs_sc_UseEmoticonPacket();
                            packet.ToData(readStream);

                            if (DataManager.Instance.IsEmotionBlock)
                            {
                                if (PlayerManager.Instance.Players[0].ID != PlayerManager.Instance.GetPlayer(packet.networkID).ID)
                                {
                                    return;
                                }
                            }

                            PlayerManager.Instance.GetPlayer(packet.networkID).UseEmotionNetwork(packet.emoticonType);
                            WindowManager.Instance.GetInGame().PlayersMap.ShowEmotion(packet.networkID, packet.emoticonType);

                        }
                        break;
                    case EPacketType.sc_updateCharacterInfo:
                        {
                            sc_UpdateCharacterInfoPacket packet = new sc_UpdateCharacterInfoPacket();
                            packet.ToData(readStream);

                            // 패킷 데이터 가지고 캐릭터 업데이트
                            Player player = PlayerManager.Instance.GetPlayer(packet.networkID);
                            player.Hp = packet.hp;

                            for (int i = 0; i < player.UnUsingInventory.ItemSlots.Length; i++)
                            {
                                player.UnUsingInventory.ItemSlots[i].DeleteItem();

                                if (packet.unUsingInventoryInfos[i].type != 0)
                                    player.UnUsingInventory.ItemSlots[i].AddNewItem((EItemCode)packet.unUsingInventoryInfos[i].type, packet.unUsingInventoryInfos[i].upgrade);
                            }

                            for (int i = 0; i < player.UsingInventory.ItemSlots.Length; i++)
                            {
                                player.UsingInventory.ItemSlots[i].DeleteItem();

                                if (packet.usingInventoryInfos[i].type != 0)
                                    player.UsingInventory.ItemSlots[i].AddNewItem((EItemCode)packet.usingInventoryInfos[i].type, packet.usingInventoryInfos[i].upgrade);
                            }

                            player.UpdateEquipEffect();
                            WindowManager.Instance.GetInGame().PlayersMap.UpdatePlayersHp(player);
                        }
                        break;
                    case EPacketType.sc_setChoiceCharacterTime:
                        {
                            sc_SetChoiceCharacterTimePacket packet = new sc_SetChoiceCharacterTimePacket();
                            packet.ToData(readStream);
                            WindowManager.Instance.GetSelect().SetTimer(packet.time);
                            // 로직
                            Debug.Log($"캐릭터 선택 시간이 {packet.time}sec 으로 설정");
                        }
                        break;
                    case EPacketType.sc_setReadyTime:
                        {
                            sc_SetReadyTimePacket packet = new sc_SetReadyTimePacket();
                            packet.ToData(readStream);
                            WindowManager.Instance.GetInGame().ReadyWindow.SetTimer(packet.time);
                            Debug.Log($"준비 시간이 {packet.time}sec 으로 설정");
                        }
                        break;
                    case EPacketType.cs_sc_dropItem:
                        {
                            cs_sc_DropItemPacket packet = new cs_sc_DropItemPacket();
                            packet.ToData(readStream);
                        }
                        break;
                    case EPacketType.sc_fadeIn:
                        {
                            sc_FadeInPacket packet = new sc_FadeInPacket();
                            packet.ToData(readStream);
                            DataManager.Instance.FadeManager.FadeIn(packet.seconds);
                        }
                        break;
                    case EPacketType.sc_fadeOut:
                        {
                            sc_FadeOutPacket packet = new sc_FadeOutPacket();
                            packet.ToData(readStream);
                            DataManager.Instance.FadeManager.FadeOut(packet.seconds);
                        }
                        break;
                    case EPacketType.sc_activeItem:
                        {
                            sc_ActiveItemPacket packet = new sc_ActiveItemPacket();
                            packet.ToData(readStream);
                            WindowManager.Instance.GetInGame().BattleWindow.FindActiveItemNetwork(packet.networkID, packet.slot);
                        }
                        break;
                    case EPacketType.sc_CreepStageInfo:
                        {
                            sc_CreepStageInfoPacket packet = new sc_CreepStageInfoPacket();
                            packet.ToData(readStream);
                            WindowManager.Instance.GetInGame().BattleWindow.SetCreepRound(packet.creepType);
                        }
                        break;
                    case EPacketType.sc_BattleAvatarInfo:
                        {
                            sc_BattleAvatarInfoPacket packet = new sc_BattleAvatarInfoPacket();
                            packet.ToData(readStream);
                            Debug.Log($"[sc_BattleAvatarInfo] {packet.networkID}의 플레이어 체력을 {packet.playerHp}로 설정");
                            Debug.Log($"{packet.networkID}플레이어의 최대 체력{packet.maxHp} 과  플레이어의 체력{packet.hp}");
                            WindowManager.Instance.GetInGame().BattleWindow.UpdateAvatarInfoNetwork(packet);
                        }
                        break;

                    case EPacketType.sc_InventoryInfo:
                        {
                            sc_InventoryInfoPacket packet = new sc_InventoryInfoPacket();
                            packet.ToData(readStream);

                            Player player = PlayerManager.Instance.GetPlayer(packet.networkID);

                            for (var i = 0; i < player.UsingInventory.ItemSlots.Length; i++)
                            {
                                var usingSlot = player.UsingInventory.ItemSlots[i];
                                usingSlot.DeleteItem();

                                if (packet.usingInventoryInfos[i].type != 0)
                                    usingSlot.AddNewItem((EItemCode)packet.usingInventoryInfos[i].type, packet.usingInventoryInfos[i].upgrade);
                            }

                            for (var i = 0; i < player.UnUsingInventory.ItemSlots.Length; i++)
                            {
                                var unUsingSlot = player.UnUsingInventory.ItemSlots[i];
                                unUsingSlot.DeleteItem();

                                if (packet.unUsingInventoryInfos[i].type != 0)
                                    unUsingSlot.AddNewItem((EItemCode)packet.unUsingInventoryInfos[i].type, packet.unUsingInventoryInfos[i].upgrade);

                            }

                            player.UpdateEquipEffect();
                            player.DelayCheckRoulette();
                        }
                        break;
                    case EPacketType.cs_requestCombinationItem:
                    case EPacketType.cs_startMatching:
                    case EPacketType.cs_requestUpgradeItem:
                        Debug.LogError($"{packetType} 받으면 안되는 패킷을 받음");
                        break;
                    case EPacketType.sc_battleInfo:
                        break;
                    case EPacketType.sc_matchingInfo:
                        {
                            sc_MatchingInfoPacket packet = new sc_MatchingInfoPacket();
                            packet.ToData(readStream);
                            Debug.Log(packet.matchingCount);
                            _matchingLogText.text = $"현재 매칭 인원 : {packet.matchingCount}명";
                        }
                        break;
                    case EPacketType.sc_BattleTimeInfo:
                        {
                            sc_BattleTimeInfoPacket packet = new sc_BattleTimeInfoPacket();
                            packet.ToData(readStream);
                            _battleTimerText.text = packet.time.ToString();
                            Debug.Log($"{packet.time}초 남았습니다");
                        }
                        break;
                    default:
                        Debug.LogError($"{(int)packetType} 이상한 패킷 타입을 받음");
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {

            }
        }

        if (readStream.RestByte > 0)
        {
            Debug.LogError($"못 읽은 패킷 {readStream.RestByte} Byte 있음");
        }
    }

    public void DisconnectServer()
    {
        if (_socket == null)
            return;

        if (_socket.Connected)
        {
            _socket.Close();
            _socket = null;
        }
    }


    byte[] StructToByteArray(object obj)
    {
        int size = Marshal.SizeOf(obj);
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(obj, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    T ByteArrayToStruct<T>(byte[] buffer) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        if (size > buffer.Length)
        {
            throw new Exception();
        }

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(buffer, 0, ptr, size);
        T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
        Marshal.FreeHGlobal(ptr);
        return obj;
    }
}
