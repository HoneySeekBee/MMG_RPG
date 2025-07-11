using GamePacket;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : GlobalSingleton<PlayerData>
{
    public CharacterData MyCharaceterData;


    /// <summary>
    /// GameManager에서 선택된 캐릭터의 정보를 복사
    /// </summary>
    public void InitializeFrom(CharacterData data)
    {
        MyCharaceterData = data;
    }
    public GamePacket.CharacterInfo MyCharacterInfo()
    {
        GamePacket.CharacterInfo characterInfo = new GamePacket.CharacterInfo()
        {
           Id  = MyCharaceterData.id,
            UserId= MyCharaceterData.userId,
            Gender = (int)MyCharaceterData.Gender,
            CharacterName = MyCharaceterData.characterName,
            Class = MyCharaceterData.@class,
            AppearanceCode = MyCharaceterData.appearanceCode,
        };
        return characterInfo;
    }
}
