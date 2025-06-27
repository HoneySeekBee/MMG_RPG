using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    // 클라이언트 연결당 1개씩, 비동기 소켓 처리 
    public abstract class Session
    {
        public int SessionId { get; set; }
        protected Socket _socket;
        private RecvBuffer _recvBuffer = new RecvBuffer(65535);
        public virtual void Start(Socket socket)
        {
            _socket = socket;
            BeginReceive();
        }

        private void BeginReceive()
        {
            _socket.BeginReceive(
                _recvBuffer.WriteSegment.Array,
                _recvBuffer.WriteSegment.Offset,
                _recvBuffer.WriteSegment.Count,
                SocketFlags.None,
                ReceiveCallback,
                null);
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int bytesRead = _socket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    _recvBuffer.OnWrite(bytesRead);

                    int processedLength = OnRecv(_recvBuffer.ReadSegment);

                    if (processedLength > 0)
                        _recvBuffer.OnRead(processedLength);
                    BeginReceive();
                }
                else
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReceiveCallback Error] {ex}");
                Close();
            }
        }
        protected virtual int OnRecv(ArraySegment<byte> buffer)
        {
            // 기본적으로 이 함수는 받은 데이터를 Packet 단위로 잘라서 처리하는 쪽으로 넘김
            return OnRecvPacket(buffer); // PacketSession에서 override하도록 설계돼 있음
        }
        public virtual void Send(byte[] sendBuffer)
        {
            _socket.BeginSend(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, SendCallback, null);
        }
        private void SendCallback(IAsyncResult ar)
        {
            int bytesSent = _socket.EndSend(ar);
            OnSend(bytesSent);
        }
        public virtual void Close()
        {
            _socket?.Shutdown(SocketShutdown.Both);
            _socket?.Close();
        }

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnDisconnected(EndPoint endPoint);
        public abstract int OnRecvPacket(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
    }
}
