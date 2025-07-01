using MMG.UI;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class LoginPopup : PopupBase
{
    [SerializeField] private TMP_InputField ID_InputField;
    [SerializeField] private TMP_InputField Password_InputField;
    bool isCheck = false;
    public void TryLogin()
    {
        if (isCheck)
            return;
        StartCoroutine(Login());
    }
    public IEnumerator Login()
    {
        isCheck = true;
        string email = ID_InputField.text?.Trim();
        string password = Password_InputField.text?.Trim();

        if (string.IsNullOrEmpty(email))
        {
            Debug.LogWarning("아이디를 입력해주세요");
            isCheck = false;
            yield break;
        }

        if (string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("비밀번호를 입력해주세요");
            isCheck = false;
            yield break;
        }

        string url = NetworkManager.MMG_API_URL + "/api/auth/login";

        LoginDto loginDto = new LoginDto { Email = email, Password = password };
        string jsonData = JsonUtility.ToJson(loginDto);

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;

            // 서버에서 {"token":"..."} 형식으로 주는 경우 파싱
            var response = JsonConvert.DeserializeObject<TokenResponseWrapper>(json);
            if (!string.IsNullOrEmpty(response.token))
            {
                PlayerPrefs.SetString("jwt_token", response.token);
                Debug.Log("로그인 성공, 토큰 저장 완료");

                LoginManager.Instance.AfterLogin();
            }
            else
            {
                isCheck = false;
                Debug.LogError("응답에 토큰이 없습니다.");
            }
        }
        else
        {
            isCheck = false;
            Debug.LogError($"로그인 실패: {request.error}");
            Debug.LogError($"서버 응답: {request.downloadHandler.text}");
            Debug.Log($"요청 JSON: {jsonData}");
        }
    }
    [Serializable]
    public class LoginDto
    {
        public string Email;
        public string Password;
    }
    [System.Serializable]
    public class TokenResponseWrapper
    {
        public string token;
    }
}
