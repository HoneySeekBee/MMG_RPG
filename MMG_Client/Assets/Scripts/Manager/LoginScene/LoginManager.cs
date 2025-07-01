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
    // �α��ξ� ���� ����
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // [1] �α��� ��û

    // [2] ĳ���� ���� Ȥ�� ���� ��û 
    private void Start()
    {
        if (GameManager.Instance.IsLoggedIn())
        {
            // �̷��� �ش� ������ ĳ���͸� ��û�ϰ� �޾ƿ´�. 
            StartCoroutine(CharacterListRequest());
        }
        else
        {
            // �α��� ȭ���� ȣ���Ѵ�. 
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
                Debug.Log("ĳ���Ͱ� �����ϴ�. ���� �������ּ���.");
            }
            else
            {
                Debug.Log($"ĳ���� �ҷ����� ����: {json}");
                CharacterData[] slots = new CharacterData[4]; 
                CharacterData[] characters = JsonConvert.DeserializeObject<CharacterData[]>(json);

            }
            PreviewManager.Instance.Set_UserCharacter(json);
            PopupManager.Instance.Show<CharacterSelectPopup>((popup) => popup.Init());
        }
        else
        {
            Debug.LogError($"ĳ���� �ҷ����� ����: {request.error}");
            Debug.LogError($"���� ����: {request.downloadHandler.text}");
        }
    }
    public void AfterLogin()
    {
        PopupManager.Instance.UnShow<LoginPopup>();
        //PopupManager.Instance.Show<CharacterSelectPopup>();
        StartCoroutine(CharacterListRequest());
    }

}
