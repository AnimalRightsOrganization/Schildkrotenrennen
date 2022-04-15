using System.Collections.Generic;

public class BaseRoom
{
    public byte RoomID;
    //public int Seed;
    public byte MaxPlayer; //几人房
    public BasePlayer[] Players;
}
public class ClientRoom : BaseRoom
{
    public ClientRoom() { }
}
public class ServerRoom : BaseRoom
{
    public ServerRoom() { }
}