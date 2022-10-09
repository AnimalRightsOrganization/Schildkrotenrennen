using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using NetCoreServer;
using NetCoreServer.Utils;
using HotFix;
using ET;

namespace TcpChatServer
{
    /* 一个客户端连接单位 */
    public class ChatSession : TcpSession
    {
        public ChatSession(TcpServer server) : base(server) { }

        protected override void OnConnected()
        {
            Debug.Print($"Chat TCP session with Id {Id} connected!");

            // 这里是异步线程中。
            EmptyPacket cmd = new EmptyPacket();
            SendAsync(PacketType.Connected, cmd); //TODO: 多个客户端，验证这是否为广播
        }

        protected override void OnDisconnected()
        {
            Debug.Print($"Chat TCP session with Id {Id} disconnected!");

            ServerPlayer player = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            if (player == null)
            {
                Debug.Print("找不到用户了");
                return;
            }
            if (player.RoomId != 0)
            {
                TCPChatServer.m_RoomManager.RemovePlayer(player); //如果是房主
            }

            TCPChatServer.m_PlayerManager.RemovePlayer(Id);

            WinFormsApp1.MainForm.Instance.RefreshPlayerNum();
        }

        // 注意这里是线程中
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            // 解析msgId
            byte msgId = buffer[0];
            byte[] body = new byte[size - 1];
            try
            {
                Array.Copy(buffer, 1, body, 0, size - 1);
            }
            catch (Exception e)
            {
                throw e;
            }
            /* 
            UnhandledException Message: Source array was not long enough. Check the source index, length, and the array's lower bounds. (Parameter 'sourceArray')
            Trace:    at System.Array.Copy(Array sourceArray, Int32 sourceIndex, Array destinationArray, Int32 destinationIndex, Int32 length, Boolean reliable)
                at System.Array.Copy(Array sourceArray, Int64 sourceIndex, Array destinationArray, Int64 destinationIndex, Int64 length)
                at TcpChatServer.ChatSession.OnReceived(Byte[] buffer, Int64 offset, Int64 size) in D:\Documents\GitHub\Turtle\NetCoreServer\NetCoreApp\TCP\TcpChatServer.cs:line 56
                at NetCoreServer.TcpSession.ProcessReceive(SocketAsyncEventArgs e) in D:\Documents\GitHub\Turtle\NetCoreServer\NetCoreApp\TCP\TcpSession.cs:line 572
                at NetCoreServer.TcpSession.TryReceive() in D:\Documents\GitHub\Turtle\NetCoreServer\NetCoreApp\TCP\TcpSession.cs:line 441
                at NetCoreServer.TcpSession.OnAsyncCompleted(Object sender, SocketAsyncEventArgs e) in D:\Documents\GitHub\Turtle\NetCoreServer\NetCoreApp\TCP\TcpSession.cs:line 542
                at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
            --- End of stack trace from previous location where exception was thrown ---
                at System.Threading._IOCompletionCallback.PerformIOCompletionCallback(UInt32 errorCode, UInt32 numBytes, NativeOverlapped* pNativeOverlapped)
            Runtime terminating: True
             */
            MemoryStream ms = new MemoryStream(body, 0, body.Length);
            PacketType type = (PacketType)msgId;
            Debug.Print($"msgType={type}, from {Id}");

            switch (type)
            {
                case PacketType.Connected:
                    break;
                case PacketType.Disconnect:
                    break;
                case PacketType.C2S_LoginToken:
                    OnLoginByToken(ms);
                    break;
                case PacketType.C2S_LoginReq:
                    OnLoginReq(ms);
                    break;
                case PacketType.C2S_RegisterReq:
                    OnSignUpReq(ms);
                    break;
                case PacketType.C2S_Chat:
                    OnChat(ms);
                    break;
                case PacketType.C2S_RoomList:
                    OnGetRoomList(ms);
                    break;
                case PacketType.C2S_CreateRoom:
                    OnCreateRoom(ms);
                    break;
                case PacketType.C2S_JoinRoom:
                    OnJoinRoom(ms);
                    break;
                case PacketType.C2S_LeaveRoom:
                    OnLeaveRoom();
                    break;
                case PacketType.C2S_OperateSeat:
                    OnOperateSeat(ms);
                    break;
                case PacketType.C2S_GameStart:
                    OnGameStart();
                    break;
                case PacketType.C2S_GamePlay:
                    OnGamePlay(ms);
                    break;
                default:
                    Debug.Print($"无法识别的消息: {type}");
                    break;
            }
        }

