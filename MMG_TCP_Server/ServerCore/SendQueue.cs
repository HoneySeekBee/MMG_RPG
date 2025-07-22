using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class SendQueue
    {
        Queue<ArraySegment<byte>> _queue = new Queue<ArraySegment<byte>>();
        bool _isSending = false;
        object _lock = new object();

        public void Send(Socket socket, ArraySegment<byte> buffer)
        {
            bool sendImmediately = false;
            lock (_lock)
            {
                _queue.Enqueue(buffer);
                if (!_isSending)
                {
                    _isSending = true;
                    sendImmediately = true;
                }
            }
            if (sendImmediately)
                RegisterSend(socket);
        }
        private void RegisterSend(Socket socket)
        {
            ArraySegment<byte> buffer;
            lock (_lock)
            {
                buffer = _queue.Peek();
            }
            try
            {
                socket.BeginSend(buffer.Array, buffer.Offset, buffer.Count, SocketFlags.None, SendCallback, socket);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SendQueue] 소켓이 이미 닫힘: {ex.Message}");
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            try
            {
                int sent = socket.EndSend(ar);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SendQueue] SendCallback에서 소켓이 이미 닫힘: {ex.Message}");
                return;
            }
            lock (_lock)
            {
                _queue.Dequeue();
                if (_queue.Count == 0)
                {
                    _isSending = false;
                    return;
                }
            }
            RegisterSend(socket);
        }
    }
}
