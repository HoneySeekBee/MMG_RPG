using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GameServer.ChatServer
{
    public static class ChatServerManager
    {
        public static ChatServerClient ChatClient { get; private set; }

        public static bool EnterConnected()
        {
            if (ChatClient != null)
                return true;

            try
            {
                ChatClient = new ChatServerClient();
                Console.WriteLine("ChatServer 연결 성공");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[경고] ChatServer 연결 실패: {ex.Message}");
                ChatClient = null;
                return false;
            }
        }
        public static async void TestHello(string message)
        {
            if (!EnterConnected())
                return;

            try
            {
                await ChatClient.TestHelloAsync(message);
                return;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[오류] ChatServer 호출 실패: {ex.Message}");
                ChatClient = null; // 다음 요청 때 재연결 시도
                return;
            }
        }
        public static async void Call_ChatServer(Func<Task> action)
        {
            if (!EnterConnected())
                return;
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[오류] ChatServer 호출 실패: {ex.Message}");
                ChatClient = null; // 다음 요청 때 재연결 시도
            }
        }
        public static async void CreateRoom(int roomId)
        {
            Call_ChatServer(() => ChatClient.CreateRoomAsync(roomId));
        }
    }
}
