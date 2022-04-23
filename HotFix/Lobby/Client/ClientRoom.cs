using System.Collections.Generic;

namespace HotFix
{
    /* 本地房间 */
    public class ClientRoom : BaseRoom
    {
        #region 房间数据
        public ClientRoom(BaseRoomData data) : base(data)
        {
            m_PlayerList = new Dictionary<int, BasePlayer>();
        }

        public override Dictionary<int, BasePlayer> m_PlayerList { get; protected set; }
        public void Join(ClientPlayer player, int seatId)
        {
            m_PlayerList.Add(seatId, player); //房主座位号0
            player.SetRoomID(RoomID)
                .SetSeatID(seatId)
                .SetStatus(PlayerStatus.ROOM);
        }
        public void Leave(int seatId)
        {
        
        }
        #endregion
    }
}