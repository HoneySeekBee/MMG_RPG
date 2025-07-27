using MMG_Chat_Server.Game;
using MMG_Chat_Server.Room;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MMG_Chat_Server.Main
{
    public class ChatSession : PacketSession
    {
        private SendQueue _sendQueue = new();
        public int SessionId { get; set; }
        public string jwtToket{ get; set; }
        public UserObject MyPlayer { get; set; }
        public ChatRoom Room{ get; set; }

        public override void Send(byte[] sendBuffer)
        {
            _sendQueue.Send(_socket, new ArraySegment<byte>(sendBuffer, 0, sendBuffer.Length));
        }
        
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Connected] {endPoint}");
            SessionManager.Add(SessionId, this);
        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Disconnected] {endPoint}");

            // SessionManager가 유저 제거까지 처리함
            SessionManager.Remove(SessionId);

            //MyPlayer?.OnDisconnected(); // GameRoom 등 게임 내부 정리
            //MyPlayer = null;
        }
        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"[Send] {numOfBytes} bytes");
        }

        protected override async void OnRecvPacketInternal(ArraySegment<byte> buffer)
        {
            await PacketManager.OnRecvPacket(this, buffer);
        }
    }
}
