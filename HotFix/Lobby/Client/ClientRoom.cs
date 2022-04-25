using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using ET;

namespace HotFix
{
    public class ClientRoom : BaseRoom
    {
        public ClientRoom(BaseRoomData data) : base(data)
        {
            m_PlayerList = new Dictionary<int, BasePlayer>();
            //Debug.Log($"ClientRoom构造函数");
            for (int i = 0; i < data.Players.Count; i++)
            {
                var playerData = data.Players[i];
                var clientPlayer = new ClientPlayer(playerData);
                m_PlayerList.Add(playerData.SeatId, clientPlayer);
                clientPlayer.SetRoomID(RoomID).SetSeatID(i).SetStatus(PlayerStatus.ROOM);
                Debug.Log($"{i}---添加用户{clientPlayer.UserName}: 房间#{clientPlayer.RoomId}, 座位#{clientPlayer.SeatId}, 状态:{clientPlayer.Status}");
            }
        }

        public void UpdateData(BaseRoomData data)
        {
            for (int i = 0; i < data.Players.Count; i++)
            {
                var playerData = data.Players[i];
                var clientPlayer = new ClientPlayer(playerData);
                m_PlayerList[playerData.SeatId] = clientPlayer;
            }
        }

        public static CardLib lib;
        // 保存乌龟棋子位置
        public Dictionary<ChessColor, int> chessPos; //棋子位置（key=棋子, value=位置）
        public Dictionary<int, List<ChessColor>> mapChess; //地图中每个格子的棋子，堆叠顺序（key=位置, value=堆叠顺序）
        // 保存自己的颜色和手牌
        public ChessColor chessColor;
        public List<Card> handCards; //长度永远是5

        // 初始化
        public void Init()
        {
            lib = new CardLib();
            chessColor = ChessColor.NONE; //空，等待指定
            handCards = new List<Card>(); //空，等待发牌
            chessPos = new Dictionary<ChessColor, int>();
            chessPos.Add(ChessColor.RED, 0);
            chessPos.Add(ChessColor.YELLOW, 0);
            chessPos.Add(ChessColor.GREEN, 0);
            chessPos.Add(ChessColor.BLUE, 0);
            chessPos.Add(ChessColor.PURPLE, 0);
            mapChess = new Dictionary<int, List<ChessColor>>();
            mapChess.Add(0, new List<ChessColor> { (ChessColor)0, (ChessColor)1, (ChessColor)2, (ChessColor)3, (ChessColor)4 });
            for (int i = 1; i < 10; i++)
            {
                mapChess.Add(i, new List<ChessColor>());
            }
        }
        public void OnGameStart_Client(S2C_GameStartPacket packet)
        {
            this.Init();

            chessColor = (ChessColor)packet.Color;
            TcpChatClient.m_ClientRoom.handCards = new List<Card>();
            for (int i = 0; i < packet.Cards.Count; i++)
            {
                int cardid = packet.Cards[i];
                Card card = lib.library[cardid];
                TcpChatClient.m_ClientRoom.handCards.Add(card);
            }
        }
        public List<int> OnGamePlay(int handIndex, int colorId)
        {
            List<int> chessArray = new List<int>(); 

            Card card = handCards[handIndex];
            bool colorful = card.cardColor == CardColor.COLOR || card.cardColor == CardColor.SLOWEST;
            ChessColor colorKey = colorful ? (ChessColor)colorId : (ChessColor)card.cardColor; //哪只乌龟
            int step = (int)card.cardNum; //走几步

            handCards.RemoveAt(handIndex);

            // 走棋子
            int curPos = chessPos[colorKey]; //某颜色棋子当前位置
            int dstPos = curPos + step; //前往位置
            if (curPos > 0)
            {
                // 考虑叠起来的情况。
                List<ChessColor> origin = mapChess[curPos];
                int index = origin.IndexOf(colorKey);
                for (int i = 0; i < origin.Count; i++)
                {
                    if (i >= index)
                    {
                        chessPos[(ChessColor)i] = dstPos;
                        chessArray.Add(i);
                    }
                }
            }
            else
            {
                chessPos[colorKey] = dstPos; //起点不堆叠
                chessArray.Add((int)colorKey);
            }
            return chessArray;
        }
        public void OnGameDeal(Card card)
        {
            handCards.Add(card);
        }
        public void PrintRoom()
        {
            string content = $"颜色={chessColor}，手牌=";
            for (int i = 0; i < handCards.Count; i++)
            {
                Card handCard = handCards[i];
                content += $"{handCard.id}、";
            }
            content += $"棋子：";
            UnityEngine.Debug.Log(content);
        }
    }
}