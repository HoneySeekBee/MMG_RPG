using GameServer.Core;
using Google.Protobuf;
using Packet;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Domain
{
    public class GameRoom : JobQueue
    {
        public int RoomId { get; private set; }

        private List<Player> _players = new List<Player>();
        public GameRoom(int roomId)
        {
            this.RoomId = roomId;
        }
        private PlayerInfo MakePlayerInfo(Player p)
        {
            return new PlayerInfo
            {
                Name = p.CharacterInfo.CharacterName,
                PlayerId = p.UserId,
            };
        }
        public void Enter(ServerSession session, C_EnterGameRoom packet)
        {
            Player player = session.MyPlayer;

            if (_players.Contains(player))
            {
                Console.WriteLine($"[GameRoom:{RoomId}] 이미 입장된 유저: {player.CharacterInfo.CharacterName}");
                return;
            }

            _players.Add(player);
            Console.WriteLine($"[GameRoom:{RoomId}] {player.CharacterInfo.CharacterName} 입장  -----> 같은 게임방의 유저들에게 입장 알려줘야함.");

        }
        public bool HasPlayer(Player player)
        {
            return _players.Contains(player);
        }
        public void Update()
        {
            Flush(); // JobQueue에 쌓인 작업 처리
        }

        public void Leave(Player player)
        {
            if (_players.Remove(player))
                Console.WriteLine($"[GameRoom:{RoomId}] {player.CharacterInfo.CharacterName} 퇴장");
        }
        private void Broadcast(PacketType packetType, IMessage message, Player exclude = null)
        {
            foreach (var player in _players)
            {
                if (player == exclude) continue;
                player.Session.Send(packetType, message);
            }
        }
        public void BroadcastMove(S_Move message, Player exclude = null)
        {
            Broadcast(PacketType.S_Move, message, exclude);
        }

    }
}
