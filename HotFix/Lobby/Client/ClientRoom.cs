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

            //handCards = new List<CardAttribute>();
            //runnerPos = new Dictionary<RunnerColor, int>(5);
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

        // 保存自己的颜色和手牌
        public RunnerColor runnerColor;
        public List<CardAttribute> handCards; //length=5
        // 保存乌龟棋子位置
        public Dictionary<RunnerColor, int> runnerPos; //length=5
    }
}