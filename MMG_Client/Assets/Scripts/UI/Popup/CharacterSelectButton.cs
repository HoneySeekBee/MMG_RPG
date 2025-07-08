using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MMG.UI;
using Packet;

// �ӽ� 
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
    public ClassType @class; // class�� ������̱� ������ �̷���!
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

    [Header("ĳ���� ������")]
    [SerializeField] private GameObject EmptyObj;

    [Header("ĳ���� ������")]
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
            // ĳ���� ���� ��ư ����� �߰����ش�. 
            thisButton.onClick.AddListener(CreateCharacter);
        }
    }
    private void Init_Character(string NickName, ClassType Class, int Level)
    {
        NickNameText.text = NickName;
        ClassText.text = Class.ToString();
        LevelText.text = "Lv." + Level;
    }

    #region ��ư�� ���
    public void CreateCharacter()
    {
        Debug.Log($"���� ĳ���͸� �����ؾ��� {buttonNumber}");
        PopupManager.Instance.Show<CharacterCreatePopup>((popup) => popup.Set(buttonNumber));
        PopupManager.Instance.UnShow<CharacterSelectPopup>();
    }
    public void EnterGame()
    {
        Debug.Log("���� ������ ��������.");
        // TCP ������ �����ؾ��Ѵ�.
        // ����â�� �Ⱥ��̰� �ؾ��Ѵ�. 
        PlayerData.Instance.InitializeFrom(ThisPlayerInfo);
        if (ServerConnector.Instance != null)
        {
            StartCoroutine(ServerConnector.Instance.ConnectToServer());
            PopupManager.Instance.UnShow<CharacterSelectPopup>();
        }

    }
    #endregion
}
