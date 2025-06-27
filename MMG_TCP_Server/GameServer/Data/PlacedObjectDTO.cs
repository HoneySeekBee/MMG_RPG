using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data
{
    public class PlacedObjectDTO
    {
        public int Id { get; set; }             // PK
        public int UserId { get; set; }         // 유저 ID
        public int Version { get; set; }        // 0: 백업, 1: 최신
        public int ObjectId { get; set; }
        public string ObjectType { get; set; }  // 오브젝트 종류 (ex. House, Fence, Tree 등)

        public int PosX { get; set; }
        public int PosY { get; set; }

        public int DirY { get; set; }           // 0, 90, 180, 270 같은 회전값
        public int ObjLevel { get; set; }       // 건물 레벨

        public DateTime UpdatedAt { get; set; } // 마지막 수정 시간
    }
}
