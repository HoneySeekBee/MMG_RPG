using System.Net.Sockets;
using System.Net;
using MMG_Chat_Server.Main;
using Microsoft.AspNetCore.Builder;
using MMG_Chat_Server.Protos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using StackExchange.Redis;


namespace MMG_Chat_Server
{
    public class Program
    {
        public static Redis _Redis;
        static void Main(string[] args)
        {
            // gRPC용 서버 시작 
            StartGrpcServer();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
        static IPEndPoint GetEndPoint()
        {
            IPEndPoint endPoint = new IPEndPoint(GetLocalIp(), 7778);
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
        static void StartGrpcServer()
        {
            InitRedis().GetAwaiter().GetResult();

            string localIp = GetLocalIp().ToString();

            var builder = WebApplication.CreateBuilder();
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Parse(localIp), 7779, listenOptions =>
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
                listener.Init(GetEndPoint(), () => new ChatSession());
                Console.WriteLine("TCP 채팅 서버 시작됨...");
            });

            Console.WriteLine($"gRPC 서버 시작: http://{localIp}:7779");
            app.Run();
        }
        static async Task InitRedis()
        {
            var connection = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
            Console.WriteLine("Redis connected!");
            _Redis = new Redis(connection.GetDatabase());
            _Redis.BackupCheck();
        }
    }
}
