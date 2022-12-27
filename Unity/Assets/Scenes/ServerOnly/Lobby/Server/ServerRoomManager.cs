using System.Linq;
using System.Collections.Generic;
using HotFix;
using Debug = UnityEngine.Debug;

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
        public ServerRoom CreateServerRoom(ServerPlayer hostPlayer, BaseRoomData roomData)
        {
            int roomId = GetAvailableRoomID();
            roomData.RoomID = roomId;
            if (dic_rooms.ContainsKey(roomId))
            {
                Debug.Log("严重的错误，创建房间时，ID重复");
                return null;
            }
            if (Count >= MAX_INDEX)
            {
                Debug.Log("大厅爆满，无法创建新房间");
                return null;
            }
            var serverRoom = new ServerRoom(hostPlayer, roomData);
            dic_rooms.Add(roomId, serverRoom);
            return serverRoom;
        }
        // 关闭房间（群发离开消息，重置成员状态）
        public void RemoveServerRoom(int roomId)
        {
            ServerRoom serverRoom = null;
            if (dic_rooms.TryGetValue(roomId, out serverRoom))
            {
                var packet = new S2C_LeaveRoomPacket { RoomID = roomId, RoomName = serverRoom.m_Data.RoomName, LeaveBy = (int)LeaveRoomType.DISSOLVE };
                serverRoom.SendAsync(PacketType.S2C_LeaveRoom, packet); //房间解散，群发离开
                Debug.Log("玩家离开，移除玩家");
                serverRoom.RemoveAll();
                dic_rooms.Remove(roomId);
            }
            else
            {
                Debug.Log("不存在的房间");
            }
        }
        // 移除用户（如果移除的是房主，则关闭房间）
        public bool RemovePlayer(ServerPlayer p)
        {
            ServerRoom serverRoom = GetServerRoom(p.RoomId);
            if (serverRoom == null)
            {
                Debug.Log($"找不到房间#{p.RoomId}");
                return false;
            }
            if (serverRoom.hostPlayer.PeerId == p.PeerId)
            {
                RemoveServerRoom(p.RoomId);
                return true;
            }
            else
            {
                bool result = serverRoom.RemovePlayer(p);
                var roomInfo = serverRoom.GetRoomInfo(); //客位关闭游戏，移除用户
                S2C_RoomInfo packet = new S2C_RoomInfo { Room = roomInfo };
                serverRoom.SendAsync(PacketType.S2C_RoomInfo, packet);
                return result;
            }
        }
        // 关闭房间（所有）
        public void RemoveAll()
        {
            foreach (var roomItem in dic_rooms)
            {
                roomItem.Value.RemoveAll();
                dic_rooms.Remove(roomItem.Key);
            }
        }
        // 查询房间
        public ServerRoom GetServerRoom(int roomId)
        {
            ServerRoom serverRoom = null;
            if (dic_rooms.TryGetValue(roomId, out serverRoom) == false)
            {
                Debug.Log("严重的错误，无法移除房间");
            }
            return serverRoom;
        }
        // 查询房间（所有，仅调试用）
        public ServerRoom[] GetAll()
        {
            ServerRoom[] DictionaryToArray = dic_rooms.Values.ToArray();
            return DictionaryToArray;
        }
        public List<ServerRoom> Sort()
        {
            var list = dic_rooms.Values.ToList();
            list.Sort((x, y) => x.createTime.CompareTo(y.createTime));
            return list;
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