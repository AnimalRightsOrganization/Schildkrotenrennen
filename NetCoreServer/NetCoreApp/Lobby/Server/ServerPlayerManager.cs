using System.Collections.Generic;

namespace NetCoreServer
{
    public class ServerPlayerManager
    {
        protected List<ServerPlayer> playerList;
        public int Count => playerList.Count;

        public ServerPlayerManager()
        {
            playerList = new List<ServerPlayer>();
        }

        // 登录成功
        public void AddPlayer(ServerPlayer player)
        {
            playerList.Add(player);
            player.ResetToLobby();
        }
        // 登出/断线/踢人
        public void RemovePlayer(System.Guid peerId)
        {
            var player = playerList.Find(x => x.PeerId == peerId);
            if (player == null) return;
            playerList.Remove(player);
        }
        // 关服
        public void RemoveAll()
        {
            for (int i = playerList.Count - 1; i >= 0; i--)
            {
                playerList[i] = null;
                playerList.RemoveAt(i);
            }
        }

        // 获取指定玩家
        public ServerPlayer GetPlayerByPeerId(System.Guid peerId)
        {
            var player = playerList.Find(x => x.PeerId == peerId);
            return player;
        }
        // 获取所有玩家
        public ServerPlayer[] GetPlayersAll()
        {
            return playerList.ToArray();
        }
        // 获取大厅内玩家
        public ServerPlayer[] GetPlayersByLobby()
        {
            return playerList.FindAll(x => x.Status == PlayerStatus.AtLobby).ToArray();
        }
    }
}