namespace HotFix
{
    /* 本地用户 */
    [System.Serializable]
    public class ClientPlayer : BasePlayer
    {
        public ClientPlayer(BasePlayerData data) : base(data) { }

        public override string ToString()
        {
            return $"[LocalPlayer] {UserName}({NickName}): #{RoomId},#{SeatId},状态:{Status}";
        }
    }
}