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
            Debug.Log("ĳ���� ���� ������ ");

            // ������ ���õ� ĳ���� ���� �˷��ֱ�
            C_SelectedCharacter selectedCharacter = new C_SelectedCharacter()
            {
                CharacterInfo = GameManager.Instance.GetCharacterInfo()
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
        if(response.Result == true)
        {
            Debug.Log("���ӿ� �����ϱ�  ");

            // [3] ���Ӿ����� �̵��ϱ� 
            MainThreadDispatcher.RunOnMainThread(() =>
            {
                Debug.Log("OnMainThread");
                MapManager.Instance.EnterGameScene(GameManager.Instance.MapNumber);
            });
        }
        else
        {
            Debug.LogError("[S_SelectedCharacterHandler] ��ū�� ����� ���ŵ��� �ʾҽ��ϴ�. ");
        }
    }

}
