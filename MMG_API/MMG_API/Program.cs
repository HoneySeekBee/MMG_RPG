using MMG_API.Repositories.Interfaces;
using MMG_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore; // �߰�!
using System.Text;
using MMG_API.Data;
using System.Text.Json;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;

namespace MMG_API
{
    public class Program
    {
        private const string ServerName = "API";
        private static StackExchange.Redis.IDatabase RedisDb;
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            int apiPort = GetPortNumber(ServerName);
            builder.WebHost.UseUrls($"https://localhost:{apiPort}");

            Console.WriteLine($"https://localhost:{apiPort} ����" );
            RedisDb = Connect_Redis();
            await SaveInitData_Redis(apiPort);
            // DbContext ��� (���� �߰�!)
            builder.Services.AddDbContext<MMGDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
            });

            builder.Services.AddSingleton(RedisDb);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();

            app.UseAuthentication(); // ���� �߰�
            app.UseAuthorization();

            app.MapControllers();

            End_API_Redis(app); 
            app.UseMiddleware<RequestLoggingMiddleware>();

            app.Run();
        }
        private static StackExchange.Redis.IDatabase Connect_Redis()
        {
            try
            {
                var redis = ConnectionMultiplexer.Connect($"localhost:{GetPortNumber("Redis")}");
                return redis.GetDatabase();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis ���� ����] {ex.Message}");
                return null!;
            }
        }
        private static async Task SaveInitData_Redis(int apiPort)
        {
            RedisDb.KeyDelete($"ServerStatus:{ServerName}");
            RedisDb.KeyDelete("API:RequestLogs");

            // ��û ����
            var log = new
            {
                Path = Process.GetCurrentProcess().Id.ToString(),
                Method = "Alive",
                Ip = "127.0.0.1",
                Time = DateTime.UtcNow
            };

            // Redis ����Ʈ�� Ǫ��
            Console.WriteLine($"[Invoe] : {log.Path}, {log.Ip}, {log.Time} ");
            await RedisDb.ListRightPushAsync("API:RequestLogs", JsonSerializer.Serialize(log));

        }
        private static void End_API_Redis(WebApplication app)
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                try
                {
                    RedisDb.KeyDelete($"ServerStatus:{ServerName}");
                    RedisDb.KeyDelete("API:RequestLogs");
                    Console.WriteLine("[Redis] Keys deleted on ProcessExit");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Redis Key ���� ����] {ex.Message}");
                }
            };
            app.Lifetime.ApplicationStopping.Register(() =>
            {
                try
                {
                    RedisDb.KeyDelete($"ServerStatus:{ServerName}");
                    RedisDb.KeyDelete("API:RequestLogs");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Redis Key ���� ����] {ex.Message}");
                }
            });
        }

        private static int GetPortNumber(string serverName)
        {
            string configPath = @"C:\Users\USER\OneDrive\���� ȭ��\MMG\MMG_RPG\MMG_RPG\ServerConfig.json";

            var json = File.ReadAllText(configPath);
            var configs = JsonSerializer.Deserialize<List<ServerConfig>>(json);
            var myConfig = configs!.FirstOrDefault(c => c.ServerName == serverName);
            if (myConfig == null)
            {
                Console.WriteLine($"[ERROR] {serverName} ������ ã�� �� �����ϴ�.");
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
