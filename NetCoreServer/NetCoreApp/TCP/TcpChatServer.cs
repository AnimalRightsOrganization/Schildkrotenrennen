using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using NetCoreServer;
using NetCoreServer.Utils;
using HotFix;
using IMessage = Google.Protobuf.IMessage;

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

            TCPChatServer.m_PlayerManager.RemovePlayer(Id);
        }

        // 注意这里是线程中
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            // 解析msgId
            byte msgId = buffer[0];
            byte[] body = new byte[buffer.Length - 1];
            Array.Copy(buffer, 1, body, 0, buffer.Length - 1);
            PacketType type = (PacketType)msgId;
            //Debug.Print($"msgType={type}, from {Id}");
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

        protected void SendAsync(PacketType msgId, IMessage cmd)
        {
            byte[] header = new byte[1] { (byte)msgId };
            byte[] body = ProtobufferTool.Serialize(cmd);
            byte[] buffer = new byte[header.Length + body.Length];
            System.Array.Copy(header, 0, buffer, 0, header.Length);
            System.Array.Copy(body, 0, buffer, header.Length, body.Length);
            //Debug.Print($"header:{header.Length},body:{body.Length},buffer:{buffer.Length},");
            SendAsync(buffer);
        }

        protected async void OnLoginReq(byte[] body)
        {
            C2S_Login msg = ProtobufferTool.Deserialize<C2S_Login>(body);
            Debug.Print($"Username={msg.Username}, Password={msg.Password} by {Id}");

            ServerPlayer p = new ServerPlayer(msg.Username, Id);

            UserInfo result = (await MySQLTool.GetUserInfo("test3", "123456"));
            if (result == null)
            {
                Debug.Print($"用户名或密码错误");
                return;
            }
            Debug.Print($"昵称: {result.nickname}");

            TCPChatServer.m_PlayerManager.AddPlayer(p);

            S2C_Login packet = new S2C_Login { Code = 0, Nickname = result.nickname };
            p.SendAsync(PacketType.S2C_LoginResult, packet);
        }
        protected void OnCreateRoom(byte[] body)
        {
            C2S_CreateRoom msg = ProtobufferTool.Deserialize<C2S_CreateRoom>(body);
            Debug.Print($"Name={msg.Name}, Pwd={msg.Pwd}, playerNum={msg.Num} by {Id}");

            //TODO: 验证合法性，在服务器创建房间

            ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
            S2C_CreateRoom packet = new S2C_CreateRoom { Id = 0, Name = msg.Name, Num = msg.Num };
            p.SendAsync(PacketType.S2C_CreateRoom, packet);
        }
        protected void OnChat(byte[] body)
        {
            TheMsg msg = ProtobufferTool.Deserialize<TheMsg>(body);
            Debug.Print($"{msg.Name}说: {msg.Content}");
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