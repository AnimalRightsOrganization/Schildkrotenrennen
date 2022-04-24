using System.Collections.Generic;

namespace HotFix
{
    // 牌库（52张）
    /*
     * 5色-1，5*2=10
     * 彩色-1，2
     * 5色+1，5*5=25
     * 彩色+1，5
     * 5色+2，5
     * 最慢+1，3
     * 最慢+2，2
     * */
    public class CardLibrary
    {
        public List<CardAttribute> libraryList;

        public CardLibrary()
        {
            libraryList = new List<CardAttribute>()
        {
            // 五色、彩色-1
            new CardAttribute { id = 0,  cardColor = (CardColor)(0), cardNum = (CardNum)(-1) },
            new CardAttribute { id = 1,  cardColor = (CardColor)(0), cardNum = (CardNum)(-1) },
            new CardAttribute { id = 2,  cardColor = (CardColor)(1), cardNum = (CardNum)(-1) },
            new CardAttribute { id = 3,  cardColor = (CardColor)(1), cardNum = (CardNum)(-1) },
            new CardAttribute { id = 4,  cardColor = (CardColor)(2), cardNum = (CardNum)(-1) },
            new CardAttribute { id = 5,  cardColor = (CardColor)(2), cardNum = (CardNum)(-1) },
            new CardAttribute { id = 6,  cardColor = (CardColor)(3), cardNum = (CardNum)(-1) },
            new CardAttribute { id = 7,  cardColor = (CardColor)(3), cardNum = (CardNum)(-1) },
            new CardAttribute { id = 8,  cardColor = (CardColor)(4), cardNum = (CardNum)(-1) },
            new CardAttribute { id = 9,  cardColor = (CardColor)(4), cardNum = (CardNum)(-1) },
            new CardAttribute { id = 10, cardColor = (CardColor)(5), cardNum = (CardNum)(-1) },
            new CardAttribute { id = 11, cardColor = (CardColor)(5), cardNum = (CardNum)(-1) },
            // 红色+1
            new CardAttribute { id = 12, cardColor = (CardColor)(0), cardNum = (CardNum)(1) },
            new CardAttribute { id = 13, cardColor = (CardColor)(0), cardNum = (CardNum)(1) },
            new CardAttribute { id = 14, cardColor = (CardColor)(0), cardNum = (CardNum)(1) },
            new CardAttribute { id = 15, cardColor = (CardColor)(0), cardNum = (CardNum)(1) },
            new CardAttribute { id = 16, cardColor = (CardColor)(0), cardNum = (CardNum)(1) },
            // 黄色+1
            new CardAttribute { id = 17, cardColor = (CardColor)(1), cardNum = (CardNum)(1) },
            new CardAttribute { id = 18, cardColor = (CardColor)(1), cardNum = (CardNum)(1) },
            new CardAttribute { id = 19, cardColor = (CardColor)(1), cardNum = (CardNum)(1) },
            new CardAttribute { id = 20, cardColor = (CardColor)(1), cardNum = (CardNum)(1) },
            new CardAttribute { id = 21, cardColor = (CardColor)(1), cardNum = (CardNum)(1) },
            // 绿色+1
            new CardAttribute { id = 22, cardColor = (CardColor)(2), cardNum = (CardNum)(1) },
            new CardAttribute { id = 23, cardColor = (CardColor)(2), cardNum = (CardNum)(1) },
            new CardAttribute { id = 24, cardColor = (CardColor)(2), cardNum = (CardNum)(1) },
            new CardAttribute { id = 25, cardColor = (CardColor)(2), cardNum = (CardNum)(1) },
            new CardAttribute { id = 26, cardColor = (CardColor)(2), cardNum = (CardNum)(1) },
            // 蓝色+1
            new CardAttribute { id = 27, cardColor = (CardColor)(3), cardNum = (CardNum)(1) },
            new CardAttribute { id = 28, cardColor = (CardColor)(3), cardNum = (CardNum)(1) },
            new CardAttribute { id = 29, cardColor = (CardColor)(3), cardNum = (CardNum)(1) },
            new CardAttribute { id = 30, cardColor = (CardColor)(3), cardNum = (CardNum)(1) },
            new CardAttribute { id = 31, cardColor = (CardColor)(3), cardNum = (CardNum)(1) },
            // 紫色+1
            new CardAttribute { id = 32, cardColor = (CardColor)(4), cardNum = (CardNum)(1) },
            new CardAttribute { id = 33, cardColor = (CardColor)(4), cardNum = (CardNum)(1) },
            new CardAttribute { id = 34, cardColor = (CardColor)(4), cardNum = (CardNum)(1) },
            new CardAttribute { id = 35, cardColor = (CardColor)(4), cardNum = (CardNum)(1) },
            new CardAttribute { id = 36, cardColor = (CardColor)(4), cardNum = (CardNum)(1) },
            // 彩色+1
            new CardAttribute { id = 37, cardColor = (CardColor)(5), cardNum = (CardNum)(1) },
            new CardAttribute { id = 38, cardColor = (CardColor)(5), cardNum = (CardNum)(1) },
            new CardAttribute { id = 39, cardColor = (CardColor)(5), cardNum = (CardNum)(1) },
            new CardAttribute { id = 40, cardColor = (CardColor)(5), cardNum = (CardNum)(1) },
            new CardAttribute { id = 41, cardColor = (CardColor)(5), cardNum = (CardNum)(1) },
            // 五色+2
            new CardAttribute { id = 42, cardColor = (CardColor)(0), cardNum = (CardNum)(2) },
            new CardAttribute { id = 43, cardColor = (CardColor)(1), cardNum = (CardNum)(2) },
            new CardAttribute { id = 44, cardColor = (CardColor)(2), cardNum = (CardNum)(2) },
            new CardAttribute { id = 45, cardColor = (CardColor)(3), cardNum = (CardNum)(2) },
            new CardAttribute { id = 46, cardColor = (CardColor)(4), cardNum = (CardNum)(2) },
            // 最慢+1
            new CardAttribute { id = 47, cardColor = (CardColor)(6), cardNum = (CardNum)(1) },
            new CardAttribute { id = 48, cardColor = (CardColor)(6), cardNum = (CardNum)(1) },
            new CardAttribute { id = 49, cardColor = (CardColor)(6), cardNum = (CardNum)(1) },
            // 最慢+2
            new CardAttribute { id = 50, cardColor = (CardColor)(6), cardNum = (CardNum)(2) },
            new CardAttribute { id = 51, cardColor = (CardColor)(6), cardNum = (CardNum)(2) },
        };
        }
    }
}