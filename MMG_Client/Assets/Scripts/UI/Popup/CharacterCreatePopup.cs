using DevionGames.InventorySystem;
using MMG.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Packet;


public class CharacterCreatePopup : PopupBase
{
    public static CharacterCreatePopup Instance { get; private set; }

    [SerializeField] private PartItemChangeButton[] partItemButtons;
    [SerializeField] private int _slotNumber;
    [SerializeField] private TMP_InputField NickNameField;
    private bool isSubmit;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void Set(int slotNuber)
    {
        _slotNumber = slotNuber;
    }
    private void OnEnable()
    {
        CreateCharacterManager.Instance.Initialize();
        SetGender(Gender.Man);
    }

    public void SetGender(Gender gender)
    {
        CreateCharacterManager.Instance.SetGender(gender);
        UpdateAllButtonNames();
    }
    public void Toggle_Gender_Man()
    {
        SetGender(Gender.Man);
    }
    public void Toggle_Gender_Gril()
    {
        SetGender(Gender.Girl);
    }
    public void OnClickNext(EquipSlot slot)
    {
        CreateCharacterManager.Instance.CycleNext(slot);
        UpdateButtonName(slot);
    }

    public void OnClickPrev(EquipSlot slot)
    {
        CreateCharacterManager.Instance.CyclePrev(slot);
        UpdateButtonName(slot);
    }

    private void UpdateAllButtonNames()
    {
        foreach (var button in partItemButtons)
            UpdateButtonName(button.ThisSlot);
    }

    private void UpdateButtonName(EquipSlot slot)
    {
        var item = CreateCharacterManager.Instance.GetCurrentItem(slot);
        foreach (var button in partItemButtons)
        {
            if (button.ThisSlot == slot)
                button.SetButtonsName(item.Name);
        }
    }
    public void Submit()
    {
        if (isSubmit)
            return;

        StartCoroutine(SubmitCreateCharacter());
    }
    public IEnumerator SubmitCreateCharacter()
    {
        isSubmit = true;
        string nickName = NickNameField.text;
        bool canUseNickname = false;
        string url = NetworkManager.MMG_API_URL + $"/api/character/check-name?nickname={nickName}";

        UnityWebRequest request = UnityWebRequest.Get(url);
        string token = PlayerPrefs.GetString("jwt_token");

        request.SetRequestHeader("Authorization", $"Bearer {token}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success || request.responseCode == 200)
        {
            string json = request.downloadHandler.text;
            var result = JsonConvert.DeserializeObject<DuplicateNameResponse>(json);

            if (result.isDuplicate)
            {
                Debug.LogWarning("이미 사용 중인 닉네임입니다.");
            }
            else
            {
                Debug.Log("사용 가능한 닉네임입니다.");
                canUseNickname = true;
            }
        }
        else
        {
            Debug.LogError($"중복 닉네임 확인 실패: {request.error}");
            Debug.LogError($"서버 응답: {request.downloadHandler.text}");
        }

        if (canUseNickname)
        {
            CharacterCreateDto characterData = new CharacterCreateDto()
            {
                CharacterName = nickName,
                SlotNumber = _slotNumber,
                Class = (int)ClassType.NoHave,
                Gender =(int)CreateCharacterManager.Instance.currentGender,
                AppearanceCode = CreateCharacterManager.Instance.createCharacter.ToAppearanceCode(),
                
            };
            StartCoroutine(CreateCharacter(characterData));
        }
        else
        {
        }
        isSubmit = false;
    }
    public IEnumerator CreateCharacter(CharacterCreateDto createDto)
    {
        string url = NetworkManager.MMG_API_URL + "/api/character/create";
        string token = PlayerPrefs.GetString("jwt_token");

        string jsonData = JsonConvert.SerializeObject(createDto);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Authorization", $"Bearer {token}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success || request.responseCode == 200)
        {
            Debug.Log("캐릭터 생성 성공");
            string json = request.downloadHandler.text;
            // 필요하면 여기서 응답 파싱

            LoginManager.Instance.Call_UserCharacterList();
            PopupManager.Instance.UnShow<CharacterCreatePopup>();
        }
        else
        {
            Debug.Log($"캐릭터 slot 번호 : {_slotNumber} : {createDto.SlotNumber}");
            Debug.LogError($"캐릭터 생성 실패: {request.error}");
            Debug.LogError($"서버 응답 : {request.downloadHandler.text}");
        }
    }
    

    [System.Serializable]
    public class DuplicateNameResponse
    {
        public bool isDuplicate;
    }
    [System.Serializable]
    public class CharacterCreateDto
    {
        public int SlotNumber { get; set; }
        public string CharacterName { get; set; } = string.Empty;
        public int Gender { get; set; }
        public int Class { get; set; } = (int)ClassType.NoHave;
        public string AppearanceCode { get; set; } = string.Empty;
    }
}
