using MMG.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PreviewManager : MonoBehaviour
{
    public static PreviewManager Instance { get; private set; }

    [SerializeField] private List<TempPlayer> testPlayers = new List<TempPlayer>(); // �ӽ� �÷��̾�
    [SerializeField] private GameObject characterPrefab; // �ӽ� ĳ���� ��

    [SerializeField] private Camera renderCamera;
    [SerializeField] private RenderTexture previewTexture; // ���� PreviewTexture
    [SerializeField] private Transform previewRoot;

    private Dictionary<int, RenderTexture> renderTextures = new Dictionary<int, RenderTexture>();

    [SerializeField] private Camera[] previewCamera;

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
    private void Set()
    {
        int cnt = 0;
        for (int i = 0; i < MAX_CHARACTER; i++)
        {
            // �ؽ��� ���� 
            RenderTexture newRT = new RenderTexture(previewTexture);
            newRT.name = "Texture_" + cnt++;
            renderTextures[i] = newRT;
            previewCamera[i].targetTexture = renderTextures[i];


            GameObject go = Instantiate(characterPrefab, previewCamera[i].transform.parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.rotation = Quaternion.Euler(0, 180, 0);
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
        return testPlayers.Any(x => x.buttonNumber == _id);
    }
    public RenderTexture GetTexture(int _id)
    {
        if (renderTextures.ContainsKey(_id))
            return renderTextures[_id];
        else
            return null;
    }
    public TestPlayer GetPlayerInfo(int _id)
    {
        TestPlayer player = testPlayers.FirstOrDefault(x => x.buttonNumber == _id).testPlayer;
        return player;
    }

    public void ShowCharacterSelectPopup()
    {
        PopupManager.Instance.Show<CharacterSelectPopup>();
    }
}
