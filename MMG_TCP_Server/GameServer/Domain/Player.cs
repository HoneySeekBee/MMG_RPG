using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GameServer.Core;

namespace GameServer.Domain
{
    public class Player
    {
        public int PlayerId { get; set; }
        public string Name { get; set; }
        public ServerSession Session { get; set; }

        // 위치 정보
        public float PosX { get; set; } = 0f;
        public float PosY { get; set; } = 0f;

        // 추후 확장을 고려해 Vector2로도 표현 가능
        public Vector2 Position => new Vector2(PosX, PosY);

        public long LastMoveTimestamp { get; set; } = 0;
        public void OnDisconnected()
        {
            Console.WriteLine($"[Player] {Name} disconnected.");

            // GameRoom에서 제거하거나 기타 정리 작업을 이곳에 작성
            GameRoom.Instance.Leave(this);
        }
    }
}
