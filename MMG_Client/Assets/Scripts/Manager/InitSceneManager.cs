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
            Debug.Log("[Login] 저장된 JWT 없음 → 로그인 화면으로 이동");
            SceneLoader.Instance.LoadScene("LoginScene");
            yield break;
        }

        // 토큰이 있다면 → API로 검증 요청
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var json = request.downloadHandler.text;
            var result = JsonConvert.DeserializeObject<LoginValidationResult>(json);

            bool isVaild = result.IsValid;

            isVaild = false; // ================= 자동 로그인은 다음에 한개의 PC에 프로그램 1개만 하게 처리되면 ㄱ

            if (isVaild)
            {
                Debug.Log("[Login] 유효한 토큰 → 자동 로그인 성공");
                GameManager.Instance.SetUser(result.UserId, result.Email, result.Nickname);
            }
            else
            {
                Debug.LogWarning("[Login] 유효하지 않은 토큰 → 로그인 화면으로 이동");
                PlayerPrefs.DeleteKey("jwt_token");
            }
        }
        else
        {
            Debug.LogError($"[Login] 토큰 검사 실패: {request.error}");
        }

        // Global 캐싱 처리 
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
