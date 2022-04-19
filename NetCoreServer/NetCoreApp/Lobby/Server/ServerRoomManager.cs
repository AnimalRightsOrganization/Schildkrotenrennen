using System.Collections.Generic;
using HotFix;

namespace NetCoreServer
{
    public class ServerRoomManager
    {
        public Dictionary<int, ServerRoom> dic_rooms;
        public int Count => dic_rooms.Count;

        public ServerRoomManager()
        {
            dic_rooms = new Dictionary<int, ServerRoom>();
        }
    }
}