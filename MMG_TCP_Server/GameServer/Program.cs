using GameServer.Core;
using GameServer.Domain;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    GameRoomManager.Instance.Update();
                    Thread.Sleep(50); // 프레임 간격
                }
            });
            ServerPacketManager.Register();
            Listener listener = new Listener();
            listener.Init(GetEndPoint(), () => new ServerSession());

            Console.WriteLine("서버 시작됨...");
            while (true)
            {
                Thread.Sleep(1000);
            }
        }
        static IPEndPoint GetEndPoint()
        {
            string host = Dns.GetHostName();
            IPAddress ipAddr = Dns.GetHostEntry(host)
    .AddressList
    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
            return endPoint;
        }
    }
}
