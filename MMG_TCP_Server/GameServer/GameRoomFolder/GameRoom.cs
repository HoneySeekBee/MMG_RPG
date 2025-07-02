using GameServer.Core;
using GameServer.Domain;
using Google.Protobuf;
using Packet;
using ServerCore;
using System.Numerics;
using static GameServer.GameRoomFolder.SpawnZoneLoader;

namespace GameServer.GameRoomFolder
{
    public class GameRoom : JobQueue
    {
        public int RoomId { get; private set; }

        private List<Player> _players = new List<Player>();
        public GameRoom(int roomId)
        {
            RoomId = roomId;
        }
        private PlayerInfo MakePlayerInfo(Player p)
        {
            return new PlayerInfo
            {
                Name = p.CharacterInfo.CharacterName,
                PlayerId = p.UserId,
            };
        }
        public void Enter(ServerSession session)
        {
            Player player = session.MyPlayer;

            if (_players.Contains(player))
            {
                Console.WriteLine($"[GameRoom:{RoomId}] 이미 입장된 유저: {player.CharacterInfo.CharacterName}");
                return;
            }

            Vector3 spawnVec = SpawnZoneLoader.GetRandomSpawnPos(RoomId, 0);
            player.PosX = spawnVec.X;
            player.PosY = spawnVec.Y;
            player.PosZ = spawnVec.Z;
            player.DirY = 0;
            _players.Add(player);

            // 해당 유저에게는 응답을 보내주고 나머지 방에 사람들에게는 입장 알려줌

            session.MyPlayer.CurrentRoomId = RoomId;

            var response = new S_EnterGameResponse();
            response.MapId = RoomId;


            foreach (var p in _players)
            {
                response.CharacterList.Add(CreateCharacterList(p, p == player));
            }


            session.Send(PacketType.S_EnterGameResponse, response);

            Console.WriteLine($"[GameRoom:{RoomId}] {player.CharacterInfo.CharacterName} 입장  -----> 같은 게임방의 유저들에게 입장 알려줘야함.");

            var broadcastEnter = new S_BroadcastEnter
            {
                EnterCharacter = CreateCharacterList(player, isLocal: false)
            };
            BroadcastEnter(broadcastEnter, player);
        }
        private CharacterList CreateCharacterList(Player player, bool isLocal)
        {
            return new CharacterList
            {
                IsLocal = isLocal,
                CharacterInfo = player.CharacterInfo,
                PosX = player.PosX,
                PosY = player.PosY,
                PosZ = player.PosZ,
                DirY = player.DirY
            };
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
        public void BroadcastMove(S_BroadcastMove message, Player exclude = null)
        {
            if (exclude != null)
                exclude.UpdateMove(message.PosX, message.PosY, message.PosZ, message.DirY);

            Broadcast(PacketType.S_BroadcastMove, message, exclude);
        }

        public void BroadcastEnter(S_BroadcastEnter message, Player exclude = null)
        {
            Console.WriteLine($"캐릭터[{message.EnterCharacter.CharacterInfo.CharacterName}] 입장 - ");
            Broadcast(PacketType.S_BroadcastEnter, message, exclude);
        }
    }
}
