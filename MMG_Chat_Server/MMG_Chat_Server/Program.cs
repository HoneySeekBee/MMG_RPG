using System.Net.Sockets;
using System.Net;
using MMG_Chat_Server.Main;
using Microsoft.AspNetCore.Builder;
using MMG_Chat_Server.Protos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using StackExchange.Redis;
using System.Text.Json;


namespace MMG_Chat_Server
{
    public class Program
    {
        public static Redis _Redis;
        static void Main(string[] args)
        {
            // gRPC용 서버 시작 
            StartGrpcServer(args);

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
        static IPEndPoint GetEndPoint(int ChatServerPortNumber)
        {
            IPEndPoint endPoint = new IPEndPoint(GetLocalIp(), ChatServerPortNumber);
            return endPoint;
        }
        static IPAddress GetLocalIp()
        {
            string host = Dns.GetHostName();
            var ipAddr = Dns.GetHostEntry(host)
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            return ipAddr;
        }
        static void StartGrpcServer(string[] args)
        {
            InitRedis().GetAwaiter().GetResult();

            string localIp = GetLocalIp().ToString();

            var builder = WebApplication.CreateBuilder();
            int gRPC_PortNumber = GetPortNumber("ChatGrpc");
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Parse(localIp), gRPC_PortNumber, listenOptions =>
                {
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                });
            });

            builder.Services.AddGrpc();

            var app = builder.Build();
            app.MapGrpcService<ChatSyncService>();

            Task.Run(() =>
            {
                PacketManager.Register();
                Listener listener = new Listener();
                int portNumber = GetPortNumber("Chat");
                listener.Init(GetEndPoint(portNumber), () => new ChatSession());
                Console.WriteLine($"TCP 채팅 서버 시작됨... {portNumber}");
            });

            Console.WriteLine($"gRPC 서버 시작: http://{localIp}:{gRPC_PortNumber}");
            app.Run();
        }
        static async Task InitRedis()
        {
            var connection = await ConnectionMultiplexer.ConnectAsync($"localhost:{GetPortNumber("ChatRedis")}");
            Console.WriteLine("Redis connected!");
            _Redis = new Redis(connection.GetDatabase());
            _Redis.BackupCheck();
        }
        private static int GetPortNumber(string ServerName)
        {
            string configPath = @"C:\Users\USER\OneDrive\바탕 화면\MMG\MMG_RPG\MMG_RPG\ServerConfig.json";

            var json = File.ReadAllText(configPath);
            var configs = JsonSerializer.Deserialize<List<ServerConfig>>(json);
            var myConfig = configs!.FirstOrDefault(c => c.ServerName == ServerName);
            if (myConfig == null)
            {
                Console.WriteLine($"[ERROR] {ServerName} 설정을 찾을 수 없습니다.");
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
