using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ServerCore;
using Packet;
using UnityEngine.TextCore.Text;

public class GameSceneLoader : SceneSingleton<GameSceneLoader>
{
    [SerializeField] private string characterAddress = "Character"; // Addressable �̸�
    private GameObject CharacterPrefab;
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[GameSceneLoader] �� �ε� �Ϸ� �� ������ �� ���� ��û ����");

        //var enterPacket = new C_EnterMap
        //{
        //    MapId = PlayerData.Instance.MapNumber
        //};

        //NetworkManager.Instance.Send(PacketType.C_EnterMap, enterPacket);
        LoadCharacter();
    }
    public void SpawnRoomAllCharacter(List<CharacterList> characterList)
    {
        var op = Addressables.LoadAssetAsync<GameObject>(characterAddress);
        foreach (var character in characterList)
        {
            // ĳ���͵��� ������Ű��. 
            // �ٵ� IsLocal�� true�� �ִ� ���� ĳ���� �̴�. 

        }
    }
    private void LoadCharacter()
    {
        var op = Addressables.LoadAssetAsync<GameObject>(characterAddress);
        op.Completed += OnCharacterPrefabLoaded;
    }

    private void OnCharacterPrefabLoaded(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            CharacterPrefab = handle.Result;

            C_EnterGameRequest EnterGameRequest = new C_EnterGameRequest()
            {
                MapId = MapManager.Instance.MapNumber,
                CharacterId = PlayerData.Instance.MyCharacterInfo().Id
            };

            NetworkManager.Instance.Send_EnterGame(EnterGameRequest);
        }
        else
        {
            Debug.LogError($"[GameSceneLoader] ĳ���� ������ Addressables �ε� ����: {handle.OperationException}");
        }
    }

    public void SpawnCharacters(List<CharacterList> characterList)
    {
        foreach (var character in characterList)
        {
            SpawnCharacter(character);
        }
    }
    public void SpawnCharacter(CharacterList character)
    {
        Vector3 pos = new Vector3(character.PosX, character.PosY, character.PosZ);
        Quaternion rotation = Quaternion.Euler(0f, character.DirY, 0f);
        GameObject go = Instantiate(CharacterPrefab, pos, rotation, parent: this.transform);
        PlayerController playerController = go.GetComponent<PlayerController>();
        playerController.Initialize(character.IsLocal);
        CharacterAppearance characterAppearance = go.GetComponent<CharacterAppearance>();
        characterAppearance.MyCharacterGender = (Gender)(character.CharacterInfo.Gender);
        characterAppearance.LoadFromAppearanceCode(character.CharacterInfo.AppearanceCode);

    }
}
