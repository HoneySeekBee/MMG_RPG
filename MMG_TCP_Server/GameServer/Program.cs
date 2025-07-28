using GameServer.Attack;
using GameServer.ChatServer;
using GameServer.Core;
using GameServer.Data;
using GameServer.Data.Monster;
using GameServer.Game.Room;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace GameServer
{
    public class Program
    {
        public static string URL {get { return $"https://localhost:{API_Port}"; } } 
        public static string GRPC_URL {get { return $"http://192.168.219.67:{ChatGRPC_Port}"; } } 
        public static int API_Port { get {
                if(apiPort < 0)
                {
                    apiPort = GetPortNumber("API");
                    Console.WriteLine($"[PortNumber] (API) {apiPort}");
                }
                return apiPort;
            } }
        private static int apiPort = -1;
        
        public static int ChatGRPC_Port{get
            {
                if (chatGRPC_Port < 0)
                {
                    chatGRPC_Port = GetPortNumber("ChatGrpc");
                    Console.WriteLine($"[PortNumber] (ChatGrpc) {chatGRPC_Port}");
                }
                return chatGRPC_Port;
            } }
        private static int chatGRPC_Port = -1;
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
            IPEndPoint endPoint = new IPEndPoint(ipAddr, GetPortNumber("Main"));
            return endPoint;
        }
        private static int GetPortNumber(string serverName)
        {
            string configPath = @"C:\Users\USER\OneDrive\바탕 화면\MMG\MMG_RPG\MMG_RPG\ServerConfig.json";

            var json = File.ReadAllText(configPath);
            var configs = JsonSerializer.Deserialize<List<ServerConfig>>(json);
            var myConfig = configs!.FirstOrDefault(c => c.ServerName == serverName);
            if (myConfig == null)
            {
                Console.WriteLine($"[ERROR] {serverName} 설정을 찾을 수 없습니다.");
                return 7132;
            }

            return myConfig.PortNumber;
        }
        public class ServerConfig
        {
            public string ServerName { get; set; }
            public string ExePath { get; set; }
            public int PortNumber { get; set; }
        }
    }
}
