namespace HotFix
{
    /* �����û� */
    [System.Serializable]
    public class ClientPlayer : BasePlayer
    {
        public ClientPlayer(string name, System.Guid peerId) : base(name, peerId) { }
    }
}