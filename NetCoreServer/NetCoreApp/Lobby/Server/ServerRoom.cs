using HotFix;
using System.Collections.Generic;

namespace NetCoreServer
{
    /* 远程房间 */
    public class ServerRoom : BaseRoom
    {
        #region 房间数据

        public ServerRoom(ServerPlayer host, int roomId, int limit) : base(host, roomId, limit)
        {
            // 被override的才需要在这里赋值
            //RoomID = roomId;
            //RoomLimit = limit;
            m_PlayerList = new Dictionary<int, BasePlayer>();
            m_PlayerList.Add(0, host); //房主座位号0
        }

        public void JoinRoom()
        {
            
        }

        public override Dictionary<int, BasePlayer> m_PlayerList { get; protected set; }
        public override void Dispose() { }

        #endregion
    }
}