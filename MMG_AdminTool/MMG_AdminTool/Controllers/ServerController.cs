using Microsoft.AspNetCore.Mvc;
using MMG_AdminTool.Models;
using MMG_AdminTool.Services;
using System.Text.Json;

namespace MMG_AdminTool.Controllers
{
    public class ServerController : Controller
    {
        private readonly RedisConnectionManager _redisManager; 
        
        public ServerController(RedisConnectionManager redisManager)
        {
            _redisManager = redisManager;
        }
        [HttpPost]
        public IActionResult Start(string name)
        {
            // JSON 경로 읽기
            string configPath = @"C:\Users\USER\OneDrive\바탕 화면\MMG\MMG_RPG\MMG_RPG\ServerConfig.json";
            var json = System.IO.File.ReadAllText(configPath);
            var configs = JsonSerializer.Deserialize<List<ServerConfig>>(json)!;

            // 실행할 서버 찾기
            var cfg = configs.FirstOrDefault(c =>
                c.ServerName.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (cfg == null || string.IsNullOrEmpty(cfg.ExePath))
                return RedirectToAction("Index");

            // exe 실행
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = cfg.ExePath,
                WorkingDirectory = Path.GetDirectoryName(cfg.ExePath)!,
                UseShellExecute = true,
                Arguments = name // 서버 이름을 실행 인자로 전달
            };

            Console.WriteLine($"[Start] {name} 실행: {cfg.ExePath}");
            System.Diagnostics.Process.Start(psi);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Stop(string name)
        {
            string configPath = @"C:\Users\USER\OneDrive\바탕 화면\MMG\MMG_RPG\MMG_RPG\ServerConfig.json";
            var json = System.IO.File.ReadAllText(configPath);
            var configs = JsonSerializer.Deserialize<List<ServerConfig>>(json)!;

            var cfg = configs.FirstOrDefault(c => c.ServerName == name);
            if (cfg == null || string.IsNullOrEmpty(cfg.ExePath))
                return RedirectToAction("Index");

            string processName = Path.GetFileNameWithoutExtension(cfg.ExePath);
            var processes = System.Diagnostics.Process.GetProcessesByName(processName);
            foreach (var p in processes)
            {
                try
                {
                    p.Kill();
                }
                catch (Exception ex)
                {
                    // 권한 문제나 이미 종료된 경우는 무시
                    Console.WriteLine($"[Stop] {p.ProcessName} 종료 실패: {ex.Message}");
                }
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult GetAPILog(string name)
        {
            var db = _redisManager.GetDatabase(); // RedisConnectionManager 사용
            if (db == null)
                return Json(new { error = "Redis not connected" });

            string redisKey = $"{name}:RequestLogs";
            var values = db.ListRange(redisKey, -100, -1);

            var logs = values
                .Select(x => JsonSerializer.Deserialize<ApiLog>(x!)!)
                .Reverse()
                .ToList();

            return Json(logs);
        }
        [HttpGet]
        public IActionResult GetMainLog(string name)
        {
            var db = _redisManager.GetDatabase(); // RedisConnectionManager 사용
            if (db == null)
                return Json(new { error = "Redis not connected" });

            string redisKey = $"{name}:RequestLogs";
            var values = db.ListRange(redisKey, -100, -1);

            var logs = values
                .Select(x => JsonSerializer.Deserialize<MainLog>(x!)!)
                .Reverse()
                .ToList();

            return Json(logs);
        }
        [HttpGet]
        public IActionResult GetChatLogs(int roomId, string date)
        {
            var db = _redisManager.GetDatabase();
            if (db == null)
                return Json(new { error = "Redis not connected" });

            string key = $"chat:{date}:room:{roomId}";
            var values = db.ListRange(key, 0, -1);

            var logs = values
                .Select(x =>
                {
                    var parts = x.ToString().Split('|', 4);
                    return new ChatLog
                    {
                        Time = DateTime.Parse(parts[0]),
                        CharId = int.Parse(parts[1]),
                        Nickname = parts[2],
                        Message = parts[3]
                    };
                })
                .ToList();

            return Json(logs);
        }

        public class ChatLog
        {
            public DateTime Time { get; set; }
            public int CharId { get; set; }
            public string Nickname { get; set; }
            public string Message { get; set; }
        }
        public class MainLog
        {
            public DateTime Time{ get; set; }
            public string Message { get; set; }
        }

        public class ApiLog
        {
            public string Path { get; set; }
            public string Method { get; set; }
            public string Ip { get; set; }
            public DateTime Time { get; set; }
        }

        private bool IsProcessRunning(string processName)
        {
            return System.Diagnostics.Process.GetProcessesByName(processName).Any();
        }
        public IActionResult Index()
        {
            string configPath = @"C:\Users\USER\OneDrive\바탕 화면\MMG\MMG_RPG\MMG_RPG\ServerConfig.json";
            var json = System.IO.File.ReadAllText(configPath);
            var configs = JsonSerializer.Deserialize<List<ServerConfig>>(json)!;

            var servers = new List<ServerStatusViewModel>();

            foreach (var cfg in configs)
            {
                string processName = cfg.ServerName switch
                {
                    "API" => "MMG_API",
                    "Main" => "GameServer",
                    "Chat" => "MMG_Chat_Server",
                    "Redis" => "redis-server",
                    _ => ""
                };

                servers.Add(new ServerStatusViewModel
                {
                    Name = cfg.ServerName,
                    Port = cfg.PortNumber,
                    Status = string.IsNullOrEmpty(processName) ? "Unknown" :
                             (IsProcessRunning(processName) ? "Alive" : "Dead"),
                    ConnectedClients = 0 // Redis 연동하면 여기서 실제값
                });
            }
            return View(servers);
        }
    }
}
