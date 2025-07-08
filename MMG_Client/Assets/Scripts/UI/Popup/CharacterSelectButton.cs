using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MMG.UI;
using Packet;

// 임시 
[System.Serializable]
public class TestPlayer
{
    public int id;
    public string NickName;
    public string Class;
    public int Leve;
}
[System.Serializable]
public class TempPlayer
{
    public int buttonNumber;
    public TestPlayer testPlayer;
}
[System.Serializable]
public class CharacterData
{
    public int id;
    public int userId;
    public Gender Gender;
    public int slotNumber;
    public string characterName;
    public ClassType @class; // class는 예약어이기 때문에 이렇게!
    public string appearanceCode;
    public string createdAt;
    public string lastPlayedAt;
    public int? LastMapId;
    public int? LastSpawnPointId;
    public bool isDeleted;
}
[System.Serializable]
public class CharacterDataListWrapper
{
    public List<CharacterData> characters;
}

public class CharacterSelectButton : MonoBehaviour
{
    private Button thisButton;
    private CharacterData ThisPlayerInfo;

    [SerializeField] private int buttonNumber;
    public int Id => buttonNumber;

    [Header("캐릭터 없을때")]
    [SerializeField] private GameObject EmptyObj;

    [Header("캐릭터 있을때")]
    [SerializeField] private GameObject CharacterObj;
    [SerializeField] private TMP_Text NickNameText;
    [SerializeField] private TMP_Text ClassText;
    [SerializeField] private TMP_Text LevelText;
    [SerializeField] private RawImage rawImage;

    public void Set(bool hasCharacter, CharacterData player = null, RenderTexture renderTexture = null)
    {
        thisButton = this.GetComponent<Button>();
        thisButton.onClick.RemoveListener(EnterGame);
        thisButton.onClick.RemoveListener(CreateCharacter);

        if (hasCharacter)
        {
            if (player != null)
            {
                ThisPlayerInfo = player;
                Init_Character(ThisPlayerInfo.characterName, ThisPlayerInfo.@class, 1);
            }
            if (renderTexture != null)
                rawImage.texture = renderTexture;
            EmptyObj.SetActive(false);
            CharacterObj.SetActive(true);
            thisButton.onClick.AddListener(EnterGame);
        }
        else
        {
            EmptyObj.SetActive(true);
            CharacterObj.SetActive(false);
            // 캐릭터 생성 버튼 기능을 추가해준다. 
            thisButton.onClick.AddListener(CreateCharacter);
        }
    }
    private void Init_Character(string NickName, ClassType Class, int Level)
    {
        NickNameText.text = NickName;
        ClassText.text = Class.ToString();
        LevelText.text = "Lv." + Level;
    }

    #region 버튼의 기능
    public void CreateCharacter()
    {
        Debug.Log($"이제 캐릭터를 생성해야지 {buttonNumber}");
        PopupManager.Instance.Show<CharacterCreatePopup>((popup) => popup.Set(buttonNumber));
        PopupManager.Instance.UnShow<CharacterSelectPopup>();
    }
    public void EnterGame()
    {
        Debug.Log("이제 게임을 시작하자.");
        // TCP 서버에 연결해야한다.
        // 선택창을 안보이게 해야한다. 
        PlayerData.Instance.InitializeFrom(ThisPlayerInfo);
        if (ServerConnector.Instance != null)
        {
            StartCoroutine(ServerConnector.Instance.ConnectToServer());
            PopupManager.Instance.UnShow<CharacterSelectPopup>();
        }

    }
    #endregion
}
