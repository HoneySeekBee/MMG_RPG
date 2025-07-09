using Newtonsoft.Json;
using Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class InitSceneManager : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(Process());
    }
    private IEnumerator GlobalCashing()
    {
        yield return SpawnCharacterManager.Instance.SpawnDataCashing();
    }
    private IEnumerator Process()
    {
        string token = PlayerPrefs.GetString("jwt_token", string.Empty);
        string url = NetworkManager.MMG_API_URL + "/api/auth/validate?token=" + UnityWebRequest.EscapeURL(token);


        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("[Login] ����� JWT ���� �� �α��� ȭ������ �̵�");
            SceneLoader.Instance.LoadScene("LoginScene");
            yield break;
        }

        // ��ū�� �ִٸ� �� API�� ���� ��û
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var json = request.downloadHandler.text;
            var result = JsonConvert.DeserializeObject<LoginValidationResult>(json);

            bool isVaild = result.IsValid;

            isVaild = false; // ================= �ڵ� �α����� ������ �Ѱ��� PC�� ���α׷� 1���� �ϰ� ó���Ǹ� ��

            if (isVaild)
            {
                Debug.Log("[Login] ��ȿ�� ��ū �� �ڵ� �α��� ����");
                GameManager.Instance.SetUser(result.UserId, result.Email, result.Nickname);
            }
            else
            {
                Debug.LogWarning("[Login] ��ȿ���� ���� ��ū �� �α��� ȭ������ �̵�");
                PlayerPrefs.DeleteKey("jwt_token");
            }
        }
        else
        {
            Debug.LogError($"[Login] ��ū �˻� ����: {request.error}");
        }

        // Global ĳ�� ó�� 
        yield return GlobalCashing();

        SceneLoader.Instance.LoadScene("LoginScene");
    }
    [System.Serializable]
    public class LoginValidationResult
    {
        [JsonProperty("isValid")]
        public bool IsValid;
        [JsonProperty("reason")]
        public string Reason;
        [JsonProperty("userId")]
        public int UserId;
        [JsonProperty("email")]
        public string Email;
        [JsonProperty("nickname")]
        public string Nickname;
    }
}
