using MMG;
using GamePacket;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    public CharacterData RemoteCharaceterData;
    public Status StatInfo;

    private PlayerController _controller;
    public void Init(CharacterList data, PlayerController controller)
    {
        RemoteCharaceterData = new CharacterData(){
            id = data.CharacterInfo.Id,
            userId = data.CharacterInfo.UserId,
            Gender = (Gender)data.CharacterInfo.Gender,
            characterName = data.CharacterInfo.CharacterName,
            @class = (ClassType)data.CharacterInfo.Class,
            appearanceCode = data.CharacterInfo.AppearanceCode,
        };
        _controller = controller;
        StatInfo = data.StatInfo;
        transform.position = new Vector3(data.MoveInfo.PosX, data.MoveInfo.PosY, data.MoveInfo.PosZ);
        // 외형 구성 등등
    }
    public void MoveTo(Vector3 targetPos, float dirY, float speed)
    {
        _controller.SetMove(targetPos, dirY, speed);
    }
    public void AttackHandle(BattleData battleData)
    {
        _controller.SetAttack(battleData);
    }
}
