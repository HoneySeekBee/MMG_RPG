using Cinemachine;
using MMG;
using Packet;
using GamePacket;
using MonsterPacket;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using AttackPacket;

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
        if (packet.Damage.IsMonster) // 공격자가 몬스터? 
        {

            RemoteMonster AttackMonster = new();
            if (Monsters.TryGetValue(packet.Damage.AttackerId, out AttackMonster))
            {
                BattleData data = new BattleData()
                {
                    targetType = TargetType.Attacker,
                    TargetId = packet.Damage.AttackerId,
                    attackTypeId = packet.Damage.AttackId,
                };

                AttackMonster.AttackHandle(data);
            }

            Debug.Log($"[HandlerBattle] User Damage~ {packet.Damage.TargetId} : {GameRoom.Instance.MyCharacter.RemoteCharaceterData.id} : {GameRoom.Instance.MyCharacter.StatInfo.NowHP}");
            if (Players.TryGetValue(packet.Damage.TargetId, out var remotePlayer))
            {
                remotePlayer.GetDamage(packet.Damage.Damage);
                BattleData data = new BattleData()
                {
                    targetType = TargetType.Damaged,
                    TargetId = packet.Damage.TargetId,
                    attackTypeId = 0,
                };
                remotePlayer.AttackHandle(data);
            }
        }
        else
        {
            RemotePlayer AttackPlayer = new();
            if (Players.TryGetValue(packet.Damage.AttackerId, out AttackPlayer))
            {
                BattleData data = new BattleData()
                {
                    targetType = TargetType.Attacker,
                    TargetId = packet.Damage.AttackerId,
                    attackTypeId = packet.Damage.AttackId,
                };

                AttackPlayer.AttackHandle(data);
            }
            if (Monsters.TryGetValue(packet.Damage.TargetId, out var remoteMonster))
            {
                remoteMonster.GetDamage(packet.Damage.Damage);
                BattleData data = new BattleData()
                {
                    targetType = TargetType.Damaged,
                    TargetId = packet.Damage.TargetId,
                    attackTypeId = 0,
                };
                remoteMonster.AttackHandle(data);
                if (GameRoom.Instance.MyCharacter.RemoteCharaceterData.id == packet.Damage.AttackerId && AttackPlayer != null)
                {
                    AttackPlayer.InGameUI.ShowMonsterHP(remoteMonster);
                }
            }

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
        Debug.Log("[GameRoom] SpawnCharacter 1");
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
    public void DeadMonster(S_DeathBroadcast packet)
    {
        if (packet.IsMonster)
        {
            Monsters[packet.ObjectId].OnDead();
            Monsters.Remove(packet.ObjectId);
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
                SpawnManager.GetMonster(status.MonsterData.MonsterId),
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
        Debug.Log($"[SpwanMonster] ID : {status.ID}, Name : {status.MonsterData.MonsterName}");
        Monsters.Add(status.ID, remoteMonster);
    }
    public void PlayerDie(PlayerId packet)
    {
        Players[packet.PlayerId_].PlayerDie();
    }
    public void PlayerRespawn(S_PlayerRespawn packet)
    {
        Players[packet.PlayerId].PlayerRespawn(packet);
    }
}
