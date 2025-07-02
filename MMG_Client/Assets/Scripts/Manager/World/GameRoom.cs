using Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoom : SceneSingleton<GameRoom>
{
    public int RoomId { get; private set; }
    public Dictionary<int, RemotePlayer> Players = new();
    public GameObject CharacterPrefab;
    public void Init(int roomId)
    {
        RoomId = roomId;
        Players.Clear();
    }
    #region
    public void HandleBroadcastEnter(S_BroadcastEnter packet)
    {
        SpawnCharacter(packet.EnterCharacter); // 서버에서 누가 입장했다고 보내줌
    }
    public void HandleBroadcastMove(S_BroadcastMove packet)
    {
        Debug.Log($"캐릭터 이동 : {packet.CharacterId}");
        if (Players.TryGetValue(packet.CharacterId, out var remotePlayer))
        {
            Vector3 targetpos = new Vector3(packet.PosX, packet.PosY, packet.PosZ);

            remotePlayer.MoveTo(targetpos, packet.DirY, packet.Speed);
        }
    }
    //public void HandleBroadcastLeave(S_BroadcastLeave packet)
    //{
    //    RemovePlayer(packet.CharacterId); // 유저 나감
    //}
    #endregion

    public void AddPlayer(CharacterList info, RemotePlayer player)
    {
        if (Players.ContainsKey(info.CharacterInfo.Id)) return;
        Debug.Log($"캐릭터 등록 : {info.CharacterInfo.Id}");
        Players.Add(info.CharacterInfo.Id, player);
    }

    public void RemovePlayer(int characterId)
    {
        if (Players.ContainsKey(characterId))
        {
            UnityEngine.Object.Destroy(Players[characterId].gameObject);
            Players.Remove(characterId);
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
        GameObject go = InstantiateCharacter(character);
        InitializeCharacter(go, character); 
        RegisterCharacter(character, go);
    }
    private GameObject InstantiateCharacter(CharacterList character)
    {
        Vector3 pos = new Vector3(character.PosX, character.PosY, character.PosZ);
        Quaternion rotation = Quaternion.Euler(0f, character.DirY, 0f);
        return Instantiate(CharacterPrefab, pos, rotation, parent: this.transform);
    }
    private void InitializeCharacter(GameObject go, CharacterList character)
    {
        PlayerController playerController = go.GetComponent<PlayerController>();
        playerController.Initialize(character.IsLocal);

        var appearance = go.GetComponent<CharacterAppearance>();
        appearance.MyCharacterGender = (Gender)character.CharacterInfo.Gender;
        appearance.LoadFromAppearanceCode(character.CharacterInfo.AppearanceCode);

        var remotePlayer = go.AddComponent<RemotePlayer>();
        remotePlayer.Init(character, playerController);
    }
    private void RegisterCharacter(CharacterList character, GameObject go)
    {
        var remotePlayer = go.GetComponent<RemotePlayer>();
        GameRoom.Instance.AddPlayer(character, remotePlayer);
    }
}
