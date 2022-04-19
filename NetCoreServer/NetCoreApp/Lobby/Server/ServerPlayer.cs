using TcpChatServer;

namespace NetCoreServer
{
    /* 远程用户 */
    public class ServerPlayer : BasePlayer
    {
        public readonly TcpSession Session; //用来直接Send消息

        public ServerPlayer(string userName, System.Guid peerid) : base(userName, peerid)
        {
            Session = TCPChatServer.server.FindSession(PeerId);
        }

        public void Send(string message)
        {
            Session.Send(message);
        }
        public void Send(byte[] buffer)
        {
            Session.Send(buffer);
        }
        public void SendAsync(string message)
        {
            Session.SendAsync(message);
        }
        public void SendAsync(byte[] buffer)
        {
            Session.SendAsync(buffer);
        }
    }
}