using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Room.Map
{
    public class BlockArea
    {
        public int Id;
        public string Description;
        public Vector3 Min;
        public Vector3 Max;

        public bool Contains(Vector3 position)
        {
            return position.X >= Min.X && position.X <= Max.X &&
                   position.Y >= Min.Y && position.Y <= Max.Y &&
                   position.Z >= Min.Z && position.Z <= Max.Z;
        }
    }
}
