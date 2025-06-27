using Packet;
using ServerCore;
using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Core
{
    public class ServerPacketUtils
    {
        public static void SendError(ServerSession session, ErrorCode code, string message = "")
        {
            S_Error errorPacket = new S_Error
            {
                ErrorCode = (int)code,
                ErrorMessage = message
            };

            session.Send(PacketType.S_Error, errorPacket);
        }
    }
}
