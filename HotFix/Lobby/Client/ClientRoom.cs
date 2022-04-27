using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using ET;
using UnityEngine;

namespace HotFix
{
    public class ClientRoom : BaseRoom
    {
        public ClientRoom(BaseRoomData data) : base(data)
        {
            m_PlayerDic = new Dictionary<int, BasePlayer>();
            //Debug.Log($"ClientRoom构造函数");
            for (int i = 0; i < data.Players.Count; i++)
            {
                var playerData = data.Players[i];
                var clientPlayer = new ClientPlayer(playerData);
                m_PlayerDic.Add(playerData.SeatId, clientPlayer);
                clientPlayer.SetRoomID(RoomID).SetSeatID(i).SetStatus(PlayerStatus.ROOM);
                //Debug.Log($"{i}---添加用户{clientPlayer.UserName}: 房间#{clientPlayer.RoomId}, 座位#{clientPlayer.SeatId}, 状态:{clientPlayer.Status}");
            }
        }

        public void UpdateData(BaseRoomData data)
        {
            for (int i = 0; i < data.Players.Count; i++)
            {
                var playerData = data.Players[i];
                var clientPlayer = new ClientPlayer(playerData);
                m_PlayerDic[playerData.SeatId] = clientPlayer;
            }
        }

        public static CardLib lib;
        // 保存乌龟棋子位置
        public Dictionary<ChessColor, int> chessPos; //棋子位置（key=棋子, value=位置）
        public Dictionary<int, List<ChessColor>> mapChess; //地图中每个格子的棋子，堆叠顺序（key=位置, value=堆叠顺序）
        // 保存自己的颜色和手牌
        public ChessColor chessColor;
        public List<Card> handCards; //索引是显示顺序
        public int NextTurn = 0; //下回合谁出牌（座位号）

        // 初始化
        private void Init()
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
            List<ChessColor> origin = mapChess[0];
            //Debug.Log($"起点叠了{origin.Count}层");
        }
        public void OnGameStart_Client(S2C_GameStartPacket packet)
        {
            this.Init();

            this.chessColor = (ChessColor)packet.Color;
            this.handCards = new List<Card>();
            for (int i = 0; i < packet.Cards.Count; i++)
            {
                int cardid = packet.Cards[i];
                Card card = lib.library[cardid];
                this.handCards.Add(card);
            }
            this.NextTurn = 0;
        }
        public List<int> OnGamePlay_Client(S2C_PlayCardPacket packet)
        {
            // 输入卡牌，返回要移动的棋子
            List<int> moveChessList = new List<int>();
            PrintHandCards();

            // 解析牌型
            int colorId = packet.Color;
            Card card = lib.library[packet.CardID];
            bool colorful = card.cardColor == CardColor.COLOR || card.cardColor == CardColor.SLOWEST;
            ChessColor colorKey = colorful ? (ChessColor)colorId : (ChessColor)card.cardColor; //哪只乌龟
            int step = (int)card.cardNum; //走几步

            // 如果是自己出的，移除手牌
            if (packet.SeatID == TcpChatClient.m_PlayerManager.LocalPlayer.SeatId)
            {
                handCards.Remove(card);
                PrintHandCards();
            }

            // 走棋子
            int curPos = chessPos[colorKey]; //某颜色棋子当前位置
            int dstPos = Mathf.Clamp(curPos + step, 0, 9); //前往位置
            if (curPos > 0)
            {
                // 考虑叠起来的情况。
                List<ChessColor> curGrid = mapChess[curPos];
                Debug.Log($"移动棋子{colorKey}，格子{curPos}上叠了{curGrid.Count}层");

                int index = curGrid.IndexOf(colorKey);
                Debug.Log($"目标棋子{colorKey}在格子{curPos}的第{index}层");

                for (int i = 0; i < curGrid.Count; i++)
                {
                    ChessColor chess = curGrid[i];
                    Debug.Log($"{i}---格子{curPos}上，第{i}层是{chess} / {curGrid.Count}");

                    if (i >= index)
                    {
                        chessPos[chess] = dstPos;

                        //mapChess[curPos].Remove(chess); //遍历中不能移除
                        mapChess[dstPos].Add(chess);

                        moveChessList.Add((int)chess);
                        Debug.Log($"<color=white>叠着走：{chess}</color>");
                    }
                }
                for (int i = index; i > 0; i--)
                {
                    ChessColor chess = curGrid[i];
                    curGrid.Remove(chess);
                }
            }
            else
            {
                chessPos[colorKey] = dstPos; //起点不堆叠

                Debug.Log($"起点不堆叠，从{curPos}移除{colorKey}");
                mapChess[curPos].Remove(colorKey);
                Debug.Log($"把{colorKey}添加到{dstPos}");
                mapChess[dstPos].Add(colorKey);

                moveChessList.Add((int)colorKey);
                Debug.Log($"<color=white>单个走：{colorKey}。" +
                    $"\n移动后，上个格子[{curPos}]{mapChess[curPos].Count}层。" +
                    $"这个格子[{dstPos}]{mapChess[dstPos].Count}层。</color>");
            }
            return moveChessList;
        }
        public void OnGameDeal_Client(Card card)
        {
            handCards.Add(card);
            PrintHandCards();
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
        public void PrintHandCards()
        {
            string handStr = $"{handCards.Count}张：";
            for (int i = 0; i < handCards.Count; i++)
                handStr += $"{i}--[{handCards[i].id}]、";
            Debug.Log(handStr);
        }

        public List<ChessColor> GetSlowest()
        {
            for (int i = 0; i < mapChess.Count; i++)
            {
                var grid = mapChess[i];
                if (grid.Count > 0)
                    return grid;
            }
            return null;
        }
    }
}