using MMG.UI;
using Newtonsoft.Json;
using Packet;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Networking;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; }
    // 로그인씬 진입 순서
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // [1] 로그인 요청

    // [2] 캐릭터 생성 혹은 선택 요청 
    private void Start()
    {
        if (GameManager.Instance.IsLoggedIn())
        {
            // 이러면 해당 유저의 캐릭터를 요청하고 받아온다. 
            StartCoroutine(CharacterListRequest());
        }
        else
        {
            // 로그인 화면을 호출한다. 
            PopupManager.Instance.Show<LoginPopup>();
        }
    }
    public void Call_UserCharacterList()
    {
        StartCoroutine(CharacterListRequest());
    }
    private IEnumerator CharacterListRequest()
    {
        string url = NetworkManager.MMG_API_URL + "/api/character/list";

        UnityWebRequest request = UnityWebRequest.Get(url);
        string token = PlayerPrefs.GetString("jwt_token");

        request.SetRequestHeader("Authorization", $"Bearer {token}");

        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.Success || request.responseCode == 200)
        {
            string json = request.downloadHandler.text;

            Debug.Log("json : " + json);
            if (string.IsNullOrEmpty(json) || json == "null" || json == "[]")
            {
                Debug.Log("캐릭터가 없습니다. 새로 생성해주세요.");
            }
            else
            {
                Debug.Log($"캐릭터 불러오기 성공: {json}");
                CharacterData[] slots = new CharacterData[4]; 
                CharacterData[] characters = JsonConvert.DeserializeObject<CharacterData[]>(json);

            }
            PreviewManager.Instance.Set_UserCharacter(json);
            PopupManager.Instance.Show<CharacterSelectPopup>((popup) => popup.Init());
        }
        else
        {
            Debug.LogError($"캐릭터 불러오기 실패: {request.error}");
            Debug.LogError($"서버 응답: {request.downloadHandler.text}");
        }
    }
    public void AfterLogin()
    {
        PopupManager.Instance.UnShow<LoginPopup>();
        //PopupManager.Instance.Show<CharacterSelectPopup>();
        StartCoroutine(CharacterListRequest());
    }

}
