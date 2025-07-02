using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GameServer.Core;
using Packet;

namespace GameServer.Domain
{
    public class Player
    {
        public ServerSession Session { get; set; }
        public int? CurrentRoomId { get; set; }
        public int UserId { get; set; } // 이게 UserId
        public CharacterInfo CharacterInfo { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float DirY { get; set; }
        public long LastMoveTimestamp { get; set; } = 0;
        public void OnDisconnected()
        {
            //Console.WriteLine($"[Player] {Character.CharacterName} disconnected.");

            // GameRoom에서 제거하거나 기타 정리 작업을 이곳에 작성

        }
    }
}
