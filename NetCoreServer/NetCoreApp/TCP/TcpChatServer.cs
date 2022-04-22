using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
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

            Empty cmd = new Empty();
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
            Debug.Print($"OnReceived: buffer={buffer.Length}, offset={offset}, size={size}");

            // 解析msgId
            byte msgId = buffer[0];
            byte[] body = new byte[size - 1];
            Array.Copy(buffer, 1, body, 0, size - 1);
            PacketType type = (PacketType)msgId;
            Debug.Print($"msgType={type}, from {Id}");

            switch (type)
            {
                case PacketType.Connected:
                    break;
                case PacketType.C2S_LoginReq:
                    OnLoginReq(body);
                    break;
                case PacketType.C2S_CreateRoom:
                    OnCreateRoom(body);
                    break;
                case PacketType.C2S_RoomList:
                    OnRoomList(body);
                    break;
                case PacketType.C2S_Chat:
                    OnChat(body);
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

        protected async void OnLoginReq(byte[] body)
        {
            //C2S_Login msg = ProtobufferTool.Deserialize<C2S_Login>(body);
            MemoryStream ms = new MemoryStream(body, 0, body.Length);
            var request = ProtobufHelper.FromStream(typeof(C2S_Login), ms) as C2S_Login; //解包
            Debug.Print($"Username={request.Username}, Password={request.Password} by {Id}");

            ServerPlayer p = new ServerPlayer(request.Username, Id);

            UserInfo result = await MySQLTool.GetUserInfo("test3", "123456");
            if (result == null)
            {
                Debug.Print($"用户名或密码错误");
                return;
            }
            Debug.Print($"昵称: {result.nickname}");

            TCPChatServer.m_PlayerManager.AddPlayer(p);

            S2C_Login packet = new S2C_Login { Code = 0, Nickname = result.nickname };
            p.SendAsync(PacketType.S2C_LoginResult, packet);

            WinFormsApp1.MainForm.Instance.RefreshPlayerNum();
        }
        protected void OnRoomList(byte[] body)
        {
            // 空消息，不用解析
            S2C_GetRoomList packet = new S2C_GetRoomList();
            foreach (ServerRoom room in TCPChatServer.m_RoomManager.GetAll())
            {
                RoomInfo info = new RoomInfo
                { 
                    RoomID = room.m_RoomData.RoomID,
                    RoomName = room.m_RoomData.RoomName,
                    CurNum = room.CurCount,
                    LimitNum = room.m_RoomData.RoomLimit,
                };
                packet.Rooms.Add(info);
            }

            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            p.SendAsync(PacketType.S2C_RoomList, packet);
        }
        protected void OnCreateRoom(byte[] body)
        {
            //C2S_CreateRoom msg = ProtobufferTool.Deserialize<C2S_CreateRoom>(body);
            MemoryStream ms = new MemoryStream(body, 0, body.Length);
            var request = ProtobufHelper.FromStream(typeof(C2S_CreateRoom), ms) as C2S_CreateRoom; //解包
            Debug.Print($"Name={request.RoomName}, Pwd={request.RoomPwd}, playerNum={request.LimitNum} by {Id}");

            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            RoomInfo roomInfo = new RoomInfo { RoomID = 0, RoomName = request.RoomName, LimitNum = request.LimitNum };

            // 验证合法性（总数是否超过等），在服务器创建房间
            BaseRoomData baseRoomData = new BaseRoomData { RoomID = -1, RoomName = request.RoomName, RoomPwd = request.RoomPwd, RoomLimit = request.LimitNum };
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
        protected void OnJoinRoom(byte[] body)
        {
            //C2S_JoinRoom msg = ProtobufferTool.Deserialize<C2S_JoinRoom>(body);
            MemoryStream ms = new MemoryStream(body, 0, body.Length);
            var request = ProtobufHelper.FromStream(typeof(C2S_JoinRoom), ms) as C2S_JoinRoom; //解包
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
        protected void OnLeaveRoom(byte[] body)
        {
            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            Debug.Print($"{p.UserName}当前在房间#{p.RoomId},座位#{p.SeatId}，请求离开");

            // 验证合法性
            ServerRoom serverRoom = TCPChatServer.m_RoomManager.GetServerRoom(p.RoomId);
            bool verify = serverRoom.RemovePlayer(p);
            if (verify == false)
            {
                Debug.Print($"错误，无法离开房间#{p.RoomId}");
                return;
            }
        }
        protected void OnChat(byte[] body)
        {
            //TheMsg msg = ProtobufferTool.Deserialize<TheMsg>(body);
            MemoryStream ms = new MemoryStream(body, 0, body.Length);
            var request = ProtobufHelper.FromStream(typeof(TheMsg), ms) as TheMsg; //解包
            Debug.Print($"{request.Name}说: {request.Content}");
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