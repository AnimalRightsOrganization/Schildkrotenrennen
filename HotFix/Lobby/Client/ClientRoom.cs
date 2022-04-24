using System.Collections.Generic;

namespace HotFix
{
    public class ClientRoom : BaseRoom
    {
        public ClientRoom(BaseRoomData data) : base(data)
        {
            m_PlayerList = new Dictionary<int, BasePlayer>();
            for (int i = 0; i < data.Players.Count; i++)
            {
                var playerData = data.Players[i];
                var clientPlayer = new ClientPlayer(playerData);
                m_PlayerList.Add(playerData.SeatId, clientPlayer);
            }

            // 初始化
            lib = new CardLib();
            chessColor = ChessColor.NONE; //空，等待指定
            handCards = new List<Card>(); //空，等待发牌
            runnerPos = new Dictionary<ChessColor, int>();
            runnerPos.Add(ChessColor.RED, 0);
            runnerPos.Add(ChessColor.YELLOW, 0);
            runnerPos.Add(ChessColor.GREEN, 0);
            runnerPos.Add(ChessColor.BLUE, 0);
            runnerPos.Add(ChessColor.PURPLE, 0);
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
        // 保存自己的颜色和手牌
        public ChessColor chessColor;
        public List<Card> handCards; //长度永远是5
        // 保存乌龟棋子位置
        public Dictionary<ChessColor, int> runnerPos; //长度永远是5
    }
}