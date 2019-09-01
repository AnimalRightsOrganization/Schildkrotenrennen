/*
//5身份牌
//5棋子

//开始
//而由最年辁妁玩家开始。以顺时 钟方南米进行游戊。

//结算
//一人到达即结束
//同时到达，越上面的排名靠前
*/
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class GameManager : wSingleton<GameManager>
{
    public const int CARD_COUNT = 52;
    [SerializeField] List<CardAttribute> libraryList;
    [SerializeField] List<CardAttribute> deskList = new List<CardAttribute>(); //桌上的牌
    [SerializeField, Range(2, 6)] int playerCount = 2; //本局玩家数，开房间时确定
    public List<GamePlayer> playerList = new List<GamePlayer>(); //玩家列表

    void Start()
    {
        CreateRoom(2);

        InitOption();
        Shuffle(libraryList);
        Deal();
    }

    // 创建房间，加入count个玩家
    public void CreateRoom(int count)
    {
        playerCount = count;

        ///TODO: 4位RoomID，由服务器分配
        int roomId = Random.Range(1000, 10000);

        ///TODO: 等待其他玩家加入

        // 从总玩家数max中，随机取count个数
        /*
        int max = System.Enum.GetNames(typeof(AvatarModel)).GetLength(0);
        int[] startArray = new int[max];
        int[] resultArray = new int[playerCount]; //结果存放在里面
        for (int i = 0; i < max; i++)
        {
            startArray[i] = i;
        }
        for (int i = 0; i < playerCount; i++)
        {
            int seed = Random.Range(0, startArray.Length - i); //从剩下的随机数里生成
            resultArray[i] = startArray[seed];//赋值给结果数组
            startArray[seed] = startArray[startArray.Length - i - 1]; //把随机数产生过的位置替换为未被选中的值。
        }*/

        // 创建当局玩家列表
        playerList.Clear();
        for (int i = 0; i < playerCount; i++)
        {
            GamePlayer player = new GamePlayer()
            {
                gameid = i,
                user_id = "wx_" + Random.Range(10000, 99999),
                //user_id = botDatabase.botList[resultArray[i]].user_id,
                //avatar = (AvatarModel)resultArray[i],
                //money = botDatabase.botList[resultArray[i]].money
            };

            //PlayerProvider scirpt = AvatarPool.instance.Spawn(player);
            //scirpt.mPlayer = player; //赋值
            //SpawnPoints.instance.SitDown(scirpt.transform); //入座

            playerList.Add(player);
        }
    }

    // 初始化卡牌配置
    public void InitOption()
    {
        string path = Application.streamingAssetsPath + "/CardJson.json";
        string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
        libraryList = JsonMapper.ToObject<List<CardAttribute>>(json);
    }
    
    // 洗牌
    static System.Random rd = new System.Random();
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
                int index = Random.Range(1, libraryList.Count);

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

        ///TODO: 如果牌库不足10张，执行洗牌
    }
    //打出一张牌，抽一张牌
    public void DealOnce(int chair_id)
    {
        if (playerList[chair_id].handCardsList.Count < 5)
        {
            // 从牌库抽一张牌
            int index = Random.Range(1, libraryList.Count);

            CardAttribute card = libraryList[index];

            // 把牌放入玩家手中
            playerList[chair_id].handCardsList.Add(card);

            // 把牌从牌库中移除
            libraryList.Remove(card);
        }
    }

    // 出牌
    public void Play(CardAttribute card)
    {

    }
}

// 52张牌
/*
 * 5色-1，5*2=10
 * 彩色-1，2
 * 5色+1，5*5=25
 * 彩色+1，5
 * 5色+2，5
 * 慢+1，3
 * 慢+2，2
 * */
public enum RunnerColor
{
    Red = 0,
    Yellow = 1,
    Green = 2,
    Blue = 3,
    Purple = 4
}
public enum CardColor
{
    Red = 0,
    Yellow = 1,
    Green = 2,
    Blue = 3,
    Purple = 4,
    Color = 5,
    Slow = 6
}
public enum CardNum
{
    NegtiveOne = -1,
    PositiveOne = 1,
    PositiveTwo = 2,
}
[System.Serializable]
public class CardAttribute
{
    public int id; //索引号0-51
    public CardColor cardColor;
    public CardNum cardNum;
}
// 数据库玩家信息
[System.Serializable]
public class Player
{
    public string user_id; //用户名 nn_1234567890
    //public AvatarModel avatar;
    public long money;
    public string nickname;
    //public string gender;
    //public string headimg;
}
// 牌局玩家属性
[System.Serializable]
public class GamePlayer : Player
{
    public int gameid; //出牌顺位
    public List<CardAttribute> handCardsList = new List<CardAttribute>(); //手牌List
}
