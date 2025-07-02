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

    [SerializeField] private string characterAddress = "Character"; // Addressable 이름
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
        Debug.Log("[GameSceneLoader] 씬 로드 완료 → 서버에 맵 입장 요청 전송");

        InputManager.Instance.Initialize();
        LoadCharacter();
    }
    public void SpawnRoomAllCharacter(List<CharacterList> characterList)
    {
        var op = Addressables.LoadAssetAsync<GameObject>(characterAddress);
        foreach (var character in characterList)
        {
            // 캐릭터들을 생성시키자. 
            // 근데 IsLocal이 true인 애는 나의 캐릭터 이다. 

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
            Debug.LogError($"[GameSceneLoader] 캐릭터 프리팹 Addressables 로딩 실패: {handle.OperationException}");
        }
    }

}
