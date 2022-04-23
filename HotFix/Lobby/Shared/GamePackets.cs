namespace HotFix
{
    public enum ErrorCode : byte
    {
        LOBBY_IS_FULL,      //��������
        RoomIsFull,         //���䱬��
        UserNameUsed,       //�˺��Ѿ�ע��
        BE_KICKED,          //�����ˣ�����/GM��
    }

    public enum PlayerStatus : byte
    {
        Offline     = 0,    //����
        AtLobby     = 1,    //�ڴ���
        Matching    = 2,    //ƥ����
        AtRoomWait  = 3,    //�ڷ���
        AtRoomReady = 4,    //�ڷ���
        AtBattle    = 5,    //��ս��
        Reconnect   = 6,    //�쳣���ߣ��ȴ�����
    }

    public enum SeatInfo : short
    {
        NONE        = -1,   //û�˻��ڷ���
        HOST        = 0,    //��λ
        GUEST       = 1,    //��λ
    }

    public enum SeatOperate : short
    {
        ADD_BOT     = 0, //��ӻ����ˣ�Proto3�У��׳�Ա������0��
        KICK_PLAYER = 1, //����
    }

    public enum LeaveRoomType : short
    {
        SELF        = 0, //�����뿪
        KICK        = 1, //�������Ƴ�
        DISSOLVE    = 2, //�����ɢ
        GAME_END    = 3, //��Ϸ������
    }

    public enum PacketType : byte
    {
        Connected = 0   ,   //���ӳɹ���������Ϣ��
        Disconnect      ,   //���ӶϿ���������Ϣ��
        ///////////////////////////////////////////////
        C2S_RegisterReq ,   //ע������
        C2S_LoginReq    ,   //��¼����
        C2S_LogoutReq   ,   //�ǳ�����
        C2S_UserInfo    ,   //�����û���Ϣ
        C2S_Chat        ,   //������Ϣ
        C2S_Settings    ,   //����ѡ��
        
        C2S_RoomList    ,   //�����б�
        C2S_CreateRoom  ,   //��������
        C2S_JoinRoom    ,   //���뷿��
        C2S_LeaveRoom   ,   //�뿪����
        C2S_OperateSeat ,   //��ӻ����ˣ�����
        
        C2S_GameReady   ,   //����׼������Ա��
        C2S_GameStart   ,   //����ʼս����������
        C2S_GamePlay    ,   //����
        C2S_GameQuit    ,   //�뿪���������䣩 =>���ش���
        ///////////////////////////////////////////////
        S2C_ErrorOperate,   //�������
        S2C_LoginResult ,   //��¼��ע����
        S2C_LogoutResult,   //�ǳ����
        S2C_UserInfo    ,   //�·��û���Ϣ
        S2C_Chat        ,   //������Ϣ�㲥
        S2C_Settings    ,   //����ѡ��
        
        S2C_RoomList    ,   //�����б����ҳ��
        S2C_RoomInfo    ,   //�������䣨���������롢�뿪��׼�������ã�
        S2C_LeaveRoom   ,   //�뿪����

        S2C_GameReady   ,   //׼���������Ա��
        S2C_GameStart   ,   //������ʼ����ת����
        S2C_YourTurn    ,   //通知轮到出牌的玩家
        S2C_GameDeal    ,   //������Ϣ
        S2C_GamePlay    ,   //������Ϣ
        S2C_GameResult  ,   //��������
    }
}