using System;

namespace NetCoreServer
{
    public class BaseRoom : IDisposable
    {
        public BaseRoom(int _roomid, BasePlayer host)
        {
            RoomID = _roomid;
        }

        public const int MIN_PLAYERS = 2; //最少人数
        public const int MAX_PLAYERS = 5; //最多人数
        public readonly int RoomID; // 当前房间ID（1~65535）
        public int Seed;            // 随机种子

        // 一个房间必须满足有2个人(掉线?)
        public virtual BasePlayer[] m_PlayerList { get; protected set; }
        public virtual BasePlayer hostPlayer => m_PlayerList[0];
        public virtual void Dispose() { }

        public override string ToString()
        {
            string str = $"房间#{RoomID}，";
            return str;
        }
    }
}