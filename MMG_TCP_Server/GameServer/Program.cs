using GameServer.Attack;
using GameServer.ChatServer;
using GameServer.Core;
using GameServer.Data;
using GameServer.Data.Monster;
using GameServer.Game.Room;
using GameServer.Services;
using GameServer.Util;
using Google.Protobuf.WellKnownTypes;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using static GameServer.Util.PortUtil;

namespace GameServer
{
    public class Program
    {
        public static RedisConnectionManager Redis { get; private set; }
        public static string URL => $"https://localhost:{PortUtil.GetPort("API")}";
        public static string GRPC_URL => $"http://192.168.219.67:{PortUtil.GetPort("ChatGrpc")}";


        static async Task Main(string[] args)
        {
            Console.WriteLine("서버 시작 중...");

            InitializeRedis();
            ServerLifecycleManager.Initialize(PortUtil.GetPort("Main"));
            await DataInitializer.InitAsync();

            ServerPacketManager.Register();
            GameLoopRunner.Start();
            StartSocketListener();

            Console.WriteLine("서버가 시작되었습니다.");

            await Task.Delay(-1);
        }
        static void InitializeRedis()
        {
            int redisPort = PortUtil.GetPort("Redis");
            Redis = new RedisConnectionManager($"localhost:{redisPort}");
        }
        static void StartSocketListener()
        {
            var listener = new Listener();
            listener.Init(GetEndPoint(), () => new ServerSession());
            Console.WriteLine("TCP 리스너 시작됨");
        }

        static IPEndPoint GetEndPoint()
        {
            string host = Dns.GetHostName();
            var ipAddr = Dns.GetHostEntry(host).AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            return new IPEndPoint(ipAddr, PortUtil.GetPort("Main"));
        }
    }
}
