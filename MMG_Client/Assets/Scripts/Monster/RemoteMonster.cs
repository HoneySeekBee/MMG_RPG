using MMG;
using MonsterPacket;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteMonster : MonoBehaviour
{
    public int Id;
    public float HP;
    public MMG.MonsterData RemoteMonsterData;
    private MonsterController _controller;

    public void Init(MonsterStatus data, MonsterController controller)
    {
        Id = data.ID;
        HP = data.HP;

        RemoteMonsterData = new MMG.MonsterData()
        {
            MonsterId = data.MonsterData.MonsterId,
            MonsterName = data.MonsterData.MonsterName,
            _MaxHP = data.MonsterData.MaxHP,
            _MoveSpeed = data.MonsterData.MoveSpeed,

            _ChaseRange = data.MonsterData.ChaseRange,
            _AttackRange = data.MonsterData.AttackRange,
        };

        _controller = controller;

        Vector3 spawnPoint = new Vector3(data.MoveData.MonsterMove.PosX, data.MoveData.MonsterMove.PosY, data.MoveData.MonsterMove.PosZ);
        transform.position = spawnPoint;
        transform.rotation = Quaternion.Euler(0, data.MoveData.MonsterMove.DirY, 0);

        _controller.Init_Position(spawnPoint, data.MoveData.MonsterMove.DirY);
    }
    public void MoveTo(Vector3 targetPos, float dirY, float speed)
    {
        _controller.SetMove(targetPos, dirY, speed);
    }
}
