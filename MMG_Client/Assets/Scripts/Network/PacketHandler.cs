using Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketHandler : MonoBehaviour
{
    public static void S_LoginCheckResponseHandler(ClientSession session, S_LoginCheckResponse response)
    {
        if (response.IsValid) // ��ȿ�� ��ū���� üũ 
        {
            GameManager.Instance.SetUser(response.UserId, response.Email, response.Nickname);
        }
        else
        {
            // �α��� ���� 

            // 1. ����� ��ū ���� (����/��ȿ �� �ٽ� �α��� �ʿ�)
            GameManager.Instance.Logout();
            Debug.LogWarning($"[�α��� ����] ����: {response.Reason}");
        }

        // �̺�Ʈ �߻��ϰ� �ϱ�
        GameManager.Instance.OnLoginCheckComplete?.Invoke(response);

        // SceneLoader.Instance.LoadScene("LoginScene"); // �α��� ������ �̵��ϱ� 
    }
}
