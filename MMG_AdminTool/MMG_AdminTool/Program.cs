using MMG_AdminTool.Models;
using MMG_AdminTool.Services;
using System.Text.Json;
using System.Xml.Linq;

namespace MMG_AdminTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllersWithViews(); // 컨트롤러와 뷰에 대한 지원 

            int redisPortNumber = GetPortNumber("Redis");
            builder.Services.AddSingleton(new RedisConnectionManager($"localhost:{redisPortNumber}"));
            int gRPCPortNumber = GetPortNumber("ChatGrpc");
            string grpcUrl = $"http://localhost:{gRPCPortNumber}";
            builder.Services.AddSingleton(new GrpcChatClient(grpcUrl));

                var app = builder.Build();
            
            app.MapControllerRoute(
                name:"default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
                );


            app.Run();
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
    }
}
