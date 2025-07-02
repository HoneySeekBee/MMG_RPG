using GameServer.Domain;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Core
{
    public class ServerSession : PacketSession
    {
        public int SessionId { get; set; }
        public Player MyPlayer { get; set; }
        private SendQueue _sendQueue = new();
        public string jwtToken { get; set; }
        public bool IsLoggedIn { get; private set; } = false;

        public override void Send(byte[] sendBuffer)
        {
            _sendQueue.Send(_socket, new ArraySegment<byte>(sendBuffer, 0, sendBuffer.Length));
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Connected] {endPoint}");
            SessionManager.Add(SessionId, this);
            MyPlayer = new Player()
            {
                Session = this
            };
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Disconnected] {endPoint}");

            // SessionManager가 유저 제거까지 처리함
            SessionManager.Remove(SessionId);

            MyPlayer?.OnDisconnected(); // GameRoom 등 게임 내부 정리
            MyPlayer = null;
        }
        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"[Send] {numOfBytes} bytes");
        }

        protected override void OnRecvPacketInternal(ArraySegment<byte> buffer)
        {
            ServerPacketManager.OnRecvPacket(this, buffer);
        }
        public void SetLoginStatus(bool status)
        {
            IsLoggedIn = status;
        }
    }
}