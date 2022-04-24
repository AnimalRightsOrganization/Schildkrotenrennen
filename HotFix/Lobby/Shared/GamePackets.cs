namespace HotFix
{
    public enum ErrorCode : byte
    {
        LOBBY_IS_FULL,      //大厅爆满
        RoomIsFull,         //房间爆满
        UserNameUsed,       //账号已经注册
        BE_KICKED,          //被踢了（顶号/GM）
    }

    public enum PlayerStatus : byte
    {
        OFFLINE     = 0,    //离线
        LOBBY       = 1,    //在大厅
        ROOM        = 2,    //在房间
        GAME        = 3,    //在战场
        RECONNECT   = 4,    //异常掉线，等待重连
    }

    public enum SeatInfo : short
    {
        NONE        = -1,   //没人或不在房间
        HOST        = 0,    //主位
        GUEST       = 1,    //客位
    }

    public enum SeatOperate : short
    {
        ADD_BOT     = 0, //添加机器人（Proto3中，首成员必须是0）
        KICK_PLAYER = 1, //踢人
    }

    public enum LeaveRoomType : short
    {
        SELF        = 0, //主动离开
        KICK        = 1, //被房主移除
        DISSOLVE    = 2, //房间解散
        GAME_END    = 3, //游戏结束？
    }

    public enum PacketType : byte
    {
        Connected = 0   ,   //连接成功（本地消息）
        Disconnect      ,   //连接断开（本地消息）
        ///////////////////////////////////////////////
        C2S_RegisterReq ,   //注册请求
        C2S_LoginReq    ,   //登录请求
        C2S_LogoutReq   ,   //登出请求
        C2S_UserInfo    ,   //请求用户信息
        C2S_Chat        ,   //聊天消息
        C2S_Settings    ,   //设置选项
        
        C2S_RoomList    ,   //房间列表
        C2S_CreateRoom  ,   //创建房间
        C2S_JoinRoom    ,   //加入房间
        C2S_LeaveRoom   ,   //离开房间
        C2S_OperateSeat ,   //添加机器人，踢人
        
        C2S_GameStart   ,   //请求开始战斗（房主）
        C2S_GamePlay    ,   //出牌
        ///////////////////////////////////////////////
        S2C_ErrorOperate,   //错误代码
        S2C_LoginResult ,   //登录、注册结果
        S2C_LogoutResult,   //登出结果
        S2C_UserInfo    ,   //下发用户信息
        S2C_Chat        ,   //聊天消息广播
        S2C_Settings    ,   //设置选项
        
        S2C_RoomList    ,   //房间列表（分页）
        S2C_RoomInfo    ,   //单个房间（自己创建/加入，别人加入/离开）
        S2C_LeaveRoom   ,   //离开房间

        S2C_GameStart   ,   //比赛开始，跳转场景
        S2C_GameDeal    ,   //发牌信息
        S2C_YourTurn    ,   //通知轮到出牌的玩家
        S2C_GamePlay    ,   //出牌信息
        S2C_GameResult  ,   //比赛结算
    }
}