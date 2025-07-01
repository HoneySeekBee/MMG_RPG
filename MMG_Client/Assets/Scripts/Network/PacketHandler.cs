using Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketHandler : MonoBehaviour
{
    public static void S_LoginTokenHandler(ClientSession session, S_LoginToken response)
    {
        if(response.Result == true)
        {
            Debug.Log("캐릭터 정보 보내기 ");

            // 서버에 선택된 캐릭터 정보 알려주기
            C_SelectedCharacter selectedCharacter = new C_SelectedCharacter()
            {
                CharacterInfo = GameManager.Instance.GetCharacterInfo()
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
        if(response.Result == true)
        {
            Debug.Log("게임에 입장하기  ");

            // [3] 게임씬으로 이동하기 
            MainThreadDispatcher.RunOnMainThread(() =>
            {
                Debug.Log("OnMainThread");
                MapManager.Instance.EnterGameScene(GameManager.Instance.MapNumber);
            });
        }
        else
        {
            Debug.LogError("[S_SelectedCharacterHandler] 토큰이 제대로 수신되지 않았습니다. ");
        }
    }

}
