using System;
using System.Collections.Generic;
using Random = System.Random;
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
            m_PlayerDic = new Dictionary<int, BasePlayer>();
            m_PlayerDic.Add(0, host); //房主座位号0
            host.SetRoomID(data.RoomID)
                .SetSeatID(0)
                .SetStatus(PlayerStatus.ROOM);
        }
        public int CurCount => m_PlayerDic.Count;
        public override Dictionary<int, BasePlayer> m_PlayerDic { get; protected set; }
        public override string ToString()
        {
            string playerStr = string.Empty;
            foreach (var item in m_PlayerDic)
            {
                var player = item.Value;
                playerStr += $"\n[{item.Key}]---{player.UserName}({player.NickName})(bot:{player.IsBot}), at{player.SeatId})";
            }
            return $"#{RoomID}[{RoomName}:{RoomPwd}] {CurCount}/{RoomLimit}: {playerStr}";
        }

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

            m_PlayerDic.Add(SeatID, p);
            p.SetRoomID(RoomID)
                .SetSeatID(SeatID)
                .SetStatus(PlayerStatus.ROOM);

            return true;
        }
        public bool RemovePlayer(BasePlayer p)
        {
            if (ContainsPlayer(p))
            {
                m_PlayerDic.Remove(p.SeatId);
                p.ResetToLobby();
                return true;
            }
            Debug.Print("严重的错误，无法移除房间");
            return false;
        }
        public bool RemoveAll()
        {
            foreach (var p in m_PlayerDic)
            {
                //ServerPlayer serverPlayer = p.Value as ServerPlayer;
                RemovePlayer(p.Value);
            }
            return false;
        }
        public bool ContainsPlayer(BasePlayer p)
        {
            BasePlayer basePlayer = null;
            if (m_PlayerDic.TryGetValue(p.SeatId, out basePlayer))
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
        // 组织用户数据
        public ServerPlayer GetPlayer(int seatId)
        {
            BasePlayer basePlayer = null;
            if (m_PlayerDic.TryGetValue(seatId, out basePlayer))
            {
                return (ServerPlayer)basePlayer;
            }
            return null;
        }
        // 组织房间数据
        public RoomInfo GetRoomInfo()
        {
            var temp = new List<PlayerInfo>();

            foreach (var item in m_PlayerDic)
            {
                var _player = item.Value;
                var _playerInfo = new PlayerInfo
                {
                    SeatID = item.Key,
                    UserName = _player.UserName,
                    NickName = _player.NickName,
                };
                temp.Add(_playerInfo);
            }
            var roomInfo = new RoomInfo //公共
            {
                RoomID = RoomID,
                RoomName = RoomName,
                HasPwd = !string.IsNullOrEmpty(RoomPwd),
                Pwd = RoomPwd,
                LimitNum = RoomLimit,
                Players = temp,
            };
            return roomInfo;
        }
        // 检查空座位
        public bool IsAvailableSeat(int seatId)
        {
            foreach (var p in m_PlayerDic)
            {
                if (p.Key == seatId)
                    return false;
            }
            return true;
        }
        const int MIN_INDEX = 0;
        const int MAX_INDEX = 4;
        private int GetAvailableRoomID()
        {
            int id = MIN_INDEX;

            if (m_PlayerDic.Count == 0)
                return id;

            for (int i = MIN_INDEX; i <= MAX_INDEX; i++)
            {
                BasePlayer player = null;
                if (m_PlayerDic.TryGetValue(i, out player) == false)
                {
                    id = i;
                    break;
                }
            }
            return id;
        }

        public void SendAsync(PacketType msgId, object cmd)
        {
            foreach(var p in m_PlayerDic)
            {
                ServerPlayer serverPlayer = p.Value as ServerPlayer;
                serverPlayer.SendAsync(msgId, cmd);
            }
        }
        #endregion

        #region 游戏逻辑
        public static CardLib lib; //所有牌
        private List<Card> cardList;
        private int nextIndex = 0; //下一张下发的牌
        public int nextPlayerIndex = 0; //下一个出牌的座位号
        private Dictionary<TurtleColor, int> chessPos; //棋子位置（key=棋子, value=位置）
        private Dictionary<int, List<TurtleColor>> GridData; //地图中每个格子的棋子，堆叠顺序（key=位置, value=堆叠顺序）
        private TurtleAnime gameStatus;
        public DateTime createTime;

        // TODO: 机器人随机出牌。优先选自己的+。优先选玩家的颜色-。
        private static List<int> CardToInt(List<Card> cards)
        {
            var list = new List<int>();
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                list.Add(card.id);
            }
            return list;
        }
        private static void Shuffle<T>(IList<T> deck)
        {
            Random rd = new Random();
            for (int n = deck.Count - 1; n > 0; --n)
            {
                int k = rd.Next(n + 1);
                T temp = deck[n];
                deck[n] = deck[k];
                deck[k] = temp;
            }
        }
        private void ShuffleStandard()
        {
            cardList = lib.Clone().standard;
            //Debug.Print($"洗牌：{cardList[0].id}、{cardList[1].id}、{cardList[2].id}、{cardList[3].id}、{cardList[4].id}");
        }
        private static byte[] AllotColor()
        {
            // 5色，不重复
            int count = (int)TurtleColor.COUNT;
            byte[] colors = new byte[] { 0, 1, 2, 3, 4 };

            // 打乱排序
            Random _rd = new Random();
            byte temp;
            for (int i = 0; i < count; i++)
            {
                int index = _rd.Next(0, count - 1);
                if (index != i)
                {
                    temp = colors[i];
                    colors[i] = colors[index];
                    colors[index] = temp;
                }
            }
            return colors;
        }
        public List<TurtleColor> GetSlowest()
        {
            for (int i = 0; i < GridData.Count; i++)
            {
                var grid = GridData[i];
                if (grid.Count > 0)
                    return grid;
            }
            return null;
        }

        void Init()
        {
            lib = new CardLib();
            nextIndex = 0;
            nextPlayerIndex = 0;
            chessPos = new Dictionary<TurtleColor, int>();
            chessPos.Add(TurtleColor.RED, 0);
            chessPos.Add(TurtleColor.YELLOW, 0);
            chessPos.Add(TurtleColor.GREEN, 0);
            chessPos.Add(TurtleColor.BLUE, 0);
            chessPos.Add(TurtleColor.PURPLE, 0);
            GridData = new Dictionary<int, List<TurtleColor>>();
            GridData.Add(0, new List<TurtleColor> { (TurtleColor)0, (TurtleColor)1, (TurtleColor)2, (TurtleColor)3, (TurtleColor)4 });
            for (int i = 1; i < 10; i++)
            {
                GridData.Add(i, new List<TurtleColor>());
            }
            gameStatus = TurtleAnime.Wait;
            createTime = DateTime.Now;
        }
        public void OnGameStart_Server()
        {
            this.Init();

            // 洗牌
            if (IsShuffleStandard)
            {
                ShuffleStandard();
            }
            else
            {
                cardList = lib.Clone().library;
                Shuffle(cardList);
            }

            // 准备颜色随机数
            var colors = AllotColor();
            // 遍历分配颜色
            for (int i = 0; i < CurCount; i++)
            {
                var player = (ServerPlayer)m_PlayerDic[i];
                player.Init();
                player.chessColor = (TurtleColor)colors[i];
            }
            // 遍历分配手牌（发5张）
            for (int n = 0; n < 5; n++)
            {
                for (int i = 0; i < CurCount; i++)
                {
                    var player = (ServerPlayer)m_PlayerDic[i];
                    OnGameDeal_Server(player);
                }
            }

            // 发送每个客户端的数据不一样
            for (int i = 0; i < CurCount; i++)
            {
                var player = (ServerPlayer)m_PlayerDic[i];
                //S2C_GameStartPacket packet = new S2C_GameStartPacket
                //{
                //    Color = (int)player.chessColor,
                //    Cards = CardToInt(player.handCards),
                //};
                //player.SendAsync(PacketType.S2C_GameStart, packet);
            }
        }
        public bool OnGamePlay_Server(ServerPlayer p, C2S_PlayCardPacket request)
        {
            if (gameStatus == TurtleAnime.End)
            {
                Debug.Print("比赛已经结束");
                return true;
            }

            int cardId = request.CardID;
            int colorId = request.Color; //彩色时才有效

            // 解析牌型
            Card card = lib.library[cardId];
            // 如果是彩色，转成实际的颜色
            bool colorful = card.cardColor == CardColor.COLOR || card.cardColor == CardColor.SLOWEST;
            TurtleColor colorKey = colorful ? (TurtleColor)colorId : (TurtleColor)card.cardColor; //哪只乌龟
            int step = (int)card.cardNum; //走几步
            // 检测作弊或Bug
            if (card.cardColor == CardColor.SLOWEST)
            {
                var slowestArray = GetSlowest();
                if (slowestArray.Contains((TurtleColor)colorId) == false)
                {
                    Debug.Print($"玩家选的颜色{(TurtleColor)colorId}，不是最慢的");
                    return false;
                }
            }

            // 走棋子
            int curPos = 0;
            try
            {
                curPos = chessPos[colorKey]; //某颜色棋子当前位置
            }
            catch
            {
                Debug.Print($"错误的key：{colorKey} / {chessPos.Count}");
            }
            int dstPos = System.Math.Clamp(curPos + step, 0, 9); //前往位置
            if (curPos > 0)
            {
                // 考虑叠起来的情况。
                List<TurtleColor> temp = new List<TurtleColor>();
                List<TurtleColor> curGrid = GridData[curPos];

                int index = curGrid.IndexOf(colorKey);

                for (int i = 0; i < curGrid.Count; i++)
                {
                    TurtleColor chess = curGrid[i];

                    if (i >= index)
                    {
                        chessPos[chess] = dstPos;

                        temp.Add(chess);
                        GridData[dstPos].Add(chess);
                        Debug.Print($"{i}+++++++++将棋子{chess}移到{dstPos}, temp.count={temp.Count}");

                        //moveChessList.Add((int)chess);
                    }
                }
                for (int i = 0; i < temp.Count; i++)
                {
                    TurtleColor chess = temp[i];
                    curGrid.Remove(chess);
                    Debug.Print($"{i}---------从格子{curPos}移除{chess}, curGrid.count={curGrid.Count}");
                }
            }
            else
            {
                chessPos[colorKey] = dstPos; //起点不堆叠

                GridData[curPos].Remove(colorKey);
                GridData[dstPos].Add(colorKey);

                //moveChessList.Add((int)colorKey);
            }

            // 下个出牌人
            nextPlayerIndex++;
            if (nextPlayerIndex >= CurCount)
                nextPlayerIndex = 0;

            // 检查是否到终点
            Debug.Print($"检查是否到终点: {dstPos}");
            if (dstPos >= 9)
            {
                OnGameResult();
                return true;
            }
            return false;
        }
        public Card OnGameDeal_Server(ServerPlayer player)
        {
            var card = cardList[nextIndex];
            player.handCards.Add(card);
            nextIndex++;

            // 牌发完了，洗牌
            if (nextIndex >= cardList.Count)
            {
                nextIndex = 0;
                if (IsShuffleStandard)
                {
                    ShuffleStandard();
                }
                else
                {
                    cardList = lib.Clone().library;
                    Shuffle(cardList);
                }
            }

            return card;
        }
        public List<int> OnGameResult()
        {
            Debug.Print("给出结算");
            gameStatus = TurtleAnime.End;
            var turtles = new List<int>();
            for (int i = GridData.Count - 1; i >= 0; i--) //从终点开始遍历
            {
                var grid = GridData[i];
                for (int t = 0; t < grid.Count; t++)
                {
                    int chess = (int)grid[t];
                    turtles.Add(chess);
                }
            }
            return turtles;
        }

        // 调试参数
        private bool IsShuffleStandard; //洗成固定的顺序
        public string PrintMap()
        {
            //输出玩家列表
            //输出剩余卡牌池
            //输棋子的位置
            string posStr = $"棋子的位置：";
            for (int i = 0; i < GridData.Count; i++)
            {
                var grid = GridData[i];
                if (grid.Count > 0)
                {
                    posStr += $"\n[第{i}格]";
                    for (int t = 0; t < grid.Count; t++)
                    {
                        posStr += $"{grid[t]}、";
                    }
                }
            }
            return posStr;
        }
        #endregion
    }
}