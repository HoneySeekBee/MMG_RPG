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
            Debug.LogWarning("���̵� �Է����ּ���");
            isCheck = false;
            yield break;
        }

        if (string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("��й�ȣ�� �Է����ּ���");
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

            // �������� {"token":"..."} �������� �ִ� ��� �Ľ�
            var response = JsonConvert.DeserializeObject<TokenResponseWrapper>(json);
            if (!string.IsNullOrEmpty(response.token))
            {
                PlayerPrefs.SetString("jwt_token", response.token);
                Debug.Log("�α��� ����, ��ū ���� �Ϸ�");

                LoginManager.Instance.AfterLogin();
            }
            else
            {
                isCheck = false;
                Debug.LogError("���信 ��ū�� �����ϴ�.");
            }
        }
        else
        {
            isCheck = false;
            Debug.LogError($"�α��� ����: {request.error}");
            Debug.LogError($"���� ����: {request.downloadHandler.text}");
            Debug.Log($"��û JSON: {jsonData}");
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
