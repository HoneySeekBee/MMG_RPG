using Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketHandler : MonoBehaviour
{
    public static void S_LoginCheckResponseHandler(ClientSession session, S_LoginCheckResponse response)
    {
        if (response.IsValid) // 유효한 토큰인지 체크 
        {
            GameManager.Instance.SetUser(response.UserId, response.Email, response.Nickname);
        }
        else
        {
            // 로그인 실패 

            // 1. 저장된 토큰 제거 (만료/무효 → 다시 로그인 필요)
            GameManager.Instance.Logout();
            Debug.LogWarning($"[로그인 실패] 이유: {response.Reason}");
        }

        // 이벤트 발생하게 하기
        GameManager.Instance.OnLoginCheckComplete?.Invoke(response);

        // SceneLoader.Instance.LoadScene("LoginScene"); // 로그인 씬으로 이동하기 
    }
}
