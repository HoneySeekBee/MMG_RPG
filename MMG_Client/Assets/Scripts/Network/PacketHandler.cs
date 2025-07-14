using Packet;
using GamePacket;
using MonsterPacket;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

            // [3] ���Ӿ����� �̵��ϱ� 
            MainThreadDispatcher.RunOnMainThread(() =>
            {
                Debug.Log("OnMainThread");
                int mapNumber = PlayerData.Instance.MyCharaceterData.LastMapId ?? MapManager.DEFAULT_MAP_NUMBER;
                MapManager.Instance.EnterGameScene(mapNumber);
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
            GameRoom.Instance.Init(response.MapId);
            GameRoom.Instance.SpawnCharacters(response.CharacterList.ToList());
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
        Debug.Log($"{response.AttackerId}�� {response.TargetId}�� ����. Damage {response.Damage}");
        // AttackerId�� ã�Ƽ� ���� �ִϸ��̼�
        // TargetId�� ã�Ƽ� Damage �ִϸ��̼� 
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            GameRoom.Instance.HandlerBattle(response);
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
    #endregion
}
