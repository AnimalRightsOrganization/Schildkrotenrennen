using System.Collections.Generic;
using Debug = System.Diagnostics.Debug;
using HotFix;

namespace NetCoreServer
{
    /* 远程房间 */
    public class ServerRoom : BaseRoom
    {
        public ServerRoom(ServerPlayer host, BaseRoomData data) : base(host, data)
        {
            // 被override的才需要在这里赋值
            m_PlayerList = new Dictionary<int, BasePlayer>();
            m_PlayerList.Add(0, host); //房主座位号0
            host.SetRoomID(data.RoomID)
                .SetSeatID(0)
                .SetStatus(PlayerStatus.AtRoomWait);
        }

        public override Dictionary<int, BasePlayer> m_PlayerList { get; protected set; }

        public bool AddPlayer(BasePlayer p)
        {
            if (ContainsPlayer(p))
            {
                Debug.Print("严重的错误，重复加入");
                return false;
            }

            int SeatId = GetAvailableRoomID();
            m_PlayerList.Add(SeatId, p);
            p.SetRoomID(RoomID)
                .SetSeatID(SeatId)
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
        public bool RemoveAll()
        {
            foreach (var p in m_PlayerList)
            {
                //ServerPlayer serverPlayer = p.Value as ServerPlayer;
                RemovePlayer(p.Value);
            }
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
        public int CurCount => m_PlayerList.Count;

        public ServerPlayer GetPlayer(int seatId)
        {
            BasePlayer basePlayer = null;
            if (m_PlayerList.TryGetValue(seatId, out basePlayer))
            {
                return (ServerPlayer)basePlayer;
            }
            return null;
        }

        // 检查空座位
        public bool IsAvailableSeat(int seatId)
        {
            foreach (var p in m_PlayerList)
            {
                if (p.Key == seatId)
                    return false;
            }
            return true;
        }

        const int MIN_INDEX = 0;
        const int MAX_INDEX = 4;
        int GetAvailableRoomID()
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

        public void SendAsync(PacketType msgId, object cmd)
        {
            foreach(var p in m_PlayerList)
            {
                ServerPlayer serverPlayer = p.Value as ServerPlayer;
                serverPlayer.SendAsync(msgId, cmd);
            }
        }
    }
}