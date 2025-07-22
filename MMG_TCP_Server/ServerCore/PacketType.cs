using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    // 1 ~ 99 : 로그인 / 인증
    // 100 ~ 199 : 게임 관련
    // 200 ~ 299 : 몬스터 관련

    // 900 ~ 999 : 에러 관련
    // 9000 ~ 9999 : 테스트용/디버그
    public enum PacketType : ushort
    {
        C_LoginToken = 1,
        S_LoginToken = 2,

        // 게임 관련 
        C_EnterGameRequest = 101, 
        S_EnterGameResponse = 102, 
        C_SelectCharacter = 105,
        S_SelectCharacter = 106,

        // BroadCast 용
        C_BroadcastMove = 111,
        S_BroadcastMove = 112,
        S_BroadcastEnter = 113,
        S_BroadcastMonstermove = 120,
       

        // 전투 관련
        C_AttackData = 131,
        S_BroadcastAttack = 132, 
        C_CastAttack = 133, 
        S_BroadcastCastAttack = 134,
        S_BroadcastDamage = 135,
        S_BroadcastDead = 136,

        S_BroadcastPlayerDie = 141, 
        C_PlayerReviveRequest = 142, 
        S_PlayerReviveResponse = 143, 
        S_BroadcastPlayerRespawn = 144,

        S_UpdateStatus = 151,
        S_BroadcastLevelUp = 152,

        // 몬스터 관련 
        S_MonsterList = 200,
        S_RespawnMonsterList = 201,

        S_Error = 901,
    }
    public enum ErrorCode
    {
        None = 0,
        AlreadyLoggedIn = 1001,
        InvalidCredentials = 1002,
        InvalidName = 1003,
        NotLoggedIn = 1004,
        NotInRoom = 2001,
        AlreadyInRoom = 2002,
        Unknown = 9999,
    }
}
