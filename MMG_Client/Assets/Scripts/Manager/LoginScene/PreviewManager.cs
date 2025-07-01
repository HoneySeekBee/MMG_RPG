using MMG.UI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PreviewManager : MonoBehaviour
{
    public static PreviewManager Instance { get; private set; }

    [SerializeField] private GameObject characterPrefab; // �ӽ� ĳ���� ��

    [SerializeField] private Camera renderCamera;
    [SerializeField] private RenderTexture previewTexture; // ���� PreviewTexture
    [SerializeField] private Transform previewRoot;

    private Dictionary<int, RenderTexture> renderTextures = new Dictionary<int, RenderTexture>();

    [SerializeField] private Camera[] previewCamera;
    [SerializeField] private CharacterData[] slots;
    [SerializeField] private CharacterData[] characters;
    [SerializeField] private CharacterAppearance[] characterAppearances;

    const int MAX_CHARACTER = 4;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // �ߺ� ����
            return;
        }

        Instance = this;
    }
    private void Start()
    {
        Set();
    }
    void Update()
    {
        for (int i = 0; i < previewCamera.Length; i++)
        {
            previewCamera[i].Render();
        }
    }
    public void Set_UserCharacter(string json)
    {
        slots = new CharacterData[MAX_CHARACTER];

        if (string.IsNullOrEmpty(json) || json == "null" || json == "[]")
        {
            Debug.Log("[Set_UserCharacter] ĳ���� JSON�� ��� ����");
            return;
        }

        try
        {
            characters = JsonConvert.DeserializeObject<List<CharacterData>>(json).ToArray();

            foreach (var character in characters)
            {
                if (character.slotNumber >= 0 && character.slotNumber < MAX_CHARACTER)
                {
                    slots[character.slotNumber] = character;
                }
                if (characterAppearances[character.slotNumber] != null)
                {
                    Debug.Log($"character {character.slotNumber} : {character.appearanceCode}");
                    characterAppearances[character.slotNumber].MyCharacterGender = character.Gender;
                    characterAppearances[character.slotNumber].LoadFromAppearanceCode(character.appearanceCode);
                }
                else
                {
                    Debug.LogWarning($"[Set_UserCharacter] characterAppearances[{character.slotNumber}] �� null�� {characterAppearances.Length}");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Set_UserCharacter] JSON �Ľ� ����: {ex.Message}");
        }
    }
    private void Set()
    {
        int cnt = 0;
        characterAppearances = new CharacterAppearance[MAX_CHARACTER];
        for (int i = 0; i < MAX_CHARACTER; i++)
        {
            // �ؽ��� ���� 
            RenderTexture newRT = new RenderTexture(previewTexture);
            newRT.name = "Texture_" + cnt++;
            renderTextures[i] = newRT;
            previewCamera[i].targetTexture = renderTextures[i];

            GameObject go = Instantiate(characterPrefab, previewCamera[i].transform.parent);
            go.transform.localPosition = Vector3.zero;
            characterAppearances[i] = go.GetComponent<CharacterAppearance>();
            SetLayerRecursively(go.transform, LayerMask.NameToLayer("Preview"));
        }
    }
    private void SetLayerRecursively(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        foreach (Transform child in t)
            SetLayerRecursively(child, layer);
    }

    // ���� ������

    public bool HasPlayer(int _id)
    {
        return characters.Any(x => x.slotNumber == _id);
    }
    public RenderTexture GetTexture(int _id)
    {
        if (renderTextures.ContainsKey(_id))
            return renderTextures[_id];
        else
            return null;
    }
    public CharacterData GetPlayerInfo(int _id)
    {
        CharacterData player = slots[_id];
        return player;
    }

    public void ShowCharacterSelectPopup()
    {
        PopupManager.Instance.Show<CharacterSelectPopup>();
    }
}
