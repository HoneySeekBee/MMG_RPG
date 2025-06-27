using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
public class CharacterSelectButton : MonoBehaviour
{
    private Button thisButton;
    private TestPlayer ThisPlayerInfo;

    [SerializeField] private int buttonNumber;
    public int Id { get { return buttonNumber; } }

    [Header("ĳ���� ������")]
    [SerializeField] private GameObject EmptyObj;

    [Header("ĳ���� ������")]
    [SerializeField] private GameObject CharacterObj;
    [SerializeField] private TMP_Text NickNameText;
    [SerializeField] private TMP_Text ClassText;
    [SerializeField] private TMP_Text LevelText;
    [SerializeField] private RawImage rawImage;

    public void Set(bool hasCharacter, TestPlayer player = null, RenderTexture renderTexture = null)
    {
        thisButton = this.GetComponent<Button>();
        if (hasCharacter)
        {
            if(player != null)
            {
                ThisPlayerInfo = player;
                Init_Character(ThisPlayerInfo.NickName, ThisPlayerInfo.Class, ThisPlayerInfo.Leve);
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
    private void Init_Character(string NickName, string Class, int Level)
    {
        NickNameText.text = NickName;
        ClassText.text = Class;
        LevelText.text = "Lv." + Level;
    }

    #region ��ư�� ���
    public void CreateCharacter()
    {
        Debug.Log("���� ĳ���͸� �����ؾ���");
    }
    public void EnterGame()
    {
        bool hasInfo = true;
        if (hasInfo == false)
            Debug.LogError("ĳ���� ������ �߸�����");
        // ���⼭ ĳ���� ������ ����� �ҷ��������� üũ�ϴ� Bool 

        Debug.Log("���� ������ ��������.");
    }
    #endregion
}
