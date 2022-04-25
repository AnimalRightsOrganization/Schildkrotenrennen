using System.Collections.Generic;
using Debug = UnityEngine.Debug;

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
            Init();
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
        public Dictionary<ChessColor, int> chessPos; //长度永远是5
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