using Packet;
using GamePacket;
using MonsterPacket;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AttackPacket;
using ChatPacket;
using MMG.UI;
using System;

public class PacketHandler : MonoBehaviour
{
    public static void S_LoginTokenHandler(ClientSession session, S_LoginToken response)
    {
        if (response.Result == true)
        {
            Debug.Log("ĳ���� ���� ������ ");

            // ������ ���õ� ĳ���� ���� �˷��ֱ�
            C_SelectedCharacter selectedCharacter = new C_SelectedCharacter()
            {
                CharacterInfo = PlayerData.Instance.MyCharacterInfo()
            };
            NetworkManager.Instance.Send_CharacterInfo(selectedCharacter);
        }
        else
        {
            Debug.LogError("[S_LoginTokenHandler] ��ū�� ����� ���ŵ��� �ʾҽ��ϴ�. ");
        }
    }
    public static void S_SelectedCharacterHandler(ClientSession session, S_SelectedCharacter response)
    {
        if (response.Result == true)
        {
            Debug.Log("���ӿ� �����ϱ�  ");

            MainThreadDispatcher.RunOnMainThread(() =>
            {
                // (!) ä�� ������ �����ϱ� => ���� ���� ���� 
                int mapNumber = PlayerData.Instance.MyCharaceterData.LastMapId ?? MapManager.DEFAULT_MAP_NUMBER;
                ServerConnector.Instance.ConnctChatServer(
                    () => MapManager.Instance.EnterGameScene(mapNumber)
                    );
            });
        }
        else
        {
            Debug.LogError("[S_SelectedCharacterHandler] ��ū�� ����� ���ŵ��� �ʾҽ��ϴ�. ");
        }
    }
    public static void S_EnterGameResponHandler(ClientSession session, S_EnterGameResponse response)
    {
        // ���������� ���°�? 
        // �����ؾ��� ĳ���� ����Ʈ��
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            Debug.Log("ChatServer�� EnterGameRoom ������");
            GameRoom.Instance.Init(response.MapId);
            GameRoom.Instance.SpawnCharacters(response.CharacterList.ToList());
            NetworkManager.Instance.Send_Chat_EnterGame();
        });
    }
    public static void S_Chat_EnterChatRoomHandler(ChatSession session, EnterChatRoom response)
    {
        Debug.Log($"[PacketHandler] : S_Chat_EnterChatRoomHandler {response.NickName}  ");
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            // ChatUI Ȱ��ȭ
            PopupManager.Instance.isConnectChatServer = true;
            if (InGamePopup.Instance != null)
                InGamePopup.Instance.ShowChatUI();
        });
    }
    public static void S_Chat_RoomChat(ChatSession session, S_BroadcastRoomChat response)
    {
        Debug.Log($"[PacketHandler] : RoomChat {response.NickName} : {response.Message}");
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            DateTime chatTime = DateTimeOffset.FromUnixTimeSeconds(response.TimeStamp).UtcDateTime;
            InGamePopup.Instance.ChatContentUI.UserChat(response.NickName, response.Message, chatTime);
        });
    }
    public static void S_Chat_SystemChat(ChatSession session, S_BroadcastRoomChat response)
    {
        Debug.Log($"[PacketHandler] : SystemChat {response.NickName} : {response.Message}");
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            DateTime chatTime = DateTimeOffset.FromUnixTimeSeconds(response.TimeStamp).UtcDateTime;
            InGamePopup.Instance.ChatContentUI.SystemChat(response.Message, chatTime);
        });

    }
    public static void S_Chat_AdminChat(ChatSession session, S_BroadcastRoomChat response)
    {
        Debug.Log($"[PacketHandler] : AdminChat {response.NickName} : {response.Message}");
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            DateTime chatTime = DateTimeOffset.FromUnixTimeSeconds(response.TimeStamp).UtcDateTime;
            InGamePopup.Instance.ChatContentUI.AdminChat(response.Message, chatTime);
        });


    }
    #region GameRoomBroadcast
    public static void S_BroadcastEnterHandler(ClientSession session, S_BroadcastEnter response)
    {
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            Debug.Log($"{response.EnterCharacter.CharacterInfo.CharacterName}�� �����Ͽ����ϴ�. ");
            GameRoom.Instance.HandleBroadcastEnter(response);
        });
    }
    public static void S_BroadcastMovehandler(ClientSession session, S_BroadcastMove response)
    {
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            GameRoom.Instance.HandleBroadcastMove(response);
        });
    }
    public static void S_BroadcastMonsterMovehandler(ClientSession session, S_BroadcastMove response)
    {
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            GameRoom.Instance.HandlerBoradcastMove_Monster(response);
        });
    }
    public static void S_BroadcastDamageHandler(ClientSession session, S_DamageBroadcast response)
    {
        Debug.Log($"[S_BroadcastDamageHandler] {response.Damage.AttackerId}�� {response.Damage.TargetId}�� ����. Damage {response.Damage}");

        // TargetId�� ã�Ƽ� Damage �ִϸ��̼� 
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            GameRoom.Instance.HandlerDamaged(response);
        });
    }
    public static void S_BroadcastAttackHandler(ClientSession session, S_Attack response)
    {
        Debug.Log($"[S_BroadcastAttackHandler] {response.AttackerId}�� ����");
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            GameRoom.Instance.HandlerAttack(response);
        });
    }
    public static void S_MonsterListHandler(ClientSession session, S_MonsterList response)
    {
        Debug.Log($"[S_MonsterListHandler] Monster �� {response.MonsterDataList.Count}");

        MainThreadDispatcher.RunOnMainThread(() =>
        {
            GameRoom.Instance.SpwanMonsters(response);
        });
    }

    public static void S_DeadHandler(ClientSession session, S_DeathBroadcast response)
    {
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            GameRoom.Instance.DeadMonster(response);
        });
    }
    public static void S_BroadcastPlayerDieHandler(ClientSession session, PlayerId response)
    {
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            GameRoom.Instance.PlayerDie(response);
        });
    }
    public static void S_BroadcastPlayerReviveHandler(ClientSession session, S_PlayerRespawn response)
    {
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            GameRoom.Instance.PlayerRespawn(response);
        });
    }
    public static void S_StatusUpdate(ClientSession session, Status response)
    {
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            GameRoom.Instance.MyCharacter.UpdateStatus(response);
        });
    }
    public static void S_BroadcastLevelUp(ClientSession session, S_BroadcastLevelUp response)
    {
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            GameRoom.Instance.Players[response.CharacterId].LevelUp(response.Status);
        });
    }
    #endregion
}
