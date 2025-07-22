using GamePacket;
using MMG.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RevivePopup : PopupBase
{
    private int playerid = -1;
    public override void Open()
    {
        base.Open();
    }
    public void SetPlayerId(int _playerid)
    {
        playerid = _playerid;
        Button reviveRequestBtn = GetComponentInChildren<Button>();
        reviveRequestBtn.onClick.AddListener(ReviveRequest);
    }
    public void ReviveRequest()
    {
        // 패킷보내기 
        if (playerid > 0)
        {
            PlayerId packet = new PlayerId()
            {
                PlayerId_ = playerid,
            };
            NetworkManager.Instance.Send_PlayerReviveRequest(packet);
        }
        else
        {
            Debug.LogError("부활 실패");
        }
        Close();
    }
}