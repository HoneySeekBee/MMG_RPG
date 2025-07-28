using MMG_API.Repositories.Interfaces;
using MMG_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore; // 추가!
using System.Text;
using MMG_API.Data;
using System.Text.Json;

namespace MMG_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.UseUrls($"https://localhost:{GetPortNumber(args)}");

            Console.WriteLine($"https://localhost:{GetPortNumber(args)} 시작" );

            // DbContext 등록 (여기 추가!)
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

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication(); // 인증 추가
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
        private static int GetPortNumber(string[] args)
        {
            string serverName = args.Length > 0 ? args[0] : "API";
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
