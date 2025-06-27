using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Core
{
    public class SessionManager
    {
        // 클라이언트 접속 시 유저 관리 
        private static Dictionary<int, ServerSession> _sessions = new();
        private static Dictionary<string, ServerSession> _userSessions = new();
        private static object _lock = new();
        public static void Add(int sessionId, ServerSession session)
        {
            lock (_lock)
            {
                Console.WriteLine($"Add [ id :{sessionId} ] Session");
                _sessions[sessionId] = session;
            }
        }

        public static void Remove(int sessionId)
        {
            lock (_lock)
            {
                if (_sessions.TryGetValue(sessionId, out var session))
                {
                    if (session.MyPlayer != null)
                    {
                        _userSessions.Remove(session.MyPlayer.Name); // 중복 로그인 방지
                    }

                    _sessions.Remove(sessionId);
                }
            }
        }

        public static List<ServerSession> GetAll()
        {
            lock (_lock)
            {
                return _sessions.Values.ToList();
            }
        }

        public static ServerSession Get(int id)
        {
            lock (_lock)
            {
                return _sessions.TryGetValue(id, out var session) ? session : null;
            }
        }

        public static bool IsLoggedIn(string userId)
        {
            return _userSessions.ContainsKey(userId);
        }

        public static void SetUserLoggedIn(string userId, ServerSession session)
        {
            _userSessions[userId] = session;
        }
    }
}
