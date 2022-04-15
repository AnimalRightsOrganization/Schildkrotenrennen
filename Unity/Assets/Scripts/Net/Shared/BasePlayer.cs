using System.Collections.Generic;

public class BasePlayer
{
    public int PlayerID;
    public string NickName;
    public byte RoomID; //-1在大厅
    public byte SeatID;
    public byte ColorID;
}
public class ClietnPlayer : BasePlayer
{
    public ClietnPlayer() { }
}
public class ServerPlayer : BasePlayer
{
    public ServerPlayer() { }
}