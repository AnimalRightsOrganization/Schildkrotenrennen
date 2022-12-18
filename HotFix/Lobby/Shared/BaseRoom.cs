using System.Collections.Generic;

namespace HotFix
{
    public class BaseRoomData
    {
        public BaseRoomData()
        {
            RoomID = -1;
            RoomName = string.Empty;
            RoomPwd = string.Empty;
            RoomLimit = 2;
            Players = new List<BasePlayerData>();
        }
        public int RoomID;                          //房间ID
        public string RoomName;                     //房间名
        public string RoomPwd;                      //密码（仅用于创建房间）
        public int RoomLimit;                       //限定人数
        public List<BasePlayerData> Players;        //成员
        public override string ToString()
        {
            string playerStr = string.Empty;
            for (int i = 0; i < Players.Count; i++)
            {
                var player = Players[i];
                playerStr += $"\n[{i}]---{player.ToString()}";
            }
            string dataStr = $"RoomID={RoomID}, RoomName={RoomName}, RoomPwd={RoomPwd}, RoomLimit={RoomLimit}, Players={playerStr}";
            return dataStr;
        }
    }
    public abstract class BaseRoom
    {
        // 房主离开房间解散（简单做法）
        public BaseRoom(BaseRoomData roomData)
        {
            m_Data = roomData;
        }
        public BaseRoomData m_Data;
        public int RoomID => m_Data.RoomID;         //房间ID
        public string RoomName => m_Data.RoomName;  //房间名
        public string RoomPwd => m_Data.RoomPwd;    //密码
        public int RoomLimit => m_Data.RoomLimit;   //限定人数
        public List<BasePlayerData> Players => m_Data.Players; //成员

        public virtual Dictionary<int, BasePlayer> m_PlayerDic { get; set; } //int是座位号
        public virtual BasePlayer hostPlayer => m_PlayerDic[0];

        public override string ToString()
        {
            return m_Data.ToString();
        }
    }
}