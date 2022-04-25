using System.Collections.Generic;
using Debug = System.Diagnostics.Debug;
using HotFix;
using ET;

namespace NetCoreServer
{
    public class ServerRoom : BaseRoom
    {
        #region 房间管理
        public ServerRoom(ServerPlayer host, BaseRoomData data) : base(data)
        {
            // 被override的才需要在这里赋值
            m_PlayerList = new Dictionary<int, BasePlayer>();
            m_PlayerList.Add(0, host); //房主座位号0
            host.SetRoomID(data.RoomID)
                .SetSeatID(0)
                .SetStatus(PlayerStatus.ROOM);
        }

        public override Dictionary<int, BasePlayer> m_PlayerList { get; protected set; }
        public int CurCount => m_PlayerList.Count;

        public bool AddPlayer(BasePlayer p, int seatId = -1)
        {
            if (ContainsPlayer(p))
            {
                Debug.Print("严重的错误，重复加入");
                return false;
            }

            int SeatID = 0;
            if (seatId == -1)
                SeatID = GetAvailableRoomID(); //真实玩家，没有赋值，自动选取空座位
            else
                SeatID = seatId; //机器人是指定的座位

            m_PlayerList.Add(SeatID, p);
            p.SetRoomID(RoomID)
                .SetSeatID(SeatID)
                .SetStatus(PlayerStatus.ROOM);

            return true;
        }
        public bool RemovePlayer(BasePlayer p)
        {
            if (ContainsPlayer(p))
            {
                m_PlayerList.Remove(p.SeatId);
                p.ResetToLobby();
                return true;
            }
            Debug.Print("严重的错误，无法移除房间");
            return false;
        }
        public bool RemoveAll()
        {
            foreach (var p in m_PlayerList)
            {
                //ServerPlayer serverPlayer = p.Value as ServerPlayer;
                RemovePlayer(p.Value);
            }
            return false;
        }
        public bool ContainsPlayer(BasePlayer p)
        {
            BasePlayer basePlayer = null;
            if (m_PlayerList.TryGetValue(p.SeatId, out basePlayer))
            {
                if (p.PeerId != basePlayer.PeerId)
                {
                    Debug.Print("严重的错误，找错人了");
                    return false;
                }
                return true;
            }
            return false;
        }
        public ServerPlayer GetPlayer(int seatId)
        {
            BasePlayer basePlayer = null;
            if (m_PlayerList.TryGetValue(seatId, out basePlayer))
            {
                return (ServerPlayer)basePlayer;
            }
            return null;
        }

        // 检查空座位
        public bool IsAvailableSeat(int seatId)
        {
            foreach (var p in m_PlayerList)
            {
                if (p.Key == seatId)
                    return false;
            }
            return true;
        }
        const int MIN_INDEX = 0;
        const int MAX_INDEX = 4;
        int GetAvailableRoomID()
        {
            int id = MIN_INDEX;

            if (m_PlayerList.Count == 0)
                return id;

            for (int i = MIN_INDEX; i <= MAX_INDEX; i++)
            {
                BasePlayer player = null;
                if (m_PlayerList.TryGetValue(i, out player) == false)
                {
                    id = i;
                    break;
                }
            }
            return id;
        }

        public void SendAsync(PacketType msgId, object cmd)
        {
            foreach(var p in m_PlayerList)
            {
                ServerPlayer serverPlayer = p.Value as ServerPlayer;
                serverPlayer.SendAsync(msgId, cmd);
            }
        }

        public override string ToString()
        {
            string playerStr = string.Empty;
            foreach (var item in m_PlayerList)
            {
                var player = item.Value;
                playerStr += $"\n[{item.Key}]---{player.UserName}({player.NickName})(bot:{player.IsBot}), at{player.SeatId})";
            }
            return $"#{RoomID}[{RoomName}:{RoomPwd}] {CurCount}/{RoomLimit}: {playerStr}";
        }
        #endregion

