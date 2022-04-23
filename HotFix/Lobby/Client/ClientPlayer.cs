namespace HotFix
{
    /* 本地用户 */
    [System.Serializable]
    public class ClientPlayer : BasePlayer
    {
        //public ClientPlayer(string name, System.Guid peerId, bool bot = false) : base(name, peerId, bot) { }
        public ClientPlayer(BasePlayerData data) : base(data) { }
    }
}