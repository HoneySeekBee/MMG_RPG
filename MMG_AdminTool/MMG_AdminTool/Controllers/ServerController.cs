using Microsoft.AspNetCore.Mvc;
using MMG_AdminTool.Models;

namespace MMG_AdminTool.Controllers
{
    public class ServerController : Controller
    {
        [HttpPost]
        public IActionResult Start(string name)
        {
            string exePath = name switch
            {
                "API" => @"C:\Users\USER\OneDrive\바탕 화면\MMG\MMG_RPG\MMG_RPG\MMG_API\MMG_API\bin\Debug\net6.0\MMG_API.exe",
                "Main" => @"C:\Users\USER\OneDrive\바탕 화면\MMG\MMG_RPG\MMG_RPG\MMG_TCP_Server\GameServer\bin\Debug\net6.0\GameServer.exe",
                "Chat" => @"C:\Users\USER\OneDrive\바탕 화면\MMG\MMG_RPG\MMG_RPG\MMG_Chat_Server\MMG_Chat_Server\bin\Debug\net8.0\MMG_Chat_Server.exe",
                _ => ""
            };

            if (!string.IsNullOrEmpty(exePath))
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = Path.GetDirectoryName(exePath)!,
                    UseShellExecute = true
                };

                System.Diagnostics.Process.Start(psi);
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Stop(string name)
        {
            string processName = name switch
            {
                "Main" => "GameServer",
                "Chat" => "MMG_Chat_Server",
                "API" => "MMG_API",
                _ => ""
            };

            if (!string.IsNullOrEmpty(processName))
            {
                var processes = System.Diagnostics.Process.GetProcessesByName(processName);
                foreach (var p in processes)
                    p.Kill();
            }

            return RedirectToAction("Index");
        }
        private bool IsProcessRunning(string processName)
        {
            return System.Diagnostics.Process.GetProcessesByName(processName).Any();
        }
        public IActionResult Index()
        {
            var servers = new List<ServerStatusViewModel>
    {
        new ServerStatusViewModel
        {
            Name = "API",
            Port = 5000, // 실제 API 서버 포트로 맞춰 줘
            Status = IsProcessRunning("MMG_API") ? "Alive" : "Dead",
            ConnectedClients = 0
        },
        new ServerStatusViewModel
        {
            Name = "Main",
            Port = 7777,
            Status = IsProcessRunning("GameServer") ? "Alive" : "Dead",
            ConnectedClients = 0
        },
        new ServerStatusViewModel
        {
            Name = "Chat",
            Port = 8888,
            Status = IsProcessRunning("MMG_Chat_Server") ? "Alive" : "Dead",
            ConnectedClients = 0
        }
    };

            return View(servers);
        }
    }
}