        #region 游戏逻辑

        public static CardLib lib; //所有牌
        List<Card> cardList;
        int nextIndex = 0; // 下一张下发的牌
        public int nextPlayerIndex = 0; //下一个出牌的座位号
        public Dictionary<ChessColor, int> chessPos; //棋子位置（key=棋子, value=位置）
        public Dictionary<int, List<ChessColor>> mapChess; //地图中每个格子的棋子，堆叠顺序（key=位置, value=堆叠顺序）
        // TODO: 保存每步操作。
        // TODO: 机器人随机出牌。优先选自己的+。优先选玩家的颜色-。

        void Init()
        {
            lib = new CardLib();
            nextIndex = 0;
            nextPlayerIndex = 0;
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

        // 开局消息
        public void OnGameStart_Server()
        {
            this.Init();

            // 洗牌
            cardList = lib.Clone().library;
            GameLogic.Shuffle(cardList);

            // 准备颜色随机数
            var colors = GameLogic.AllotColor();
            // 遍历分配颜色
            for (int i = 0; i < CurCount; i++)
            {
                var player = (ServerPlayer)m_PlayerList[i];
                player.Init();
                player.chessColor = (ChessColor)colors[i];
            }
            // 遍历分配手牌（发5张）
            for (int n = 0; n < 5; n++)
            {
                for (int i = 0; i < CurCount; i++)
                {
                    var player = (ServerPlayer)m_PlayerList[i];
                    OnGameDeal(player);
                }
            }

            // 发送每个客户端的数据不一样
            for (int i = 0; i < CurCount; i++)
            {
                var player = (ServerPlayer)m_PlayerList[i];
                S2C_GameStartPacket packet = new S2C_GameStartPacket
                {
                    Color = (int)player.chessColor,
                    Cards = CardToInt(player.handCards),
                };
                player.SendAsync(PacketType.S2C_GameStart, packet);
            }
        }
        static List<int> CardToInt(List<Card> cards)
        {
            var list = new List<int>();
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                list.Add(card.id);
            }
            return list;
        }

        // 收到出牌，处理棋子
        public bool TurnNext(ServerPlayer p, C2S_PlayCardPacket request)
        {
            int seatId = p.SeatId;
            int cardId = request.CardID;
            int colorId = request.Color; //彩色时才有效

            // 解析牌型
            Card card = lib.library[cardId];
            // 如果是彩色，转成实际的颜色
            bool colorful = card.cardColor == CardColor.COLOR || card.cardColor == CardColor.SLOWEST;
            ChessColor colorKey = colorful ? (ChessColor)colorId : (ChessColor)card.cardColor; //哪只乌龟
            int step = (int)card.cardNum; //走几步

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
                        chessPos[(ChessColor)i] = dstPos;
                }
            }
            else
            {
                chessPos[colorKey] = dstPos; //起点不堆叠
            }

            // 下个出牌人
            nextPlayerIndex++;
            if (nextPlayerIndex >= CurCount)
                nextPlayerIndex = 0;

            // 检查是否到终点
            if (dstPos >= 9)
            {
                OnGameResult();
                return true;
            }
            return false;
        }

        // 收到出牌，发一张新牌
        public Card OnGameDeal(ServerPlayer player)
        {
            var card = cardList[nextIndex];
            player.handCards.Add(card);
            nextIndex++;

            // 牌发完了，洗牌
            if (nextIndex >= cardList.Count)
            {
                nextIndex = 0;
                GameLogic.Shuffle(cardList);
            }

            return card;
        }

        // 结算
        public List<int> OnGameResult()
        {
            Debug.Print("给出结算");
            var list = new List<int>();
            for (int i = mapChess.Count - 1; i > 0; i--)
            {
                var grid = mapChess[i];
                for (int t = 0; t < grid.Count - 1; t++)
                {
                    var chess = grid[t];
                    list.Add((int)chess);
                }
            }
            return list;
        }

        #endregion
    }
}