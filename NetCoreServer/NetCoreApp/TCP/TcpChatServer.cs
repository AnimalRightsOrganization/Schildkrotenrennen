using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using NetCoreServer;
using HotFix;

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
            SendAsync("hello, you're connected");
        }

        protected override void OnDisconnected()
        {
            Debug.Print($"Chat TCP session with Id {Id} disconnected!");

            TCPChatServer.m_PlayerManager.RemovePlayer(Id);
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            Debug.Print($"C2S: {message}({size})");

            // Multicast message to all connected sessions
            Server.Multicast(message);
            //SendAsync(message);

            // If the buffer starts with '!' the disconnect the current session
            //if (message == "!")
            //    Disconnect();

            // 解析msgId
            byte msgId = buffer[0];
            byte[] body = new byte[buffer.Length - 1];
            Array.Copy(buffer, 1, body, 0, buffer.Length - 1);

            PacketType type = (PacketType)msgId;
            Debug.Print($"msgId={msgId}");

            switch (type)
            {
                case PacketType.C2S_LoginReq:
                    {
                        HotFix.TheMsg msg = ProtobufferTool.Deserialize<HotFix.TheMsg>(body);
                        Debug.Print($"[{type}] Name={msg.Name}, Content={msg.Content}");

                        //TODO: SQL验证操作

                        ServerPlayer p = new ServerPlayer(msg.Name, Id);
                        TCPChatServer.m_PlayerManager.AddPlayer(p);

                        break;
                    }
                case PacketType.C2S_CreateRoom:
                    {
                        Debug.Print($"create by:");

                        C2S_CreateRoom msg = ProtobufferTool.Deserialize<C2S_CreateRoom>(body);
                        Debug.Print($"[{type}] playerNum={msg.Num}");

                        break;
                    }
            }
            //TODO: 通过委托分发出去
        }

        protected override void OnError(SocketError error)
        {
            Debug.Print($"Chat TCP session caught an error with code {error}");
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

            /*
            // Perform text input
            for (;;)
            {
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

                // Restart the server
                if (line == "!")
                {
                    Debug.Print("Server restarting...");
                    server.Restart();
                    Debug.Print("Done!");
                    continue;
                }

                // Multicast admin message to all sessions
                line = "(admin) " + line;
                server.Multicast(line);
            }
            */
        }
        public static void Stop()
        {
            // Stop the server
            Debug.Print("Server stopping...");
            server?.Stop();
            Debug.Print("Done!");
        }
    }
}