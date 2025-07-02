using Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : GlobalSingleton<GameManager>
{
    public int UserId { get; private set; }
    public string Email { get; private set; }
    public string Nickname { get; private set; }

    public void SetUser(int userId, string email, string nickname)
    {
        UserId = userId;
        Email = email;
        Nickname = nickname;
    }

    public bool IsLoggedIn()
    {
        return UserId > 0 && !string.IsNullOrEmpty(Email);
    }

    public void Logout()
    {
        UserId = 0;
        Email = null;
        Nickname = null;
        PlayerPrefs.DeleteKey("jwt_token");

        Debug.Log("[로그아웃] 완료");
    }
}
