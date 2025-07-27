using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMG_Chat_Server.Main
{
    public class SessionManager
    {
        private static Dictionary<int, ChatSession> _sessions = new();
        private static Dictionary<int, ChatSession> _userSessions = new();
        private static object _lock = new();

        public static void Add(int sessionId, ChatSession session)
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
                        _userSessions.Remove(session.MyPlayer.UserInfo.CharacterId); // 중복 로그인 방지
                    }

                    _sessions.Remove(sessionId);
                }
            }
        }
        public static List<ChatSession> GetAll()
        {
            lock (_lock)
            {
                return _sessions.Values.ToList();
            }
        }
        public static ChatSession Get(int id)
        {
            lock (_lock)
            {
                return _sessions.TryGetValue(id, out var session) ? session : null;
            }
        }
        public static bool IsLoggedIn(int userId)
        {
            return _userSessions.ContainsKey(userId);
        }

        public static void SetUserLoggedIn(int userId, ChatSession session)
        {
            _userSessions[userId] = session;
        }

    }
}
