using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameServer.Services
{
    public class ServerStatusReporter
    {
        private static void PushLog(string message)
        {
            var db = Program.Redis.GetDatabase();
            if (db == null)
            {
                Console.WriteLine("[ServerStatusReporter] Redis가 연결되지 않음");
                return;
            }
            string key = "Main:RequestLogs";

            var log = new
            {
                Time = DateTime.UtcNow,
                Message = message
            };

            db.ListRightPush(key, JsonSerializer.Serialize(log));
            db.ListTrim(key, -500, -1); // 최근 500개만 유지
        }

        public static void ReportAlive(int port)
        {
            string key = $"Main:RequestLogs";
            var db = Program.Redis.GetDatabase();
            if (db == null) return;
            db.KeyDelete(key);
            
            string message = $"서버 실행 : [Port : {port}], [PID : {Process.GetCurrentProcess().Id}]";
            PushLog(message);
        }

        public static void ReportShutdown()
        {
            string message = $"서버 종료";
            PushLog(message);

        }
        public static void ReportEvent(string message)
        {
            PushLog(message);
        }
    }
}
