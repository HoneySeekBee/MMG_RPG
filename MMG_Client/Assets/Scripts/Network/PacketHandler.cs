using Packet;
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
            GameSceneLoader.Instance.SpawnCharacters(response.CharacterList.ToList());
        });
    }

    #region GameRoomBroadcast
    public static void S_BroadcastEnterHandler(ClientSession session, S_BroadcastEnter response)
    {
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            Debug.Log($"{response.EnterCharacter.CharacterInfo.CharacterName}이 입장하였습니다. ");
            GameSceneLoader.Instance.SpawnCharacter(response.EnterCharacter);
        });
    }
    #endregion
}
