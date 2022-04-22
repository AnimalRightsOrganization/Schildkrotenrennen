using System.Collections.Generic;

namespace HotFix
{
    /* 本地房间 */
    public class ClientRoom : BaseRoom
    {
        #region 房间数据
        public ClientRoom(ClientPlayer host, BaseRoomData data) : base(host, data)
        {
            // 被override的才需要在这里赋值
            //RoomID = roomId;
            //RoomLimit = limit;
            m_PlayerList = new Dictionary<int, BasePlayer>();
            m_PlayerList.Add(0, host); //房主座位号0
            host.SetRoomID(data.RoomID)
                .SetSeatID(0)
                .SetStatus(PlayerStatus.AtRoomWait);
        }

        public override Dictionary<int, BasePlayer> m_PlayerList { get; protected set; }
        #endregion
    }
}