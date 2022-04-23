namespace HotFix
{
    /* 本地用户 */
    [System.Serializable]
    public class ClientPlayer : BasePlayer
    {
        public ClientPlayer(string name, System.Guid peerId) : base(name, peerId) { }
    }
}