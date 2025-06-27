using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    // 1 ~ 99 : 로그인 / 인증
    // 100 ~ 199 : 로비 관련 
    // 200 ~ 299 : 게임 관련
    // 300 ~ 399 : 전투 관련
    // 400 ~ 499 : 핑 관련 
    // 500 ~ 599 : 이동 관련
    // 900 ~ 999 : 에러 관련
    // 9000 ~ 9999 : 테스트용/디버그
    public enum PacketType : ushort
    {
        LoginRequest = 1,
        LoginResponse = 2,

        // 게임 관련
        C_EnterGame = 201,
        S_EnterGame = 202,
        S_PlayerEntered = 203,
        C_LeaveGame = 204,

        C_LoadVillageDataRequest = 211,
        S_LoadVillageDataResponse = 212,
        C_SaveVillageData = 213,

        C_SavePlantedCrop = 221,
        C_DestroyPlantedCrop = 222,

        // 핑관련
        C_Ping = 301,
        S_Pong = 301,

        // 에러 관련 

        C_Move = 501,
        S_Move = 502,

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
