using System.Collections.Generic;
using kcp2k.Examples;
using HotFix;
using ET;
using Random = System.Random;
using Debug = System.Diagnostics.Debug;

namespace NetCoreServer
{
    public class ServerPlayer : BasePlayer
    {
        public readonly int peerId;

        public ServerPlayer(BasePlayerData data) : base(data)
        {
            peerId = base.PeerId;
        }

        public void SendAsync(PacketType msgId, object cmd)
        {
            if (m_Data.IsBot)
                return; //对机器人不发送
            KcpChatServer.Get.SendAsync(peerId, msgId, cmd);
        }

        // 保存自己的颜色和手牌
        public TurtleColor chessColor;
        public List<Card> handCards; //长度永远是5
        public void Init()
        {
            chessColor = TurtleColor.NONE; //空，等待指定
            handCards = new List<Card>(); //空，等待发牌
        }

        #region 机器人
        ///*
        public C2S_PlayCardPacket Bot_PlayCardPacket()
        {
            Debug.Print($"等待结束。机器人的手牌：{handCards[0].id},{handCards[1].id},{handCards[2].id},{handCards[3].id},{handCards[4].id}");

            int index = new Random().Next(0, 5); //[,)
            Card card = handCards[index];
            int color = 0;
            if (card.cardColor == CardColor.COLOR)
            {
                color = new Random().Next(0, 5);
            }
            else if (card.cardColor == CardColor.SLOWEST)
            {
                var serverRoom = KcpChatServer.m_RoomManager.GetServerRoom(RoomId);
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
        //*/
        #endregion
    }
}