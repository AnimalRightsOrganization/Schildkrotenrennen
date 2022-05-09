using System.Collections.Generic;
using TcpChatServer;
using HotFix;
using ET;
using Random = System.Random;
using Debug = System.Diagnostics.Debug;

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

        // �����Լ�����ɫ������
        public TurtleColor chessColor;
        public List<Card> handCards; //������Զ��5
        public void Init()
        {
            chessColor = TurtleColor.NONE; //�գ��ȴ�ָ��
            handCards = new List<Card>(); //�գ��ȴ�����
        }

        #region ������
        public C2S_PlayCardPacket Bot_PlayCardPacket()
        {
            Debug.Print($"�ȴ������������˵����ƣ�{handCards[0].id},{handCards[1].id},{handCards[2].id},{handCards[3].id},{handCards[4].id}");

            int index = new Random().Next(0, 5); //[,)
            Card card = handCards[index];
            int color = 0;
            if (card.cardColor == CardColor.COLOR)
            {
                color = new Random().Next(0, 5);
            }
            else if (card.cardColor == CardColor.SLOWEST)
            {
                var serverRoom = TCPChatServer.m_RoomManager.GetServerRoom(RoomId);
                var list = serverRoom.GetSlowest();
                int rd = new Random().Next(0, list.Count);
                color = (int)list[rd];
            }

            var request = new C2S_PlayCardPacket
            {
                CardID = card.id,
                Color = color,
            };
            return request;
        }
        #endregion
    }
}