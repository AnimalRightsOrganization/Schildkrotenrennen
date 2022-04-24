using System.Collections.Generic;

namespace HotFix
{
    // 牌库配置文件（52张）
    /* 5色-1，5*2=10
     * 彩色-1，2
     * 5色+1，5*5=25
     * 彩色+1，5
     * 5色+2，5
     * 最慢+1，3
     * 最慢+2，2 */
    public interface ICloneable<T>
    {
        T Clone();
    }
    public class CardLib : ICloneable<CardLib>
    {
        public CardLib()
        {
            library = new List<Card>()
            {
                // 五色、彩色-1
                new Card { id = 0,  cardColor = (CardColor)(0), cardNum = (CardNum)(-1) },
                new Card { id = 1,  cardColor = (CardColor)(0), cardNum = (CardNum)(-1) },
                new Card { id = 2,  cardColor = (CardColor)(1), cardNum = (CardNum)(-1) },
                new Card { id = 3,  cardColor = (CardColor)(1), cardNum = (CardNum)(-1) },
                new Card { id = 4,  cardColor = (CardColor)(2), cardNum = (CardNum)(-1) },
                new Card { id = 5,  cardColor = (CardColor)(2), cardNum = (CardNum)(-1) },
                new Card { id = 6,  cardColor = (CardColor)(3), cardNum = (CardNum)(-1) },
                new Card { id = 7,  cardColor = (CardColor)(3), cardNum = (CardNum)(-1) },
                new Card { id = 8,  cardColor = (CardColor)(4), cardNum = (CardNum)(-1) },
                new Card { id = 9,  cardColor = (CardColor)(4), cardNum = (CardNum)(-1) },
                new Card { id = 10, cardColor = (CardColor)(5), cardNum = (CardNum)(-1) },
                new Card { id = 11, cardColor = (CardColor)(5), cardNum = (CardNum)(-1) },
                // 红色+1
                new Card { id = 12, cardColor = (CardColor)(0), cardNum = (CardNum)(1) },
                new Card { id = 13, cardColor = (CardColor)(0), cardNum = (CardNum)(1) },
                new Card { id = 14, cardColor = (CardColor)(0), cardNum = (CardNum)(1) },
                new Card { id = 15, cardColor = (CardColor)(0), cardNum = (CardNum)(1) },
                new Card { id = 16, cardColor = (CardColor)(0), cardNum = (CardNum)(1) },
                // 黄色+1
                new Card { id = 17, cardColor = (CardColor)(1), cardNum = (CardNum)(1) },
                new Card { id = 18, cardColor = (CardColor)(1), cardNum = (CardNum)(1) },
                new Card { id = 19, cardColor = (CardColor)(1), cardNum = (CardNum)(1) },
                new Card { id = 20, cardColor = (CardColor)(1), cardNum = (CardNum)(1) },
                new Card { id = 21, cardColor = (CardColor)(1), cardNum = (CardNum)(1) },
                // 绿色+1
                new Card { id = 22, cardColor = (CardColor)(2), cardNum = (CardNum)(1) },
                new Card { id = 23, cardColor = (CardColor)(2), cardNum = (CardNum)(1) },
                new Card { id = 24, cardColor = (CardColor)(2), cardNum = (CardNum)(1) },
                new Card { id = 25, cardColor = (CardColor)(2), cardNum = (CardNum)(1) },
                new Card { id = 26, cardColor = (CardColor)(2), cardNum = (CardNum)(1) },
                // 蓝色+1
                new Card { id = 27, cardColor = (CardColor)(3), cardNum = (CardNum)(1) },
                new Card { id = 28, cardColor = (CardColor)(3), cardNum = (CardNum)(1) },
                new Card { id = 29, cardColor = (CardColor)(3), cardNum = (CardNum)(1) },
                new Card { id = 30, cardColor = (CardColor)(3), cardNum = (CardNum)(1) },
                new Card { id = 31, cardColor = (CardColor)(3), cardNum = (CardNum)(1) },
                // 紫色+1
                new Card { id = 32, cardColor = (CardColor)(4), cardNum = (CardNum)(1) },
                new Card { id = 33, cardColor = (CardColor)(4), cardNum = (CardNum)(1) },
                new Card { id = 34, cardColor = (CardColor)(4), cardNum = (CardNum)(1) },
                new Card { id = 35, cardColor = (CardColor)(4), cardNum = (CardNum)(1) },
                new Card { id = 36, cardColor = (CardColor)(4), cardNum = (CardNum)(1) },
                // 彩色+1
                new Card { id = 37, cardColor = (CardColor)(5), cardNum = (CardNum)(1) },
                new Card { id = 38, cardColor = (CardColor)(5), cardNum = (CardNum)(1) },
                new Card { id = 39, cardColor = (CardColor)(5), cardNum = (CardNum)(1) },
                new Card { id = 40, cardColor = (CardColor)(5), cardNum = (CardNum)(1) },
                new Card { id = 41, cardColor = (CardColor)(5), cardNum = (CardNum)(1) },
                // 五色+2
                new Card { id = 42, cardColor = (CardColor)(0), cardNum = (CardNum)(2) },
                new Card { id = 43, cardColor = (CardColor)(1), cardNum = (CardNum)(2) },
                new Card { id = 44, cardColor = (CardColor)(2), cardNum = (CardNum)(2) },
                new Card { id = 45, cardColor = (CardColor)(3), cardNum = (CardNum)(2) },
                new Card { id = 46, cardColor = (CardColor)(4), cardNum = (CardNum)(2) },
                // 最慢+1
                new Card { id = 47, cardColor = (CardColor)(6), cardNum = (CardNum)(1) },
                new Card { id = 48, cardColor = (CardColor)(6), cardNum = (CardNum)(1) },
                new Card { id = 49, cardColor = (CardColor)(6), cardNum = (CardNum)(1) },
                // 最慢+2
                new Card { id = 50, cardColor = (CardColor)(6), cardNum = (CardNum)(2) },
                new Card { id = 51, cardColor = (CardColor)(6), cardNum = (CardNum)(2) },
            };
        }
        public CardLib Clone()
        {
            return new CardLib();
        }
        public List<Card> library { get; private set; }
    }

    /* 结算 
     * 一人到达即结束
     * 同时到达，越上面的排名靠前 */

    // 5身份牌，5棋子
    public enum ChessColor
    {
        NONE    = -1, //未指定的
        RED     = 0,
        YELLOW  = 1,
        GREEN   = 2,
        BLUE    = 3,
        PURPLE  = 4,
        COUNT   = 5,
    }
    // 卡牌颜色
    public enum CardColor
    {
        NONE    = -1, //未指定的
        RED     = 0,
        YELLOW  = 1,
        GREEN   = 2,
        BLUE    = 3,
        PURPLE  = 4,
        COLOR   = 5,
        SLOWEST = 6,
    }
    // 卡牌点数
    public enum CardNum
    {
        NegtiveOne = -1, //-1
        PositiveOne = 1, //+1
        PositiveTwo = 2, //+2
    }

    [System.Serializable]
    public class Card
    {
        public int id; //索引号0-51
        public CardColor cardColor;
        public CardNum cardNum;
    }
}