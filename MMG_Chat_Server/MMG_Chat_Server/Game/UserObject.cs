using MMG_Chat_Server.Main;
using MMG_Chat_Server.Room;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMG_Chat_Server.Game
{
    public class UserData
    {
        public int UserId;
        public int CharacterId;
        public string NickName;
    }
    public class UserObject
    {
        public ChatSession Session;
        public int? CurrentRoomId { get; set; }
        public UserData UserInfo { get; set; }
        public ChatRoom Room { get; set; }
        public void OnDisconnected()
        {

        }
        public UserObject(UserData userData, ChatRoom room, ChatSession _session)
        {
            UserInfo = userData;
            Room = room;
            Session = _session;
        }

    }
}
