using System.Collections.Generic;

namespace HotFix
{
    /*
    //5身份牌
    //5棋子

    //开始
    //而由最年辁妁玩家开始。以顺时 钟方南米进行游戊。

    //结算
    //一人到达即结束
    //同时到达，越上面的排名靠前
    */

    /* 52张牌
     * 5色-1，5*2=10
     * 彩色-1，2
     * 5色+1，5*5=25
     * 彩色+1，5
     * 5色+2，5
     * 最慢+1，3
     * 最慢+2，2
     * */
    public enum RunnerColor : byte
    {
        Red = 0,
        Yellow = 1,
        Green = 2,
        Blue = 3,
        Purple = 4,
        COUNT = 5,
    }
    public enum CardColor
    {
        NONE    = -1, //未指定的
        Red     = 0,
        Yellow  = 1,
        Green   = 2,
        Blue    = 3,
        Purple  = 4,
        Color   = 5,
        Slowest = 6,
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
}