using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMG_Chat_Server.Room
{
    public class ChatRoomManager
    {
        public static ChatRoomManager Instance { get; } = new ChatRoomManager();
        private Dictionary<int, ChatRoom> _rooms = new();
        private Dictionary<int, List<int>> _roomsPlayers = new();
        private Dictionary<int, int> _playerRoom = new(); // (character id, roomid)
        //private int _roomIdCounter = 1;
        private readonly object _lock = new();

        public ChatRoom CreateRoom(int roomId)
        {
            var room = new ChatRoom(roomId);
            _rooms[roomId] = room;
            Console.WriteLine($"[ChatRoomManager] ChatRoom {roomId} 생성");
            _roomsPlayers[roomId] = new List<int>();
            return room;
        }
        public int FindRoomId(int characterId)
        {
            lock (_lock)
            {
                return _playerRoom.TryGetValue(characterId, out var roomId) ? roomId : -1;
            }
        }
        public void EnterRoom(int roomId, int characterId)
        {
            lock (_lock)
            {
                _roomsPlayers[roomId].Add(characterId);
                _playerRoom[characterId] = roomId;
            }
        }
        public void ExitRoom(int roomId, int characterId)
        {
            lock (_lock)
            {
                _roomsPlayers[roomId].Remove(characterId);
                _playerRoom.Remove(characterId);
            }
        }
        public ChatRoom GetRoom(int roomId)
        {
            _rooms.TryGetValue(roomId, out var room);
            return room;
        }
        public IEnumerable<ChatRoom> GetAllRooms()
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

        //public void Update()
        //{
        //    foreach (var room in _rooms.Values)
        //    {
        //        room.Update();
        //    }
        //}

        //public void Update(float deltaTime)
        //{
        //    foreach (var room in _rooms.Values)
        //    {
        //        room.Update(deltaTime);
        //    }
        //}
        public ChatRoom GetOrCreateRoom(int mapId)
        {
            if (!_rooms.TryGetValue(mapId, out var room))
            {
                room = CreateRoom(mapId);
            }
            return room;
        }
        public void SendChat(string type, string message)
        {
            PacketType packetType = type == "System" ? PacketType.S_SystemChat : PacketType.S_AdminChat;
            foreach(var room in _rooms.Values)
            {
                room.Broadcast_Chat(packetType, message);
            }
        }
    }
}
