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
            Debug.Log("캐릭터 정보 보내기 ");

            // 서버에 선택된 캐릭터 정보 알려주기
            C_SelectedCharacter selectedCharacter = new C_SelectedCharacter()
            {
                CharacterInfo = PlayerData.Instance.MyCharacterInfo()
            };
            NetworkManager.Instance.Send_CharacterInfo(selectedCharacter);
        }
        else
        {
            Debug.LogError("[S_LoginTokenHandler] 토큰이 제대로 수신되지 않았습니다. ");
        }
    }
    public static void S_SelectedCharacterHandler(ClientSession session, S_SelectedCharacter response)
    {
        if (response.Result == true)
        {
            Debug.Log("게임에 입장하기  ");

            // [3] 게임씬으로 이동하기 
            MainThreadDispatcher.RunOnMainThread(() =>
            {
                Debug.Log("OnMainThread");
                int mapNumber = PlayerData.Instance.MyCharaceterData.LastMapId ?? MapManager.DEFAULT_MAP_NUMBER;
                MapManager.Instance.EnterGameScene(mapNumber);
            });
        }
        else
        {
            Debug.LogError("[S_SelectedCharacterHandler] 토큰이 제대로 수신되지 않았습니다. ");
        }
    }
    public static void S_EnterGameResponHandler(ClientSession session, S_EnterGameResponse response)
    {
        // 리스폰으로 오는것? 
        // 생성해야할 캐릭터 리스트들

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
            Debug.Log($"{response.EnterCharacter.CharacterInfo.CharacterName}이 입장하였습니다. ");
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
        Debug.Log($"{response.AttackerId}가 {response.TargetId}를 공격. Damage {response.Damage}");
        // AttackerId를 찾아서 공격 애니메이션
        // TargetId를 찾아서 Damage 애니메이션 
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            GameRoom.Instance.HandlerBattle(response);
        });
    }

    public static void S_MonsterListHandler(ClientSession session, S_MonsterList response)
    {
        Debug.Log($"[S_MonsterListHandler] Monster 수 {response.MonsterDataList.Count}");

        MainThreadDispatcher.RunOnMainThread(() =>
        {
            GameRoom.Instance.SpwanMonsters(response);
        });
    }
    #endregion
}
