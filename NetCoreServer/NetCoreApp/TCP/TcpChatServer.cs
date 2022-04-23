using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
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
        public ChatSession(TcpServer server) : base(server) {}

        protected override void OnConnected()
        {
            Debug.Print($"Chat TCP session with Id {Id} connected!");

            // Send invite message
            //string message = "Hello from TCP chat! Please send a message or '!' to disconnect the client!";
            //SendAsync(message);

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
            TCPChatServer.m_RoomManager.RemoveIfHost(player); //如果是房主
            TCPChatServer.m_PlayerManager.RemovePlayer(Id);

            WinFormsApp1.MainForm.Instance.RefreshPlayerNum();
        }

        // 注意这里是线程中
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            //Debug.Print($"OnReceived: buffer={buffer.Length}, offset={offset}, size={size}");

            // 解析msgId
            byte msgId = buffer[0];
            byte[] body = new byte[size - 1];
            Array.Copy(buffer, 1, body, 0, size - 1);
            MemoryStream ms = new MemoryStream(body, 0, body.Length);
            PacketType type = (PacketType)msgId;
            Debug.Print($"msgType={type}, from {Id}");

            switch (type)
            {
                case PacketType.Connected:
                    break;
                case PacketType.C2S_LoginReq:
                    OnLoginReq(ms);
                    break;
                case PacketType.C2S_RoomList:
                    OnRoomList();
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
                case PacketType.C2S_Chat:
                    OnChat(ms);
                    break;
                case PacketType.C2S_GameStart:
                    OnStartGame();
                    break;
                case PacketType.C2S_GamePlay:
                    OnPlayCard(ms);
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

        protected async void OnLoginReq(MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_LoginPacket>(ms); //解包
            Debug.Print($"Username={request.Username}, Password={request.Password} by {Id}");

            //UserInfo result = await MySQLTool.GetUserInfo(request.Username, request.Password);
            UserInfo result = await MySQLTool.GetUserInfo("test3", "123456");
            if (result == null)
            {
                Debug.Print($"用户名或密码错误");
                return;
            }
            //Debug.Print($"昵称: {result.nickname}");
            ServerPlayer p = new ServerPlayer(request.Username, Id, false);
            p.NickName = result.nickname;

            TCPChatServer.m_PlayerManager.AddPlayer(p);

            var packet = new S2C_LoginResultPacket { Code = 0, Username = request.Username, Nickname = result.nickname };
            p.SendAsync(PacketType.S2C_LoginResult, packet);

            WinFormsApp1.MainForm.Instance.RefreshPlayerNum();
        }
        protected void OnRoomList()
        {
            // 空消息，不用解析
            S2C_GetRoomList packet = new S2C_GetRoomList();
            List<PlayerInfo> players = new List<PlayerInfo>();
            foreach (ServerRoom room in TCPChatServer.m_RoomManager.GetAll())
            {
                RoomInfo info = new RoomInfo
                {
                    RoomID = room.m_RoomData.RoomID,
                    RoomName = room.m_RoomData.RoomName,
                    LimitNum = room.m_RoomData.RoomLimit,
                    Players = players,
                };
                packet.Rooms.Add(info);
            }

            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            p.SendAsync(PacketType.S2C_RoomList, packet);
        }
        protected void OnCreateRoom(MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_CreateRoomPacket>(ms); //解包
            Debug.Print($"Name={request.RoomName}, Pwd={request.RoomPwd}, playerNum={request.LimitNum} by {Id}");

            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            PlayerInfo hostPlayer = new PlayerInfo { UserName = p.UserName, NickName = p.NickName, SeatID = 0 };
            //Debug.Print($"host={hostPlayer.NickName}");

            List<PlayerInfo> players = new List<PlayerInfo>();
            players.Add(hostPlayer);
            RoomInfo roomInfo = new RoomInfo
            {
                RoomID = 0,
                RoomName = request.RoomName,
                LimitNum = request.LimitNum,
                Players = players,
            };

            // 验证合法性（总数是否超过等），在服务器创建房间
            BasePlayerData hostPlayerData = new BasePlayerData { PeerId = p.PeerId, UserName = p.UserName, NickName = p.NickName, SeatId = p.SeatId };
            BaseRoomData baseRoomData = new BaseRoomData
            {
                RoomID = -1, //通过Manager创建时会生成。
                RoomName = request.RoomName,
                RoomPwd = request.RoomPwd,
                RoomLimit = request.LimitNum,
                Players = new List<BasePlayerData>() { hostPlayerData },
            };
            ServerRoom serverRoom = TCPChatServer.m_RoomManager.CreateServerRoom(p, baseRoomData);
            if (serverRoom == null)
            {
                Debug.Print("创建房间出错");
                ErrorPacket err_packet = new ErrorPacket { Code = (int)ErrorCode.RoomIsFull, Message = "创建房间出错" };
                p.SendAsync(PacketType.S2C_ErrorOperate, err_packet);
                return;
            }

            S2C_RoomInfo packet = new S2C_RoomInfo { Room = roomInfo };
            p.SendAsync(PacketType.S2C_RoomInfo, packet);

            WinFormsApp1.MainForm.Instance.RefreshRoomNum();
        }
        protected void OnJoinRoom(MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_JoinRoomPacket>(ms); //解包
            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            Debug.Print($"{p.UserName}请求加入房间#{request.RoomID}");

            // 验证合法性（座位是否够，房间状态是否在等待，密码，等）
            ServerRoom serverRoom = TCPChatServer.m_RoomManager.GetServerRoom(request.RoomID);
            if (serverRoom.m_RoomData.RoomPwd == request.RoomPwd)
            {
                Debug.Print("房间密码错误");
                return;
            }
            if (serverRoom.m_PlayerList.Count >= serverRoom.m_RoomData.RoomLimit)
            {
                Debug.Print("房间爆满");
                return;
            }
            serverRoom.AddPlayer(p);

            RoomInfo roomInfo = new RoomInfo { RoomID = serverRoom.m_RoomData.RoomID, RoomName = serverRoom.m_RoomData.RoomName, LimitNum = serverRoom.m_RoomData.RoomLimit };
            S2C_RoomInfo packet = new S2C_RoomInfo { Room = roomInfo };
            p.SendAsync(PacketType.S2C_RoomInfo, packet);
        }
        protected void OnLeaveRoom()
        {
            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            Debug.Print($"{p.UserName}当前在房间#{p.RoomId}，座位#{p.SeatId}，请求离开");

            // 验证合法性
            ServerRoom serverRoom = TCPChatServer.m_RoomManager.GetServerRoom(p.RoomId);
            BaseRoomData roomData = serverRoom.m_RoomData;
            bool verify = serverRoom.ContainsPlayer(p);
            if (verify == false)
            {
                Debug.Print($"错误，用户不在该房间#{p.RoomId}");
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
                var packet = new S2C_LeaveRoomPacket { RoomID = roomData.RoomID, RoomName = roomData.RoomName, LeaveBy = (int)LeaveRoomType.SELF };
                p.SendAsync(PacketType.S2C_LeaveRoom, packet); //主动离开
                Debug.Print("房间内其他人员广播，更新房间信息");

                // 房间内其他人员广播，更新房间信息
                RoomInfo roomInfo = new RoomInfo { RoomID = roomData.RoomID, RoomName = roomData.RoomName, LimitNum = roomData.RoomLimit };
                S2C_RoomInfo packet1 = new S2C_RoomInfo { Room = roomInfo };
                serverRoom.SendAsync(PacketType.S2C_RoomInfo, packet1);
            }

            WinFormsApp1.MainForm.Instance.RefreshRoomNum();
        }
        protected void OnOperateSeat(MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_OperateSeatPacket>(ms); //解包
            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            ServerRoom serverRoom = TCPChatServer.m_RoomManager.GetServerRoom(p.RoomId);
            BaseRoomData roomData = serverRoom.m_RoomData;
            Debug.Print($"{p.UserName}在房间#{p.RoomId}对座位#{request.SeatID}添加操作：{(SeatOperate)request.Operate}");

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
                        Guid botID = Guid.Empty;
                        BasePlayerData basePlayerData = new BasePlayerData
                        {
                            PeerId = botID,
                            UserName = "",
                            NickName = "",
                            SeatId = -1,
                        };
                        ServerPlayer bot = new ServerPlayer("_BOT_", botID, true);
                        bot.NickName = "_BOT_";
                        TCPChatServer.m_PlayerManager.AddPlayer(bot);
                        serverRoom.AddPlayer(bot);
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

            // 重新组装房间内信息
            var players = new List<PlayerInfo>();
            foreach (var item in serverRoom.m_PlayerList)
            {
                BasePlayer player = item.Value;
                PlayerInfo info = new PlayerInfo
                {
                    SeatID = item.Key,
                    UserName = player.UserName,
                    NickName = player.NickName,
                };
                players.Add(info);
            }
            RoomInfo roomInfo = new RoomInfo
            {
                RoomID = roomData.RoomID,
                RoomName = roomData.RoomName,
                LimitNum = roomData.RoomLimit,
                Players = players,
            };
            S2C_RoomInfo packet1 = new S2C_RoomInfo { Room = roomInfo };
            // 房间内广播，更新房间信息
            serverRoom.SendAsync(PacketType.S2C_RoomInfo, packet1);
        }
        protected void OnChat(MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<TheMsg>(ms); //解包
            Debug.Print($"{request.Name}说: {request.Content}");
        }
        protected void OnStartGame()
        {
            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            ServerRoom serverRoom = TCPChatServer.m_RoomManager.GetServerRoom(p.RoomId);
            Debug.Print($"{p.UserName}请求开始比赛，房间#{p.RoomId}({serverRoom.m_PlayerList.Count}/{serverRoom.m_RoomData.RoomLimit})");

            // 校验房间人数。
            if (serverRoom.m_PlayerList.Count < serverRoom.m_RoomData.RoomLimit)
            {
                Debug.Print("ERROR: 房间未满员");
                //ErrorPacket response = new ErrorPacket { Code = 0, Message = "ERROR: 房间未满员" };
                //p.SendAsync(PacketType.S2C_ErrorOperate, response);
                return;
            }
            //TODO: 校验所有成员状态。

            EmptyPacket packet = new EmptyPacket(); //TODO: 组织开局所需数据
            serverRoom.SendAsync(PacketType.S2C_GameStart, packet); //给所有成员发送开始
        }
        protected void OnPlayCard(MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_PlayCard>(ms); //解包

            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            Debug.Print($"{p.UserName}，在房间#{p.RoomId}，座位#{p.SeatId}，出牌：{request.CardID}");

            ServerRoom serverRoom = TCPChatServer.m_RoomManager.GetServerRoom(p.RoomId);
            S2C_PlayCard packet = new S2C_PlayCard { SeatID = p.SeatId, CardID = request.CardID };
            serverRoom.SendAsync(PacketType.S2C_GamePlay, packet); //给所有成员发送开始
        }
        protected void OnGameResult()
        {
            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            ServerRoom serverRoom = TCPChatServer.m_RoomManager.GetServerRoom(p.RoomId);
            S2C_GameResult packet = new S2C_GameResult { };
            serverRoom.SendAsync(PacketType.S2C_GameResult, packet);
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