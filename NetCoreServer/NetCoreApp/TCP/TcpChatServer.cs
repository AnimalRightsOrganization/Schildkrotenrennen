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
            //ServerPlayer p = new ServerPlayer(string.Empty, Id);
            //TCPChatServer.m_PlayerManager.AddPlayer(p);

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

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            //string message = System.Text.Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            //Debug.Print($"C2S: {message}({size})");

            // Multicast message to all connected sessions
            //Server.Multicast(message);
            //return;

            // If the buffer starts with '!' the disconnect the current session
            //if (message == "!")
            //    Disconnect();

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
                    {
                        C2S_Login msg = ProtobufferTool.Deserialize<C2S_Login>(body);
                        Debug.Print($"[{type}] Username={msg.Username}, Password={msg.Password} by {Id}");

                        //TODO: SQL验证操作
                        //bool result = MySQLTool.CheckLogin();

                        ServerPlayer p = new ServerPlayer(msg.Username, Id);
                        TCPChatServer.m_PlayerManager.AddPlayer(p);
                        string nickName = string.Empty;

                        S2C_Login packet = new S2C_Login { Code = 0, Nickname = nickName };
                        p.SendAsync(PacketType.S2C_LoginResult, packet);
                    }
                    break;
                case PacketType.C2S_CreateRoom:
                    {
                        C2S_CreateRoom msg = ProtobufferTool.Deserialize<C2S_CreateRoom>(body);
                        Debug.Print($"[{type}] Name={msg.Name}, Pwd={msg.Pwd}, playerNum={msg.Num} by {Id}");

                        //TODO: 验证合法性，在服务器创建房间

                        ServerPlayer p = TCPChatServer.m_PlayerManager.GetPlayerByPeerId(Id);
                        S2C_CreateRoom packet = new S2C_CreateRoom { Id = 0, Name = msg.Name, Num = msg.Num };
                        p.SendAsync(PacketType.S2C_CreateRoom, packet);
                    }
                    break;
                case PacketType.C2S_Chat:
                    {
                        TheMsg msg = ProtobufferTool.Deserialize<TheMsg>(body);
                        Debug.Print($"[{type}] {msg.Name}说: {msg.Content}");
                    }
                    break;
                default:
                    Debug.Print($"无法识别的消息: {type}");
                    break;
            }
            //TODO: 通过委托分发出去
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