using MMG;
using Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteMonster : MonoBehaviour
{
    public int Id;
    public float HP;
    public MonsterData RemoteMonsterData;
    private MonsterController _controller;

    public void Init(MonsterStatus data, MonsterController controller)
    {
        Id = data.ID;
        HP = data.HP;

        RemoteMonsterData = new MonsterData()
        {
            MonsterId = data.MonsterId,
            MonsterName = data.MonsterName,
            _MaxHP = data.MaxHP,
            _MoveSpeed = data.MoveSpeed,

            _ChaseRange = data.ChaseRange,
            _AttackRange = data.AttackRange,
        };
        _controller = controller;
        Vector3 spawnPoint = new Vector3(data.MoveData.MonsterMove.PosX, data.MoveData.MonsterMove.PosY, data.MoveData.MonsterMove.PosZ);
        transform.position = spawnPoint;
        transform.rotation = Quaternion.Euler(0, data.MoveData.MonsterMove.DirY, 0);
    }
}
