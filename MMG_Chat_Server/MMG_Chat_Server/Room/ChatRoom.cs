using ChatPacket;
using Google.Protobuf;
using MMG_Chat_Server.Game;
using MMG_Chat_Server.Main;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMG_Chat_Server.Room
{
    public class ChatRoom
    {
        public int RoomId { get; private set; }

        public Dictionary<int, UserObject> _players = new();
        

        public ChatRoom(int roomId)
        {
            Console.WriteLine("ChatRoom 초기화 ");
            RoomId = roomId;
        }
        public async Task Enter(ChatSession session, UserObject _userInfo)
        {
            int charId = _userInfo.UserInfo.CharacterId;
            if (_players.ContainsKey(charId))
            {
                Console.WriteLine($"[GameRoom:{RoomId}] 이미 입장된 유저: ");
                return;
            }
            session.Room = this;
            _players.Add(charId, _userInfo);
            Console.WriteLine($"[Enter] {_userInfo.UserInfo.CharacterId}");

        }
        public void Leave(UserObject player)
        {
            if (_players.Remove(player.UserInfo.UserId))
                Console.WriteLine($"[GameRoom:{RoomId}] {player.UserInfo.CharacterId} 퇴장");
        }
        public UserObject FindPlayerById(int characterId) => _players[characterId];
        private void Broadcast(PacketType packetType, IMessage message, UserObject exclude = null)
        {
            foreach (var player in _players)
            {
                if (exclude != null && player.Key == exclude.UserInfo.UserId) continue;

                player.Value.Session.Send(packetType, message);
            }
        }
        public void Broadcast_RoomChat(int _charId, string _message, string _nickName, DateTime dateTime)
        {
            long unixTimeSeconds = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
            Program._Redis.Save_RoomChat(_charId, _message, _nickName, dateTime, RoomId); // 여기서 채팅 기록 저장
            Console.WriteLine($"[ChatRoom] Broadcast RoomChat {_nickName} : {_message} || Room Player 수 {_players.Count} ");
            S_BroadcastRoomChat roomchat = new S_BroadcastRoomChat()
            {
                CharacterId = _charId,
                Message = _message,
                NickName = _nickName,
                TimeStamp = unixTimeSeconds,
            };
            Broadcast(PacketType.S_BroadcastRoomChat, roomchat);
        }
    }
}
