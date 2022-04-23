using System;
using System.Collections.Generic;

namespace HotFix
{
    public class BaseRoomData : System.Object
    {
        // 初值
        public BaseRoomData()
        {
            RoomID = -1;
            RoomName = string.Empty;
            RoomPwd = string.Empty;
            RoomLimit = MIN_PLAYERS;
            Players = new List<BasePlayerData>();
        }
        public int RoomID;                  //房间ID
        public string RoomName;             //房间名
        public string RoomPwd;              //密码
        public int RoomLimit;               //限定人数
        public List<BasePlayerData> Players;//成员
        public const int MIN_PLAYERS = 2;   //最少人数
        public const int MAX_PLAYERS = 5;   //最多人数

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
        public BaseRoomData m_RoomData;

        // 房主离开房间解散（简单做法）
        public BaseRoom(BasePlayer host, BaseRoomData roomData)
        {
            m_RoomData = roomData;
        }

        public virtual Dictionary<int, BasePlayer> m_PlayerList { get; protected set; } //int是座位号
        public virtual BasePlayer hostPlayer => m_PlayerList[0];

        public override string ToString()
        {
            string str = $"房间#{m_RoomData.RoomID}，";
            return str;
        }
    }
}