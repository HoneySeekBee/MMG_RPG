using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Domain
{
    public class GameRoomManager
    {
        public static GameRoomManager Instance { get; } = new GameRoomManager();

        private Dictionary<int, GameRoom> _rooms = new();
        private int _roomIdCounter = 1;
        public GameRoom CreateRoom(int roomId)
        {
            var room = new GameRoom(roomId);
            _rooms[roomId] = room;
            Console.WriteLine($"[GameRoomManager] GameRoom #{roomId} 생성");
            return room;
        }
        public GameRoom GetRoom(int roomId)
        {
            _rooms.TryGetValue(roomId, out var room);
            return room;
        }

        public IEnumerable<GameRoom> GetAllRooms()
        {
            return _rooms.Values;
        }

        public void RemoveRoom(int roomId)
        {
            if (_rooms.Remove(roomId))
            {
                Console.WriteLine($"[GameRoomManager] GameRoom #{roomId} 제거됨");
            }
        }

        public void Update()
        {
            foreach (var room in _rooms.Values)
            {
                room.Update();
            }
        }
    }
}
