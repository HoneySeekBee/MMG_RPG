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

        InputManager.Instance.Initialize();
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
            GameObject CharacterPrefab = handle.Result;
            GameRoom.Instance.CharacterPrefab = CharacterPrefab;

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

}