        protected override void OnError(SocketError error)
        {
            Debug.Print($"Chat TCP session caught an error with code {error}");
        }

        protected void SendAsync(PacketType msgId, object cmd)
        {
            byte[] header = new byte[1] { (byte)msgId };
            byte[] body = ProtobufHelper.ToBytes(cmd);
            byte[] buffer = new byte[header.Length + body.Length];
            System.Array.Copy(header, 0, buffer, 0, header.Length);
            System.Array.Copy(body, 0, buffer, header.Length, body.Length);
            //Debug.Print($"header:{header.Length},body:{body.Length},buffer:{buffer.Length},");
            SendAsync(buffer);
        }

        protected async void OnLoginByToken(MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_LoginTokenPacket>(ms); //解包
            if (request == null)
            {
                Debug.Print("Token.空数据");
                return;
            }
            Debug.Print($"[C2S] Token={request.Token} by {Id}");

            UserInfo result = await MySQLTool.GetUserInfo(request.Token);
            if (result == null)
            {
                Debug.Print($"用户名或密码错误");
                var tempData = new BasePlayerData { PeerId = Id, };
                var tempPlayer = new ServerPlayer(tempData);
                ErrorPacket err_packet = new ErrorPacket { Code = (int)ErrorCode.LOGIN_FAILED, Message = "用户名或密码错误" };
                tempPlayer.SendAsync(PacketType.S2C_ErrorOperate, err_packet);
                return;
            }
            var playerData = new BasePlayerData
            {
                IsBot = false,
                PeerId = Id,
                UserName = result.username,
                NickName = result.nickname,
                RoomId = -1,
                SeatId = -1,
                Status = PlayerStatus.LOBBY,
            };
            var serverPlayer = new ServerPlayer(playerData);
            TCPChatServer.m_PlayerManager.AddPlayer(serverPlayer);

            var packet = new S2C_LoginResultPacket { Code = 0, Username = result.username, Nickname = result.nickname };
            serverPlayer.SendAsync(PacketType.S2C_LoginResult, packet);

            WinFormsApp1.MainForm.Instance.RefreshPlayerNum();
        }
        protected async void OnLoginReq(MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_LoginPacket>(ms); //解包
            if (request == null)
            {
                Debug.Print("OnLoginReq.空数据");
                return;
            }
            Debug.Print($"[C2S] Username={request.Username}, Password={request.Password} by {Id}");

            UserInfo result = await MySQLTool.GetUserInfo(request.Username, request.Password);
            if (result == null)
            {
                Debug.Print($"用户名或密码错误");
                var tempData = new BasePlayerData { PeerId = Id, };
                var tempPlayer = new ServerPlayer(tempData);
                ErrorPacket err_packet = new ErrorPacket { Code = (int)ErrorCode.LOGIN_FAILED, Message = "用户名或密码错误" };
                tempPlayer.SendAsync(PacketType.S2C_ErrorOperate, err_packet);
                return;
            }
            var playerData = new BasePlayerData
            {
                IsBot = false,
                PeerId = Id,
                UserName = request.Username,
                NickName = result.nickname,
                RoomId = -1,
                SeatId = -1,
                Status = PlayerStatus.LOBBY,
            };
            var serverPlayer = new ServerPlayer(playerData);
            TCPChatServer.m_PlayerManager.AddPlayer(serverPlayer);

            var packet = new S2C_LoginResultPacket { Code = 0, Username = request.Username, Nickname = result.nickname };
            serverPlayer.SendAsync(PacketType.S2C_LoginResult, packet);

            WinFormsApp1.MainForm.Instance.RefreshPlayerNum();
        }
        protected async void OnSignUpReq(MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_LoginPacket>(ms); //解包
            if (request == null)
            {
                Debug.Print("OnSignUpReq.空数据");
                return;
            }

            Debug.Print($"[C2S] 请求注册:{request.Username}");

            bool result = await MySQLTool.SignUp(request.Username, request.Password);
            Debug.Print($"注册结果: {result}");

            var playerData = new BasePlayerData
            {
                IsBot = false,
                PeerId = Id,
                UserName = request.Username,
                NickName = request.Username,
                RoomId = -1,
                SeatId = -1,
                Status = PlayerStatus.OFFLINE,
            };
            var serverPlayer = new ServerPlayer(playerData);
            if (result)
            {
                var packet = new S2C_LoginResultPacket { Code = 0, Username = request.Username, Nickname = request.Username };
                serverPlayer.SendAsync(PacketType.S2C_LoginResult, packet);
            }
            else
            {
                var err_packet = new ErrorPacket { Code = (int)ErrorCode.USERNAME_USED, Message = "用户名已被注册" };
                serverPlayer.SendAsync(PacketType.S2C_ErrorOperate, err_packet);
            }
        }
        protected void OnChat(MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<TheMsg>(ms);
            if (request == null)
            {
                Debug.Print("OnChat.空数据");
                return;
            }
            Debug.Print($"{request.Name}说: {request.Content}");
        }
        protected void OnGetRoomList(MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_RoomListPacket>(ms);
            if (request == null)
            {
                Debug.Print("OnGetRoomList.空数据");
                return;
            }
            Debug.Print($"[C2S_GetRoomList] Page={request.Page} by {Id}");

            // 空消息，不用解析
            S2C_GetRoomList packet = new S2C_GetRoomList();
            var listAll = TCPChatServer.m_RoomManager.Sort();
            if (listAll.Count > 0)
            {
                // 获取指定区间
                var listRange = GetPage(listAll, request.Page);
                for (int i = 0; i < listRange.Count; i++)
                {
                    var serverRoom = listRange[i];
                    var roomInfo = serverRoom.GetRoomInfo(); //构建房间列表
                    roomInfo.Pwd = string.Empty;
                    packet.Rooms.Add(roomInfo);
                }
                packet.Page = listRange.Count == 0 ? 0 : request.Page; //0代表不存在的页码
            }

            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            p.SendAsync(PacketType.S2C_RoomList, packet);
        }
        private const int EachCount = 10;
        private List<ServerRoom> GetPage(List<ServerRoom> DataSource, int PageIndex)
        {
            //Skip()跳过多少条
            //Take()查询多少条
            return DataSource.Skip((PageIndex - 1) * EachCount).Take(EachCount).ToList();
        }
        protected void OnCreateRoom(MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_CreateRoomPacket>(ms);
            if (request == null)
            {
                Debug.Print("OnCreateRoom.空数据");
                return;
            }
            Debug.Print($"[C2S] Name={request.RoomName}, Pwd={request.RoomPwd}, playerNum={request.LimitNum} by {Id}");
            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);

