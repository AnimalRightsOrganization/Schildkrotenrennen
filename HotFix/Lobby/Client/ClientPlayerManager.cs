namespace HotFix
{
    // 只管理房间内的玩家
    public class ClientPlayerManager
    {
        public ClientPlayerManager() { }

        private ClientPlayer _localPlayer; //自己
        public ClientPlayer LocalPlayer => _localPlayer;

        // 重置
        public void Reset()
        {
            _localPlayer = null;
        }

        // 增，登录成功，匹配成功，调用
        public void AddClientPlayer(ClientPlayer player, bool isSelf)
        {
            if (isSelf)
            {
                _localPlayer = player;
            }
        }
    }
}