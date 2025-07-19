using GameServer.Attack;
using GameServer.Core;
using GameServer.Data;
using GameServer.Data.Monster;
using GameServer.Game.Room;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    public class Program
    {
        public static string URL = "https://localhost:7132";
        static void Main(string[] args)
        {
            Init();

            Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();
                long last = sw.ElapsedMilliseconds;

                while (true)
                {
                    long now = sw.ElapsedMilliseconds;
                    float deltaTime = (now - last) / 1000f;
                    last = now;

                    GameRoomManager.Instance.Update(deltaTime); // 시간 기반 로직 처리
                    GameRoomManager.Instance.Update();          // JobQueue 처리

                    Thread.Sleep(50);
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
        public static async Task Init()
        {
            await SkillDataManager.LoadAttackData();
            await MonsterDataManager.LoadData();
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
