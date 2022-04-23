using System.Threading;
using System.Net.Sockets;
using Debug = UnityEngine.Debug;
using ET;

namespace HotFix
{
    public class ChatClient : TcpClient
    {
        public ChatClient(string address, int port) : base(address, port) { }

        public void DisconnectAndStop()
        {
            _stop = true;
            DisconnectAsync();
            while (IsConnected)
                Thread.Yield();
        }

        protected override void OnConnected()
        {
            Debug.Log($"Chat TCP client connected a new session with Id {Id}");
            Debug.Log("<color=green>Connected!</color>");
        }

        protected override void OnDisconnected()
        {
            Debug.Log($"Chat TCP client disconnected a session with Id {Id}");
            Debug.Log("<color=red>Disonnected</color>");

            // Wait for a while...
            Thread.Sleep(1000);

            // Try to connect again
            if (!_stop)
                ConnectAsync();

            //TODO: 只执行一次！！！
            // 这里是异步线程中，需要通过Update推送到主线程。
            PacketType msgId = PacketType.Disconnect;
            byte[] buffer = new byte[1] { (byte)msgId };
            EventManager.Get().queue.Enqueue(buffer);
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            //string message = System.Text.Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            //Debug.Log($"S2C: {message}({size})");

            // 这里是异步线程中，需要通过Update推送到主线程。
            System.Array.Resize<byte>(ref buffer, (int)size); //8192裁剪
            EventManager.Get().queue.Enqueue(buffer);
        }

        protected override void OnError(SocketError error)
        {
            Debug.Log($"Chat TCP client caught an error with code {error}");
        }

        private bool _stop;

        //private const int RETRY = 5; //TODO:根据断开形式，服务器主动断开则不再重连
    }

    public class TcpChatClient
    {
        protected static ChatClient client;
        const string address = "127.0.0.1";
        const int port = 1111;

        public static ClientPlayerManager m_PlayerManager;

        public static void Dispose()
        {
            m_PlayerManager = null;

            Debug.Log("关闭网络");

            //Debug.Log($"IsExist:{client != null}"); //True
            if (client != null)
            {
                //Debug.Log($"IsConnected:{client.IsConnected}"); //False
                if (client.IsConnected)
                {
                    client.DisconnectAndStop();
                }
                //Debug.Log($"IsSocketDisposed:{client.IsDisposed}"); //False
                if (client.IsDisposed == false)
                {
                    client.Dispose();
                }
            }
            client = null;
        }
        public static void Connect()
        {
            m_PlayerManager = new ClientPlayerManager();

            // Create a new TCP chat client
            client = new ChatClient(address, port);

            // Connect the client
            client.ConnectAsync();
            Debug.Log($"connect to: {address}:{port}");
        }
        public static void Reconnect()
        {
            client.Reconnect();
            Debug.Log($"reconnect to: {address}:{port}");
        }
        public static void Disconnect()
        {
            //if (client == null)
            //    return;
            m_PlayerManager = null;
            client?.DisconnectAndStop();
        }

        private static byte[] MakeBuffer(PacketType msgId, object cmd)
        {
            byte[] header = new byte[1] { (byte)msgId };
            byte[] body = ProtobufHelper.ToBytes(cmd);
            byte[] buffer = new byte[header.Length + body.Length];
            System.Array.Copy(header, 0, buffer, 0, header.Length);
            System.Array.Copy(body, 0, buffer, header.Length, body.Length);
            //Debug.Log($"[SendAsync] header:{header.Length},body:{body.Length},buffer:{buffer.Length},");
            return buffer;
        }
        public static void Send(PacketType msgId, object cmd)
        {
            byte[] buffer = MakeBuffer(msgId, cmd);
            client.Send(buffer);
        }
        public static void SendAsync(PacketType msgId, object cmd)
        {
            byte[] buffer = MakeBuffer(msgId, cmd);
            client.SendAsync(buffer);
        }

        public static void SendLogin(string usr, string pwd)
        {
            if (string.IsNullOrEmpty(usr) || string.IsNullOrEmpty(pwd))
            {
                Debug.LogError($"用户名或密码不能为空"); //TODO: Toast
                return;
            }
            if (pwd.Length < 6)
            {
                Debug.LogError($"密码长度过短"); //TODO: Toast
                return;
            }
            //TODO: 服务器/客户端共用规则，双边验证...

            C2S_Login cmd = new C2S_Login { Username = usr, Password = pwd };
            SendAsync(PacketType.C2S_LoginReq, cmd);
        }
        public static void SendChat(string message)
        {
            if (message.Length <= 0)
            {
                Debug.LogError($"内容为空"); //TODO: Toast
                return;
            }

            TheMsg cmd = new TheMsg { Name = "lala", Content = message };
            SendAsync(PacketType.C2S_Chat, cmd);
        }
        public static void SendGetRoomList()
        {
            EmptyPacket cmd = new EmptyPacket();
            SendAsync(PacketType.C2S_RoomList, cmd);
        }
        public static bool SendCreateRoom(string name, string pwd, int num)
        {
            if (name.Length < 3)
            {
                Debug.LogError($"房间名称至少3个字:{name.Length}"); //TODO: Toast
                return false;
            }

            C2S_CreateRoom cmd = new C2S_CreateRoom { RoomName = name, RoomPwd = pwd, LimitNum = num };
            SendAsync(PacketType.C2S_CreateRoom, cmd);
            return true;
        }
        public static void SendJoinRoom(int roomId, string pwd)
        {
            C2S_JoinRoom cmd = new C2S_JoinRoom { RoomID = roomId, RoomPwd = pwd };
            SendAsync(PacketType.C2S_LeaveRoom, cmd);
        }
        public static void SendLeaveRoom()
        {
            EmptyPacket cmd = new EmptyPacket();
            SendAsync(PacketType.C2S_LeaveRoom, cmd);
        }
        public static void SendPlayCard(int cardId)
        {
            C2S_PlayCard cmd = new C2S_PlayCard { CardID = cardId };
            SendAsync(PacketType.C2S_GamePlay, cmd);
        }
    }
}