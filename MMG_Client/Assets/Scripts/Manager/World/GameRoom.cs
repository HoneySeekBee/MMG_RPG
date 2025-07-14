using Cinemachine;
using MMG;
using Packet;
using GamePacket;
using MonsterPacket;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GameRoom : SceneSingleton<GameRoom>
{
    public GameObject CharacterPrefab;
    public int RoomId { get; private set; }
    public Dictionary<int, RemotePlayer> Players = new();
    public Dictionary<int, RemoteMonster> Monsters = new();
    public CinemachineVirtualCamera VirtualCamera;

    public RemotePlayer MyCharacter = new();
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
            Vector3 targetpos = new Vector3(packet.BroadcastMove.PosX, packet.BroadcastMove.PosY, packet.BroadcastMove.PosZ);

            remotePlayer.MoveTo(targetpos, packet.BroadcastMove.DirY, packet.BroadcastMove.Speed);
        }
    }
    public void HandlerBoradcastMove_Monster(S_BroadcastMove packet)
    {
        if (Monsters.TryGetValue(packet.CharacterId, out var remotePlayer))
        {
            Vector3 targetpos = new Vector3(packet.BroadcastMove.PosX, packet.BroadcastMove.PosY, packet.BroadcastMove.PosZ);

            remotePlayer.MoveTo(targetpos, packet.BroadcastMove.DirY, packet.BroadcastMove.Speed);
        }
    }
    public void HandlerBattle(S_DamageBroadcast packet)
    {
        if (Players.TryGetValue(packet.AttackerId, out var attackPlayer))
        {
            BattleData data = new BattleData()
            {
                targetType = TargetType.Attacker,
                TargetId = packet.AttackerId,
                attackTypeId = 0
            };

            attackPlayer.AttackHandle(data);
        }
        if (Players.TryGetValue(packet.TargetId, out var targetPlayer))
        {
            BattleData data = new BattleData()
            {
                targetType = TargetType.Damaged,
                TargetId = packet.TargetId,
                attackTypeId = 0
            };
            targetPlayer.AttackHandle(data);
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
        Vector3 pos = new Vector3(character.MoveInfo.PosX, character.MoveInfo.PosY, character.MoveInfo.PosZ);
        Quaternion rotation = Quaternion.Euler(0f, character.MoveInfo.DirY, 0f);
        return Instantiate(CharacterPrefab, pos, rotation, parent: this.transform);
    }
    private void InitializeCharacter(GameObject go, CharacterList character)
    {
        PlayerController playerController = go.GetComponent<PlayerController>();

        playerController.Initialize(character.IsLocal);
        if (character.IsLocal)
        {
            VirtualCamera.LookAt = playerController.transform;
            VirtualCamera.Follow = playerController.transform;
        }

        var appearance = go.GetComponent<CharacterAppearance>();
        appearance.MyCharacterGender = (Gender)character.CharacterInfo.Gender;
        appearance.LoadFromAppearanceCode(character.CharacterInfo.AppearanceCode);

        var remotePlayer = go.AddComponent<RemotePlayer>();
        if (character.IsLocal)
        {
            MyCharacter = remotePlayer;
        }
        remotePlayer.Init(character, playerController);
    }
    private void RegisterCharacter(CharacterList character, GameObject go)
    {
        var remotePlayer = go.GetComponent<RemotePlayer>();
        GameRoom.Instance.AddPlayer(character, remotePlayer);
    }
    public void SpwanMonsters(S_MonsterList packet)
    {
        foreach (var monster in packet.MonsterDataList)
        {
            SpawnMonster(monster);
        }
    }
    public void SpawnMonster(MonsterStatus status)
    {
        SpawnCharacterManager SpawnManager = SpawnCharacterManager.Instance;
        // [1] monsterId 기반으로 Prefab을 가지고 오자. 

        Vector3 spawnpoint = new Vector3(status.MoveData.MonsterMove.PosX,
            status.MoveData.MonsterMove.PosY,
            status.MoveData.MonsterMove.PosZ);

        GameObject Monster =
            Instantiate(
                SpawnManager.GetMonster(status.MonsterId),
            spawnpoint,
            Quaternion.Euler(0, status.MoveData.MonsterMove.DirY, 0),
            this.transform
            );

        // [2] 초기화 해주기
        var remoteMonster = Monster.AddComponent<RemoteMonster>();
        MonsterController monsterController = Monster.GetComponent<MonsterController>();
        monsterController.Initialize();

        remoteMonster.Init(status, monsterController);

        // [3] 등록 해주기 
        Debug.Log($"[SpwanMonster] ID : {status.ID}, Name : {status.MonsterName}");
        Monsters.Add(status.ID, remoteMonster);
    }
}
