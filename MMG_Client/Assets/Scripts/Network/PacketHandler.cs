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
            GameSceneLoader.Instance.SpawnCharacters(response.CharacterList.ToList());
        });
    }

    #region GameRoomBroadcast
    public static void S_BroadcastEnterHandler(ClientSession session, S_BroadcastEnter response)
    {
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            Debug.Log($"{response.EnterCharacter.CharacterInfo.CharacterName}�� �����Ͽ����ϴ�. ");
            GameSceneLoader.Instance.SpawnCharacter(response.EnterCharacter);
        });
    }
    #endregion
}
