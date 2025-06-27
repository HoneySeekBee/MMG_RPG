using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Core
{
    // TCP 연결 수락용
    public class Listener
    {
        private Socket _listenSocket;
        private Func<ServerSession> _sessionFactory;

        private int _sessionIdCounter = 0;

        public void Init(IPEndPoint endPoint, Func<ServerSession> sessionFactory)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory = sessionFactory;

            _listenSocket.Bind(endPoint);
            _listenSocket.Listen(100);

            Console.WriteLine("Listening on " + endPoint.ToString());
            RegisterAccept();
        }
        private void RegisterAccept()
        {
            _listenSocket.BeginAccept(AcceptCallback, null);
        }
        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = _listenSocket.EndAccept(ar);

                // 고유한 세션 ID 생성
                int sessionId = Interlocked.Increment(ref _sessionIdCounter);

                ServerSession session = _sessionFactory.Invoke() as ServerSession;
                session.SessionId = sessionId; // 여기서 고유 ID 부여

                SessionManager.Add(sessionId, session);
                session.Start(clientSocket);

                RegisterAccept(); // 다음 Accept
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AcceptCallback Error: {ex}");
            }
        }
    }
}
