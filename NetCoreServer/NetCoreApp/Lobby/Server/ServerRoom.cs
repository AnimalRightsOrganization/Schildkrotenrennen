using HotFix;

namespace NetCoreServer
{
    /* 远程房间 */
    public class ServerRoom : BaseRoom
    {
        #region 房间数据

        public ServerRoom(int roomId, ServerPlayer host) : base(roomId, host)
        {
            m_PlayerList = new ServerPlayer[] { host };
        }

        public override BasePlayer[] m_PlayerList { get; protected set; }
        public override void Dispose() { }

        #endregion
    }
}