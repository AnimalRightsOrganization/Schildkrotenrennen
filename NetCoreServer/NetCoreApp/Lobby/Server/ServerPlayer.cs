using System.Collections.Generic;
using TcpChatServer;
using HotFix;
using ET;

namespace NetCoreServer
{
    /* 远程用户 */
    public class ServerPlayer : BasePlayer
    {
        public readonly TcpSession Session; //用来直接Send消息

        public ServerPlayer(BasePlayerData data) : base(data)
        {
            Session = TCPChatServer.server.FindSession(data.PeerId);
        }

        public void SendAsync(PacketType msgId, object cmd)
        {
            if (m_Data.IsBot)
                return; //对机器人不发送
            byte[] header = new byte[1] { (byte)msgId };
            byte[] body = ProtobufHelper.ToBytes(cmd);
            byte[] buffer = new byte[header.Length + body.Length];
            System.Array.Copy(header, 0, buffer, 0, header.Length);
            System.Array.Copy(body, 0, buffer, header.Length, body.Length);
            //Debug.Print($"header:{header.Length},body:{body.Length},buffer:{buffer.Length},");
            Session.SendAsync(buffer);
        }

        // 保存自己的颜色和手牌
        public ChessColor chessColor;
        public List<Card> handCards; //长度永远是5
        public void Init()
        {
            chessColor = ChessColor.NONE; //空，等待指定
            handCards = new List<Card>(); //空，等待发牌
        }
    }
}