namespace HotFix
{
    public class BasePlayerData : System.Object
    {
        // 初值
        public BasePlayerData()
        {
            IsBot = false;
            PeerId = System.Guid.Empty;
            UserName = string.Empty;
            NickName = string.Empty;
            RoomId = -1;
            SeatId = -1;
            Status = PlayerStatus.LOBBY;
        }
        public bool IsBot;          //机器人标识
        public System.Guid PeerId;  //房间ID
        public string UserName;     //登录名
        public string NickName;     //昵称
        public int RoomId;          //房间号
        public int SeatId;          //座位号
        public PlayerStatus Status; 

        public override string ToString()
        {
            string dataStr = $"[{PeerId}]{UserName}({Status})，房间#{RoomId}，座位#{SeatId}";
            return dataStr;
        }
    }
    public abstract class BasePlayer
    {
        protected BasePlayerData m_Data;

        public bool IsBot => m_Data.IsBot;
        public System.Guid PeerId => m_Data.PeerId;
        public string UserName => m_Data.UserName;
        public string NickName => m_Data.NickName;
        public int RoomId => m_Data.RoomId;
        public int SeatId => m_Data.SeatId;
        public PlayerStatus Status => m_Data.Status;

        protected BasePlayer(BasePlayerData data)
        {
            m_Data = data;
        }
        public virtual BasePlayer SetRoomID(int roomId)
        {
            this.m_Data.RoomId = (short)roomId;
            return this;
        }
        public virtual BasePlayer SetSeatID(int seatid)
        {
            this.m_Data.SeatId = (short)seatid;
            return this;
        }
        public virtual BasePlayer SetStatus(PlayerStatus status)
        {
            this.m_Data.Status = status;
            return this;
        }
        public virtual void ResetToLobby()
        {
            this.SetRoomID(-1)
                .SetSeatID(-1)
                .SetStatus(PlayerStatus.LOBBY);
        }
        
        public override string ToString()
        {
            return m_Data.ToString();
        }
    }
}