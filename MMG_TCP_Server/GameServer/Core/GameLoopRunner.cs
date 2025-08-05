using GameServer.Game.Room;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Core
{
    public static class GameLoopRunner
    {
        private static bool _running = false;

        public static void Start()
        {
            if (_running)
                return;

            _running = true;

            Task.Run(() =>
            {
                Console.WriteLine("GameLoop 시작");

                Stopwatch sw = Stopwatch.StartNew();
                long last = sw.ElapsedMilliseconds;

                while (_running)
                {
                    long now = sw.ElapsedMilliseconds;
                    float deltaTime = (now - last) / 1000f;
                    last = now;

                    try
                    {
                        GameRoomManager.Instance.Update(deltaTime);
                        GameRoomManager.Instance.Update();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[GameLoop] 예외 발생 : {ex}");
                    }

                    Thread.Sleep(50);
                }

                Console.WriteLine("GameLoop 종료 ");
            });
        }
        public static void Stop()
        {
            _running = false;
        }
    }
}
