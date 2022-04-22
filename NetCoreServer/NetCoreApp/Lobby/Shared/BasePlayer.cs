namespace HotFix
{
    public class BasePlayerData
    {
        public System.Guid PeerId;  //房间ID
        public string UserName;     //登录名
        public string NickName;     //昵称
        public int SeatId;          //座位号
    }
    public abstract class BasePlayer
    {
        public readonly System.Guid PeerId; //连接ID（Connect后生成，登录后和用户名绑定）
        public readonly string UserName;    //登录名/手机号/邮箱号/三方token
        public string NickName;             //昵称（查SQL获得，缓存在类中）
        public short RoomId;                //-1是在大厅
        public short SeatId;                //-1是不在房间，0是主位
        public PlayerStatus Status;

        protected BasePlayer(string userName, System.Guid peerid)
        {
            PeerId = peerid;
            UserName = userName;
            ResetToLobby(); //登录成功创建的，已经在大厅
        }
        public virtual BasePlayer SetRoomID(int roomId)
        {
            this.RoomId = (short)roomId;
            return this;
        }
        public virtual BasePlayer SetSeatID(int seatid)
        {
            this.SeatId = (short)seatid;
            return this;
        }
        public virtual BasePlayer SetStatus(PlayerStatus status)
        {
            this.Status = status;
            return this;
        }
        public virtual void ResetToLobby()
        {
            this.SetRoomID(-1)
                .SetSeatID(-1)
                .SetStatus(PlayerStatus.AtLobby);
        }
        
        public override string ToString()
        {
            string str = $"[{PeerId}]{UserName}({Status})，房间号{RoomId}，座位号{SeatId}";
            return str;
        }
    }
}