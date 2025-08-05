using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Util
{
    public static class PortUtil
    {
        private static readonly Dictionary<string, int> _portCache = new();
        private static string _configPath = @"C:\Users\USER\OneDrive\바탕 화면\MMG\MMG_RPG\MMG_RPG\ServerConfig.json";

        public static int GetPort(string serverName)
        {
            if (_portCache.TryGetValue(serverName, out int cachec))
                return cachec;

            try
            {
                var json = File.ReadAllText(_configPath);
                var configs = JsonSerializer.Deserialize<List<ServerConfig>>(json);

                var target = configs?.FirstOrDefault(c => c.ServerName == serverName);
                if(target == null)
                {
                    Console.WriteLine($"{serverName} 설정이 없습니다.");
                    return 7132;
                }

                _portCache[serverName] = target.PortNumber;
                return target.PortNumber;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] [PortUtil] : {ex}");
                return 7132;
            }
        }
        public static void SetConfigPath(string path)
        {
            _configPath = path;
        }
        public class ServerConfig
        {
            public string ServerName { get; set; }
            public string ExePath { get; set; }
            public int PortNumber { get; set; }
        }
    }
}
