using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using HotFix;

namespace NetCoreServer
{
    public class ServerRoomManager
    {
        protected Dictionary<int, ServerRoom> dic_rooms;
        public int Count => dic_rooms.Count;

        public ServerRoomManager()
        {
            dic_rooms = new Dictionary<int, ServerRoom>();
        }

        // 创建房间
        public ServerRoom CreateServerRoom(ServerPlayer hostPlayer, int limit)
        {
            int roomId = GetAvailableRoomID();
            if (dic_rooms.ContainsKey(roomId))
            {
                Debug.Print("严重的错误，创建房间时，ID重复");
                return null;
            }
            if (Count >= MAX_INDEX)
            {
                Debug.Print("大厅爆满，无法创建新房间");
                return null;
            }

            ServerRoom serverRoom = new ServerRoom(hostPlayer, roomId, limit);
            dic_rooms.Add(roomId, serverRoom);
            return serverRoom;
        }
        // 关闭房间（房主解散或房间结算后执行）
        public void RemoveServerRoom(int roomId)
        {
            ServerRoom serverRoom = null;
            if (dic_rooms.TryGetValue(roomId, out serverRoom))
            {
                serverRoom.Dispose();
                dic_rooms.Remove(roomId);
            }
            else
            {
                Debug.Print("严重的错误，无法移除房间");
            }
        }
        // 关闭房间（如果该用户是房主）
        public bool RemoveIfHost(ServerPlayer p)
        {
            ServerRoom serverRoom = GetServerRoom(p.RoomId);
            if (serverRoom.hostPlayer.PeerId == p.PeerId)
            {
                RemoveServerRoom(p.RoomId);
                return true;
            }
            return false;
        }
        // 关闭房间（所有）
        public void RemoveAll()
        {
            foreach (var roomItem in dic_rooms)
            {
                roomItem.Value.Dispose();
                dic_rooms.Remove(roomItem.Key);
            }
        }
        // 查询房间
        public ServerRoom GetServerRoom(int roomId)
        {
            ServerRoom serverRoom = null;
            if (dic_rooms.TryGetValue(roomId, out serverRoom) == false)
            {
                Debug.Print("严重的错误，无法移除房间");
            }
            return serverRoom;
        }
        // 查询房间（所有，仅调试用）
        public ServerRoom[] GetAll()
        {
            ServerRoom[] DictionaryToArray = dic_rooms.Values.ToArray();
            return DictionaryToArray;
        }

        // 如果战场需要运行Update，定义专门的战场房间容器。
        // 方便主循环上容易地一次性获取，调用Room.Update()。

        // 获取空闲房间Id
        const int MIN_INDEX = 1;
        const int MAX_INDEX = 65536;
        private int GetAvailableRoomID()
        {
            int id = MIN_INDEX;

            if (dic_rooms.Count == 0)
                return id;

            for (int i = MIN_INDEX; i <= MAX_INDEX; i++)
            {
                ServerRoom serverRoom = null;
                if (dic_rooms.TryGetValue(i, out serverRoom) == false)
                {
                    id = i;
                    break;
                }
            }
            return id;
        }
    }
}