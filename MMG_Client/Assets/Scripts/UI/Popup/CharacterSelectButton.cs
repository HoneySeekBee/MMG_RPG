using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
public class CharacterSelectButton : MonoBehaviour
{
    private Button thisButton;
    private TestPlayer ThisPlayerInfo;

    [SerializeField] private int buttonNumber;
    public int Id { get { return buttonNumber; } }

    [Header("캐릭터 없을때")]
    [SerializeField] private GameObject EmptyObj;

    [Header("캐릭터 있을때")]
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
            // 캐릭터 생성 버튼 기능을 추가해준다. 
            thisButton.onClick.AddListener(CreateCharacter);
        }
    }
    private void Init_Character(string NickName, string Class, int Level)
    {
        NickNameText.text = NickName;
        ClassText.text = Class;
        LevelText.text = "Lv." + Level;
    }

    #region 버튼의 기능
    public void CreateCharacter()
    {
        Debug.Log("이제 캐릭터를 생성해야지");
    }
    public void EnterGame()
    {
        bool hasInfo = true;
        if (hasInfo == false)
            Debug.LogError("캐릭터 정보가 잘못됬어요");
        // 여기서 캐릭터 정보가 제대로 불러와졌는지 체크하는 Bool 

        Debug.Log("이제 게임을 시작하자.");
    }
    #endregion
}
