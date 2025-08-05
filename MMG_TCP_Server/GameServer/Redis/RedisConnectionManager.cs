using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Redis
{
    public class RedisConnectionManager
    {
        private readonly ConnectionMultiplexer _connection;

        public RedisConnectionManager(string configuration)
        {
            try
            {
                _connection = ConnectionMultiplexer.Connect(configuration);
                Console.WriteLine($"Redis 연결 성공 : {configuration}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Redis 연결 실패 : {ex.Message}");
                throw;
            }
        }
        public IDatabase GetDatabase(int db = -1)
        {
            return _connection.GetDatabase(db);
        }
        public ISubscriber GetSubscriber()
        {
            return _connection.GetSubscriber(); // pub/sub 용도
        }

        public bool IsConnected()
        {
            return _connection?.IsConnected ?? false;
        }
        public void Close()
        {
            _connection?.Close();
            Console.WriteLine("Redis 연결 종료됨 ");
        }
    }
}
