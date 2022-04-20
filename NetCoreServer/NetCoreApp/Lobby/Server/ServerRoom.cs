using HotFix;
using System.Collections.Generic;
using Debug = System.Diagnostics.Debug;

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

        public override Dictionary<int, BasePlayer> m_PlayerList { get; protected set; }
        public override void Dispose() { }

        public bool AddPlayer(BasePlayer p)
        {
            if (ContainsPlayer(p))
            {
                Debug.Print("严重的错误，重复加入");
                return false;
            }

            int SeatId = GetAvailableRoomID();
            m_PlayerList.Add(SeatId, p);
            p.SetRoomID((short)RoomID)
                .SetSeatID((short)SeatId)
                .SetStatus(PlayerStatus.AtRoomWait);

            return true;
        }
        public bool RemovePlayer(BasePlayer p)
        {
            if (ContainsPlayer(p))
            {
                m_PlayerList.Remove(p.SeatId);
                p.ResetToLobby();
                return true;
            }
            Debug.Print("严重的错误，无法移除房间");
            return false;
        }
        public bool ContainsPlayer(BasePlayer p)
        {
            BasePlayer basePlayer = null;
            if (m_PlayerList.TryGetValue(p.SeatId, out basePlayer))
            {
                if (p.PeerId != basePlayer.PeerId)
                {
                    Debug.Print("严重的错误，找错人了");
                    return false;
                }
                return true;
            }
            return false;
        }

        const int MIN_INDEX = 0;
        const int MAX_INDEX = 4;
        private int GetAvailableRoomID()
        {
            int id = MIN_INDEX;

            if (m_PlayerList.Count == 0)
                return id;

            for (int i = MIN_INDEX; i <= MAX_INDEX; i++)
            {
                BasePlayer player = null;
                if (m_PlayerList.TryGetValue(i, out player) == false)
                {
                    id = i;
                    break;
                }
            }
            return id;
        }
        #endregion
    }
}