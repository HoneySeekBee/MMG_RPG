using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Services
{
    public class RedisConnectionManager
    {
        private readonly string _connectionString;
        private ConnectionMultiplexer? _connection;
        private readonly Timer _retryTimer;

        public RedisConnectionManager(string connectionString)
        {
            _connectionString = connectionString;

            // 5초마다 연결 확인 및 재연결 시도
            _retryTimer = new Timer(async _ => await EnsureConnectedAsync(), null, 0, 5000);
        }

        public IDatabase? GetDatabase()
        {
            if (_connection != null && _connection.IsConnected)
                return _connection.GetDatabase();
            return null; // Redis가 아직 연결 안 됐으면 null 반환
        }
        private async Task EnsureConnectedAsync()
        {
            if (_connection == null || !_connection.IsConnected)
            {
                try
                {
                    Console.WriteLine("[Redis] 재연결 시도...");
                    _connection = await ConnectionMultiplexer.ConnectAsync(_connectionString);
                    Console.WriteLine("[Redis] 연결 성공");
                }
                catch
                {
                    // 실패 시 5초 후 재시도 (Timer가 자동으로 반복)
                }
            }
        }
    }
}
