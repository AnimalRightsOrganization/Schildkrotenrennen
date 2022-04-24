using System.Collections.Generic;
using Random = System.Random;
#if NOT_UNITY
using Debug = System.Diagnostics.Debug;
#else
using Debug = UnityEngine.Debug;
#endif

namespace HotFix
{
    // 公共算法黑盒
    public partial class GameLogic
    {
        //protected int seed;
        static Random rd;

        public int playerCount; //本局玩家数，开房间时确定
        public List<GamePlayer> playerList; //玩家列表

        List<Card> libraryList; //所有牌
        //List<Card> deskList; //桌上的牌

        // 开局，分配玩家颜色
        public static byte[] AllotColor()
        {
            // 5色，不重复
            int count = (int)ChessColor.COUNT;
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
    }
    // 客户端逻辑
    public partial class GameLogic
    {
        public void OnGetRoomList()
        {

        }

        // 房间内广播
        public void OnEnterRoom()
        {

        }

        // 房间内广播
        public void OnLeaveRoom()
        {

        }

        // 通知玩家切换场景，房间内广播
        public void OnGameStart()
        {
            //S2C_GameStart，下发房间号、玩家ID
            //玩家颜色，初始手牌5张。
        }

        // 某人出牌，房间内广播
        public void OnPlay()
        {
            //S2C_Play，下发玩家ID，出牌ID
        }

        // 发牌给自己，房间内广播
        public void OnDeal()
        {
            //S2C_Deal，下发玩家ID(自己)，得牌ID
        }

        // 弹出结算，房间内广播
        public void OnGameResult()
        {

        }

        // 创建房间
        public void CreateRoom()
        {
            // 0代表单机
            rd = new Random();
            playerCount = 2;
            int roomId = rd.Next(1000, 10000); //服务器分配，单机填0

            playerList = new List<GamePlayer>();
            for (int i = 0; i < playerCount; i++)
            {
                GamePlayer player = new GamePlayer()
                {
                    seatId = i,
                    user_id = "wx_" + rd.Next(10000, 99999),
                };
                playerList.Add(player);
                Debug.Print($"添加玩家{i}");
            }
        }

        // 初始化棋盘（仅单机）
        public void InitMap()
        {

        }

        // 洗牌
        public static void Shuffle<T>(IList<T> deck)
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

        // 初始化卡牌配置
        public void InitOption()
        {
            var lib = new CardLib();
            libraryList = lib.Clone().library;
            Shuffle(libraryList);
        }

        // 发牌（5张）
        // 发牌，nextTurn升序一人一张轮着发
        public int nextTurn = 0; //流程控制，当前出牌玩家id
        public void Deal()
        {
            // 发五轮
            for (int k = 0; k < 5; k++)
            {
                // 共多少玩家
                for (int j = 0; j < playerCount; j++)
                {
                    // 从牌库抽一张牌
                    int index = rd.Next(1, libraryList.Count);

                    Card card = libraryList[index];

                    // 把牌放入玩家手中
                    playerList[nextTurn].handCardsList.Add(card);

                    // 把牌从牌库中移除
                    libraryList.Remove(card);

                    // 下一轮
                    //Debug.LogFormat("玩家{0} - 第{1}轮 - 抽第{2}张 - 当前剩余{3}", j, k, index, libraryList.Count);
                    nextTurn++;
                    if (nextTurn > playerCount - 1)
                    {
                        nextTurn = 0;
                    }
                }
            }

            //TODO: 如果牌库不足10张，执行洗牌
        }

        // 出牌

        // 出牌后再抽取一张
    }
    // 服务器逻辑
    public partial class GameLogic
    {

        public void OnGetRoomList_Server()
        {

        }

        public void OnEnterRoom_Server()
        {

        }

        public void OnLeaveRoom_Server()
        {

        }

        // 服务器收到房主按下开始
        //public void OnGameStart_Server(C2S_GameStart cmd)
        //{
        //    var colors = AllotColor();
        //    for (int i = 0; i < colors.Length; i++)
        //    {
        //        Debug.Log($"打印随机数：{i}---{colors[i]}");
        //    }

        //    S2C_GameStart info = new S2C_GameStart
        //    {
        //        RoomID = cmd.RoomID,
        //        Seats = new S2C_SeatInfo[]
        //        {
        //        new S2C_SeatInfo { },
        //        new S2C_SeatInfo { },
        //        },
        //    };
        //    //TODO: 发送给客户端
        //}
    }
}