            // 验证合法性（总数是否超过等），在服务器创建房间
            var roomData = new BaseRoomData
            {
                RoomID = -1, //通过Manager创建时会生成。
                RoomName = request.RoomName,
                RoomPwd = request.RoomPwd,
                RoomLimit = request.LimitNum,
                Players = new List<BasePlayerData>()
                {
                    new BasePlayerData { PeerId = p.PeerId, UserName = p.UserName, NickName = p.NickName, SeatId = p.SeatId },
                },
            };
            var serverRoom = TCPChatServer.m_RoomManager.CreateServerRoom(p, roomData);
            if (serverRoom == null)
            {
                Debug.Print("大厅爆满，无法创建");
                ErrorPacket err_packet = new ErrorPacket { Code = (int)ErrorCode.LOBBY_IS_FULL, Message = "大厅爆满" };
                p.SendAsync(PacketType.S2C_ErrorOperate, err_packet);
                return;
            }

            var roomInfo = serverRoom.GetRoomInfo(); //创建房间
            S2C_RoomInfo packet = new S2C_RoomInfo { Room = roomInfo };
            Debug.Print($"[S2C_RoomInfo] {roomInfo.ToString()}");
            p.SendAsync(PacketType.S2C_RoomInfo, packet);

            WinFormsApp1.MainForm.Instance.RefreshRoomNum();
        }
        protected void OnJoinRoom(MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_JoinRoomPacket>(ms);
            if (request == null)
            {
                Debug.Print("OnJoinRoom.空数据");
                return;
            }
            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            Debug.Print($"{p.UserName}请求加入房间#{request.RoomID}");

            // 验证合法性（座位是否够，房间状态是否在等待，密码，等）
            ServerRoom serverRoom = TCPChatServer.m_RoomManager.GetServerRoom(request.RoomID);
            if (serverRoom.RoomPwd != request.RoomPwd)
            {
                ErrorPacket err_packet = new ErrorPacket { Code = (int)ErrorCode.ROOM_PWD_ERR, Message = "房间密码错误" };
                p.SendAsync(PacketType.S2C_ErrorOperate, err_packet);
                return;
            }
            if (serverRoom.m_PlayerDic.Count >= serverRoom.RoomLimit)
            {
                ErrorPacket err_packet = new ErrorPacket { Code = (int)ErrorCode.ROOM_IS_FULL, Message = "房间爆满" };
                p.SendAsync(PacketType.S2C_ErrorOperate, err_packet);
                return;
            }
            serverRoom.AddPlayer(p);
            Debug.Print($"允许加入，服务器修改{p.UserName}状态，#{p.RoomId},#{p.SeatId},状态:{p.Status}");

            Debug.Print(serverRoom.ToString());
            var roomInfo = serverRoom.GetRoomInfo(); //加入房间
            S2C_RoomInfo packet = new S2C_RoomInfo { Room = roomInfo };
            serverRoom.SendAsync(PacketType.S2C_RoomInfo, packet);
        }
        protected void OnLeaveRoom()
        {
            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            Debug.Print($"{p.UserName}当前在房间#{p.RoomId}，座位#{p.SeatId}，请求离开");

            // 验证合法性
            var serverRoom = TCPChatServer.m_RoomManager.GetServerRoom(p.RoomId);
            var roomData = serverRoom.m_Data;
            bool verify = serverRoom.ContainsPlayer(p);
            if (verify == false)
            {
                Debug.Print($"错误，用户不在该房间#{p.RoomId}");
                return;
            }
            if (serverRoom.hostPlayer.Status == PlayerStatus.GAME)
            {
                Debug.Print("比赛中投降，结算时判负");
                //接下来比赛会跳过该用户回合
                //判断剩下几人，如果只剩一个真实玩家，直接弹出结算。
                return;
            }
            Debug.Print($"验证成功，可以离开房间#{p.RoomId}，座位#{p.SeatId}"); //-1, -1
            if (p.SeatId == 0)
            {
                // 如果是房主，解散房间
                TCPChatServer.m_RoomManager.RemoveServerRoom(roomData.RoomID);
                Debug.Print("是房主，解散房间");
            }
            else
            {
                serverRoom.RemovePlayer(p);

                var packet1 = new S2C_LeaveRoomPacket
                { 
                    RoomID = roomData.RoomID, 
                    RoomName = roomData.RoomName, 
                    LeaveBy = (int)LeaveRoomType.SELF,
                };
                p.SendAsync(PacketType.S2C_LeaveRoom, packet1); //主动离开
                Debug.Print($"[S2C] 单发给{p.UserName}，离开房间");

                // 房间内其他人员广播，更新房间信息
                var roomInfo = serverRoom.GetRoomInfo(); //离开房间
                var packet2 = new S2C_RoomInfo { Room = roomInfo };
                serverRoom.SendAsync(PacketType.S2C_RoomInfo, packet2);
                Debug.Print("[S2C] 广播给房间内剩余人员，更新房间信息");
            }

            WinFormsApp1.MainForm.Instance.RefreshRoomNum();
        }
        protected void OnOperateSeat(MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_OperateSeatPacket>(ms);
            if (request == null)
            {
                Debug.Print("OnOperateSeat.空数据");
                return;
            }
            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            ServerRoom serverRoom = TCPChatServer.m_RoomManager.GetServerRoom(p.RoomId);
            BaseRoomData roomData = serverRoom.m_Data;
            Debug.Print($"[C2S] {p.UserName}在房间#{p.RoomId}对座位#{request.SeatID}添加操作：{(SeatOperate)request.Operate}");

            // 都要房主权限
            if (p.SeatId != 0)
            {
                Debug.Print($"只有房主可以操作");
                return;
            }
            // 校验操作合法性，操作修改房间信息
            switch ((SeatOperate)request.Operate)
            {
                case SeatOperate.ADD_BOT:
                    {
                        bool available = serverRoom.IsAvailableSeat(request.SeatID);
                        if (available == false)
                        {
                            Debug.Print($"无法对座位#{request.SeatID}，操作：{(SeatOperate)request.Operate}");
                            return;
                        }
                        // 创建机器人
                        BasePlayerData playerData = new BasePlayerData
                        {
                            IsBot = true,
                            UserName = "_BOT_",
                            NickName = "_BOT_",
                            RoomId = roomData.RoomID,
                            SeatId = request.SeatID,
                            Status = PlayerStatus.ROOM,
                        };
                        ServerPlayer robot = new ServerPlayer(playerData);
                        TCPChatServer.m_PlayerManager.AddPlayer(robot);
                        serverRoom.AddPlayer(robot, request.SeatID);
                        break;
                    }
                case SeatOperate.KICK_PLAYER:
                    {
                        var targetPlayer = serverRoom.GetPlayer(request.SeatID);
                        if (targetPlayer == null)
                        {
                            Debug.Print($"座位上没人");
                            return;
                        }
                        serverRoom.RemovePlayer(targetPlayer);
                        var packet = new S2C_LeaveRoomPacket { RoomID = roomData.RoomID, RoomName = roomData.RoomName, LeaveBy = (int)LeaveRoomType.KICK };
                        targetPlayer.SendAsync(PacketType.S2C_LeaveRoom, packet); //被房主移除
                        break;
                    }
            }

            var roomInfo = serverRoom.GetRoomInfo(); //房主操作客位
            var packet1 = new S2C_RoomInfo { Room = roomInfo };
            Debug.Print($"[S2C] {roomInfo.ToString()}");
            // 房间内广播，更新房间信息
            serverRoom.SendAsync(PacketType.S2C_RoomInfo, packet1);
        }
        protected void OnGameStart()
        {
            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            ServerRoom serverRoom = TCPChatServer.m_RoomManager.GetServerRoom(p.RoomId);
            Debug.Print($"{p.UserName}请求开始比赛，房间#{p.RoomId}({serverRoom.m_PlayerDic.Count}/{serverRoom.RoomLimit})");

            // 校验房间人数。
            if (serverRoom.m_PlayerDic.Count < serverRoom.RoomLimit)
            {
                Debug.Print("ERROR: 房间未满员");
                //ErrorPacket response = new ErrorPacket { Code = 0, Message = "ERROR: 房间未满员" };
                //p.SendAsync(PacketType.S2C_ErrorOperate, response);
                return;
            }

            serverRoom.OnGameStart_Server();
        }
        // 收到消息（只对真人）
        protected void OnGamePlay(MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_PlayCardPacket>(ms);
            if (request == null)
            {
                Debug.Print("OnGamePlay.空数据");
                return;
            }
            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            OnGamePlay(request, p);
        }
        protected async void OnGamePlay(C2S_PlayCardPacket request, ServerPlayer p)
        {
            Debug.Print($"[C2S] {p.UserName}，在房间#{p.RoomId}，座位#{p.SeatId}，出牌：{request.CardID}/{request.Color}");

            ServerRoom serverRoom = TCPChatServer.m_RoomManager.GetServerRoom(p.RoomId);
            if (serverRoom == null)
            {
                Debug.Print($"[Error] 房间已经解散");
                return;
            }

            if (p.SeatId != serverRoom.nextPlayerIndex)
            {
                Debug.Print($"顺序错误，不允许座位#{p.SeatId}出牌，等待座位#{serverRoom.nextPlayerIndex}");
            }
            bool end = serverRoom.OnGamePlay_Server(p, request);

            // 房间内广播出牌结果
            var packet1 = new S2C_PlayCardPacket
            {
                CardID = request.CardID,
                Color = request.Color,
                SeatID = p.SeatId,
            };
            serverRoom.SendAsync(PacketType.S2C_GamePlay, packet1);
            Debug.Print($"[S2C] 广播出牌消息：座位#{packet1.SeatID}出{packet1.CardID}");

            if (end)
            {
                Debug.Print("到达终点，不再发牌");
                OnGameResult();
                return;
            }

            // 给出牌者发送新发的牌
            Card card = serverRoom.OnGameDeal_Server(p);
            var packet2 = new S2C_DealPacket { CardID = card.id, SeatID = p.SeatId };
            p.SendAsync(PacketType.S2C_GameDeal, packet2);
            Debug.Print($"[S2C] 单发发牌消息：{packet2.CardID}给座位#{packet2.SeatID}");

            // 广播下轮出牌人
            int nextSeatId = serverRoom.nextPlayerIndex;
            var packet3 = new S2C_NextTurnPacket { SeatID = nextSeatId };
            serverRoom.SendAsync(PacketType.S2C_YourTurn, packet3);
            Debug.Print($"[S2C] 广播下轮出牌人，座位#{nextSeatId}");

            ServerPlayer nextPlayer = serverRoom.GetPlayer(nextSeatId);
            if (nextPlayer.IsBot)
            {
                // 下个是机器人，计算后发出牌消息
                Debug.Print($"下个出牌的是机器人，等待五秒（动画时间）---{System.DateTime.Now.ToString("HH:mm:ss")}");
                await Task.Delay(5000);
                Debug.Print($"Done---{System.DateTime.Now.ToString("HH:mm:ss")}");
                var bot_request = nextPlayer.Bot_PlayCardPacket();
                OnGamePlay(bot_request, nextPlayer);
            }
        }
        public void OnGameResult()
        {
            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            ServerRoom serverRoom = TCPChatServer.m_RoomManager.GetServerRoom(p.RoomId);
            List<int> turtleRank = serverRoom.OnGameResult(); //五只龟的顺序

            List<int> playerRank = new List<int>();
            Debug.Print($"结算：人数={serverRoom.m_PlayerDic.Count}");
            for (int i = 0; i < serverRoom.m_PlayerDic.Count; i++)
            {
                var serverPlayer = serverRoom.GetPlayer(i);
                int color = (int)serverPlayer.chessColor;
                int rank = turtleRank.IndexOf(color);
                playerRank.Add(rank);
            }

            var packet = new S2C_GameResultPacket { Rank = playerRank }; //key:座位号, value:排名
            serverRoom.SendAsync(PacketType.S2C_GameResult, packet);
            TCPChatServer.m_RoomManager.RemoveServerRoom(serverRoom.RoomID);

            // 打印
            string rankStr = string.Empty;
            for (int i = 0; i < playerRank.Count; i++)
            {
                rankStr += $"座位{i}排名{playerRank[i]}颜色；";
            }
            Debug.Print($"广播结算消息[{playerRank.Count}]：{rankStr}");
        }
    }

    public class ChatServer : TcpServer
    {
        public ChatServer(IPAddress address, int port) : base(address, port) {}

        protected override TcpSession CreateSession() { return new ChatSession(this); }

        protected override void OnError(SocketError error)
        {
            Debug.Print($"Chat TCP server caught an error with code {error}");
        }
    }

    public class TCPChatServer
    {
        protected const int port = 1111;
        public static ChatServer server;

        public static ServerRoomManager m_RoomManager;
        public static ServerPlayerManager m_PlayerManager;

        public static void Run()
        {
            if (server != null && server.IsStarted)
            {
                Debug.Print("Server is Started!");
                return;
            }

            m_RoomManager = new ServerRoomManager();
            m_PlayerManager = new ServerPlayerManager();
            Debug.Print("TCPChatServer Init...");

            // Create a new TCP chat server
            server = new ChatServer(IPAddress.Any, port);

            // Start the server
            Debug.Print("Server starting...");
            server.Start();
            Debug.Print("Done!");
        }
        public static void Stop()
        {
            if (server == null || server.IsStarted == false)
            {
                Debug.Print("No server!");
                return;
            }

            // Stop the server
            Debug.Print("Server stopping...");
            server?.Stop();
            //server?.Dispose();
            //server = null;
            Debug.Print("Done!");
        }
        public static void Restart()
        {
            // Restart the server
            Debug.Print("Server restarting...");
            server.Restart();
            Debug.Print("Done!");
        }
    }
}