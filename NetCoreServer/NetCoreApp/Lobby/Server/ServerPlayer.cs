using TcpChatServer;
using HotFix;
using ET;

namespace NetCoreServer
{
    /* Զ���û� */
    public class ServerPlayer : BasePlayer
    {
        public readonly TcpSession Session; //����ֱ��Send��Ϣ

        public ServerPlayer(BasePlayerData data) : base(data)
        {
            Session = TCPChatServer.server.FindSession(data.PeerId);
        }

        public void SendAsync(PacketType msgId, object cmd)
        {
            if (m_Data.IsBot)
                return; //�Ի����˲�����
            byte[] header = new byte[1] { (byte)msgId };
            byte[] body = ProtobufHelper.ToBytes(cmd);
            byte[] buffer = new byte[header.Length + body.Length];
            System.Array.Copy(header, 0, buffer, 0, header.Length);
            System.Array.Copy(body, 0, buffer, header.Length, body.Length);
            //Debug.Print($"header:{header.Length},body:{body.Length},buffer:{buffer.Length},");
            Session.SendAsync(buffer);
        }
    }
}