using Packet;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient
{
    public class PacketHandler
    {
        public static void LoginResponseHandler(ClientSession session, LoginResponse response)
        {
            Console.WriteLine($"[LoginResponse] Success: {response.Success} {response.Message}");


            if (response.Success)
            {
                C_EnterGame enterPacket = new C_EnterGame();
                enterPacket.Name = "TestUser";

                session.Send(PacketType.C_EnterGame, enterPacket); // PacketSession.Send 호출
            }
        }
        public static void S_PlayerEnteredHandler(ClientSession session, S_PlayerEntered response)
        {
            Console.WriteLine($"{response.Name}님이 방에 입장하였습니다.");
        }
        public static void S_EnterGameHandler(ClientSession session, S_EnterGame response)
        {
            Console.WriteLine("[서버 응답] 게임 입장 성공!");
            Console.WriteLine($"플레이어 ID: {response.PlayerId}");
            Console.WriteLine($"이름: {response.Name}");
            Console.WriteLine($"위치: ({response.PosX}, {response.PosY})");
            //PlayerInfo player = new 

            if (response.Success)
            {
                C_Ping ping = new C_Ping()
                {
                    Message = "Hello Server!"
                };
                session.Send(PacketType.C_Ping, ping);
            }
            
        }

        public static void S_PongHandler(ClientSession session, S_Pong packet)
        {
            Console.WriteLine($"[Pong 응답] 서버 메시지 : {packet.Message}");
        }
        public static void S_ErrorHandler(ClientSession session, S_Error pkt)
        {
            Console.WriteLine($"[Error] Code: {pkt.ErrorCode}, Message: {pkt.ErrorMessage}");
        }
    }
}
