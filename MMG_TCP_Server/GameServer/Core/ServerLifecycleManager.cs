using GameServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Core
{
    public static class ServerLifecycleManager
    {
        private static bool _isInitialized = false;

        public static void Initialize(int mainPort)
        {
            if (_isInitialized)
                return;

            Console.WriteLine("ServerlifecycleManager 초기화 중...");
            ServerStatusReporter.ReportAlive(mainPort);

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            _isInitialized = true;
            Console.WriteLine("서버 상태 모니터링 시작");
        }

        private static void OnProcessExit(object? sender, EventArgs e)
        {
            Console.WriteLine("서버 종료. 상태 보고중");
            try
            {
                ServerStatusReporter.ReportShutdown();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] 서버 종료 보고 실패: {ex.Message}");
            }
        }
        public static void Shutdown()
        {
            Console.WriteLine("서버 수동 종료 요청");
            ServerStatusReporter.ReportShutdown();
        }
    }
}
