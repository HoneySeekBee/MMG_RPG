using Packet;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient
{
    public class ClientSession : PacketSession
    {
        private SendQueue _sendQueue = new(); 
        public override void Send(byte[] sendBuffer)
        {
            _sendQueue.Send(_socket, new ArraySegment<byte>(sendBuffer, 0, sendBuffer.Length));
        }
        public override async void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Connected to Server] {endPoint}");

            // 예시: 로그인 요청 보내기
            var loginReq = new LoginRequest()
            {
                UserId = "a@email.com",
                Password = "1234"
            };
            Send(PacketType.C_LoginRequest, loginReq);

        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Disconnected from Server] {endPoint}");
        }


        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"[Send] {numOfBytes} bytes");
        }

        protected override void OnRecvPacketInternal(ArraySegment<byte> buffer)
        {
            ClientPacketManager.OnRecvPacket(this, buffer);
        }
    }
}
