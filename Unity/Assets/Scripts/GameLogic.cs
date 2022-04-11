using System.Collections.Generic;
using Random = System.Random;
using Debug = UnityEngine.Debug;

// 承载输入输出的逻辑盒子
public class GameLogic
{
    //protected int seed;
    static Random rd;

    public int playerCount; //本局玩家数，开房间时确定
    public List<GamePlayer> playerList; //玩家列表

    List<CardAttribute> libraryList; //所有牌
    List<CardAttribute> deskList = new List<CardAttribute>(); //桌上的牌

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
                gameid = i,
                user_id = "wx_" + rd.Next(10000, 99999),
            };
            playerList.Add(player);
            Debug.Log($"添加玩家{i}");
        }
    }

    // 初始化棋盘（仅单机）
    public void InitMap()
    {
        //var obj = ResManager.LoadPrefab("Prefabs/runner");
    }

    // 开局，分配玩家颜色
    public void AllotColor()
    {
        // 5色，不重复
        List<RunnerColor> colors = new List<RunnerColor>()
        {
            (RunnerColor)0,
            (RunnerColor)1,
            (RunnerColor)2,
            (RunnerColor)3,
            (RunnerColor)4,
        };

        Random _rd = new Random();
        int index = 0;
        RunnerColor temp;
        for (int i = 0; i < colors.Count; i++)
        {
            index = _rd.Next(0, colors.Count - 1);
            if (index != i)
            {
                temp = colors[i];
                colors[i] = colors[index];
                colors[index] = temp;
            }
        }

        for (int i = playerList.Count - 1; i >= 0; i--)
        {
            var player = playerList[i];
            var color = colors[i];
            Debug.Log($"玩家{i}---颜色{color}");
        }
    }

    // 洗牌
    public static void Shuffle<T>(IList<T> deck)
    {
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
        CardData cards = new CardData();
        libraryList = cards.libraryList;
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

                CardAttribute card = libraryList[index];

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