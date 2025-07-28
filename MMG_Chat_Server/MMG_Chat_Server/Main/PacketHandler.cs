using ChatPacket;
using MMG_Chat_Server.Game;
using MMG_Chat_Server.Room;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMG_Chat_Server.Main
{
    public class PacketHandler
    {
        public static async void C_EnterChatRoom(ChatSession session, EnterChatRoom packet)
        {
            UserData userData = new UserData()
            {
                UserId = packet.UserId,
                CharacterId = packet.CharacterId,
                NickName = packet.NickName,
            };

            Console.WriteLine($"[C_EnterChatRoom] : {packet.CharacterId}가 {packet.RoomId}에 입장");

            ChatRoom chatRoom = ChatRoomManager.Instance.GetOrCreateRoom(packet.RoomId);
            if(chatRoom == null)
            {
                Console.WriteLine($"[C_EnterChatRoom] : {packet.RoomId}방이 없음");
            }
            UserObject user = new UserObject(userData, chatRoom, session);

            session.MyPlayer = user;

            await chatRoom.Enter(session, user);

            session.Send(PacketType.S_EnterChatRoom, packet);
        }
        public static void C_RoomChat(ChatSession session, C_RoomChat packet)
        {
            Console.WriteLine($"[C_RoomChat] : {packet.Message} {session.Room.RoomId}");
            session.Room.Broadcast_RoomChat(session.MyPlayer.UserInfo.CharacterId, packet.Message, session.MyPlayer.UserInfo.NickName, DateTime.UtcNow);
        }
        private static void ExitGameRoom(UserData player)
        {
            // [1] 이전 방에서 퇴장 처리

            //if (player.CurrentRoomId.HasValue)
            //{
            //    int prevRoomId = player.CurrentRoomId.Value;
            //    ChatRoom prevRoom = GameRoomManager.Instance.GetRoom(prevRoomId);

            //    // 그 방이 아직 존재하고, 해당 플레이어가 그 방에 실제로 속해있을 때만 퇴장 처리
            //    if (prevRoom != null && prevRoom.HasPlayer(player))
            //    {
            //        prevRoom.Push(() => prevRoom.Leave(player));
            //    }
            //    else
            //    {
            //        Console.WriteLine($"[경고] player.CurrentRoomId={prevRoomId}인데 실제 방에 없음 → 무시");
            //    }
            //}
        }
    }
}
