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
        public static GameRoom Instance { get; } = new GameRoom();

        private List<Player> _players = new List<Player>();

        public void TestAction(ServerSession session)
        {
            Console.WriteLine($"[GameRoom] 세션 ID : {session.SessionId}");
        }
        private PlayerInfo MakePlayerInfo(Player _player)
        {
            PlayerInfo _playerInfo = new PlayerInfo();
            _playerInfo.Name = _player.Name;
            _playerInfo.PlayerId = _player.PlayerId;
            _playerInfo.PosX = _player.PosX;
            _playerInfo.PosY = _player.PosY;
            return _playerInfo;
        }
        public void Enter(ServerSession session, C_EnterGame packet)
        {
            Player player = session.MyPlayer;

            if (_players.Contains(player))
            {
                Console.WriteLine($"[GameRoom] 이미 입장된 유저입니다: {player.Name}");
                return;
            }
            _players.Add(player);
            Console.WriteLine($"[GameRoom] id {player.PlayerId} / Name : {player.Name} 입장 완료");

            List<PlayerInfo> playerList = new();

            foreach (Player _player in _players)
            {
                PlayerInfo _playerInfo = MakePlayerInfo(_player);
                playerList.Add(_playerInfo);
            }
            // 응답 패킷 전송
            S_EnterGame response = new S_EnterGame()
            {
                Success = true,
                WelcomeMessage = $"환영합니다, {player.Name}님!",
                Name = player.Name,
                PlayerId = player.PlayerId,
            };
            response.Players.AddRange(playerList);
            session.Send(PacketType.S_EnterGame, response);

            PlayerInfo myPlayerInfo = MakePlayerInfo(player);
            // 다른 유저에게는 입장 알림

            S_PlayerEntered broadcast = new S_PlayerEntered
            {
                Name = player.Name,
                PlayerInfo = myPlayerInfo
            };
            Broadcast(PacketType.S_PlayerEntered, broadcast, exclude: player);
        }
        public void BroadcastMove(S_Move message, Player exclude = null)
        {
            Broadcast(PacketType.S_Move, message, exclude);
        }
        public void Update()
        {
            Flush(); // JobQueue에 쌓인 작업 처리
        }
        public void Leave(Player player)
        {
            if (_players.Remove(player))
                Console.WriteLine($"[GameRoom] {player.Name} 퇴장 완료");
        }
        private void Broadcast(PacketType packetType, IMessage message, Player exclude = null)
        {
            foreach (var p in _players)
            {
                if (p == exclude) continue;
                p.Session.Send(packetType, message);
            }
        }
    }
}
