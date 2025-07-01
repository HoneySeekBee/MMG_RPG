using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientSession : PacketSession
{
    private SendQueue _sendQueue = new();

    public override void Send(byte[] sendBuffer)
    {
        _sendQueue.Send(_socket, new ArraySegment<byte>(sendBuffer, 0, sendBuffer.Length));
    }
    public override async void OnConnected(EndPoint endPoint)
    {
        Debug.Log($"[Connected to Server] {endPoint}");
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Debug.Log($"[Disconnected from Server] {endPoint}");
    }
    public override void OnSend(int numOfBytes)
    {
        Debug.Log($"[Send] {numOfBytes} bytes");
    }

    protected override void OnRecvPacketInternal(ArraySegment<byte> buffer)
    {
        PacketManager.OnRecvPacket(this, buffer);
    }

}
