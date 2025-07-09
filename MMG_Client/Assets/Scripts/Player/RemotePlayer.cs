using MMG;
using Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    public CharacterData RemoteCharaceterData;
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
        transform.position = new Vector3(data.PosX, data.PosY, data.PosZ);
        // 외형 구성 등등
    }
    public void MoveTo(Vector3 targetPos, float dirY, float speed)
    {
        Debug.Log($"움직입시다. {targetPos}");
        _controller.SetMove(targetPos, dirY, speed);
    }
    public void AttackHandle(BattleData battleData)
    {
        _controller.SetAttack(battleData);
    }
}
