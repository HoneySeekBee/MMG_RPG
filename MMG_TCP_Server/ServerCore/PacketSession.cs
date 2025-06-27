using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        private const int HeaderSize = 2;
        private byte[] _buffer = new byte[4096];
        private int _recvPos = 0;
        public override int OnRecvPacket(ArraySegment<byte> buffer)
        {
            int readPos = buffer.Offset;
            int endPos = buffer.Offset + buffer.Count;

            while (true)
            {
                if (endPos - readPos < 2)
                    break;

                // Size는 'ID + Body' 길이임 → 전체 패킷은 2 (size field) + size
                ushort size = BitConverter.ToUInt16(buffer.Array, readPos);
                int totalSize = 2 + size;

                if (endPos - readPos < totalSize)
                    break;

                var packetBuffer = new ArraySegment<byte>(buffer.Array, readPos, totalSize);
                OnRecvPacketInternal(packetBuffer);

                readPos += totalSize;
            }

            return readPos - buffer.Offset;
        }

        public void Send(PacketType packetType, IMessage message)
        {
            ushort packetId = (ushort)packetType;

            // 직렬화
            byte[] body;
            using (var ms = new MemoryStream())
            {
                message.WriteTo(ms);
                body = ms.ToArray();
            }

            ushort size = (ushort)(2 + body.Length); // ID(2byte) + Body
            byte[] sendBuffer = new byte[2 + size];  // Size(2byte) + 전체

            // Size (2바이트)
            Array.Copy(BitConverter.GetBytes(size), 0, sendBuffer, 0, 2);

            // Packet ID (2바이트)
            Array.Copy(BitConverter.GetBytes(packetId), 0, sendBuffer, 2, 2);

            // Body
            Array.Copy(body, 0, sendBuffer, 4, body.Length);


            Send(sendBuffer);
        }
        protected abstract void OnRecvPacketInternal(ArraySegment<byte> buffer);

        public override void OnConnected(EndPoint endPoint) { }

        public override void OnDisconnected(EndPoint endPoint) { }

        public override void OnSend(int numOfBytes) { }
    }
}
