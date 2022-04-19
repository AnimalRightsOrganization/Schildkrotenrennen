namespace HotFix
{
    public class C2S_GameStart
    {
        public byte RoomID;
    }
    public class C2S_Play
    {
        public int PlayerID;
        public byte CardID;
    }
    public class C2S_Deal
    {
        public int PlayerID;
        public byte CardID;
    }

    public class S2C_SeatInfo
    {
        public int PlayerID;
        public byte Color;
        public byte[] Cards;
    }
    public class S2C_GameStart
    {
        public byte RoomID;
        public S2C_SeatInfo[] Seats;
    }
    public class S2C_Play
    {
        public int PlayerID;
        public byte CardID;
    }
    public class S2C_Deal
    {
        public int PlayerID;
        public byte CardID;
    }
}