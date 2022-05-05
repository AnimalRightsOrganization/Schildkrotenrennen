using System.Net;
using System.Net.Sockets;
using System.Threading;
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

        // 被动断开（尝试重连）
        protected override void OnDisconnected()
        {
            Debug.Log($"Chat TCP client disconnected a session with Id {Id}");
            Debug.Log("<color=red>Disonnected</color>");

            if (tryTime > 3)
            {
                _stop = true;
                Debug.Log("<color=red>重连达到上限</color>");
            }

            // Wait for a while...
            Thread.Sleep(1000);

            // Try to connect again
            if (!_stop)
            {
                ConnectAsync();

                tryTime++;
            }

            // 这里是异步线程中，需要通过Update推送到主线程。
            PacketType msgId = PacketType.Disconnect;
            byte[] buffer = new byte[1] { (byte)msgId };
            EventManager.Get().queue.Enqueue(buffer);
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            // 这里是异步线程中，需要通过Update推送到主线程。
            System.Array.Resize<byte>(ref buffer, (int)size); //8192裁剪
            EventManager.Get().queue.Enqueue(buffer);
        }

        protected override void OnError(SocketError error)
        {
            Debug.Log($"Chat TCP client caught an error with code {error}");
        }

        private bool _stop;

        public int tryTime; //尝试重连计数
    }

    public class TcpChatClient
    {
        protected static ChatClient client;
        //const string address = "192.168.1.101"; //通过http读取配置
        static string address;
        const int port = 1111;

        public static ClientPlayerManager m_PlayerManager;
        public static ClientRoom m_ClientRoom;

        public static void Dispose()
        {
            Debug.Log("关闭网络");

            if (client != null)
            {
                // 不管有没有停反正执行一次，标记stop，让线程等待释放
                client.DisconnectAndStop();
                client.Dispose();
            }
            client = null;
        }
        public static void Connect()
        {
            m_PlayerManager = new ClientPlayerManager();

            // Create a new TCP chat client
            address = GetHostEntry(Main.present.server).ToString();
            client = new ChatClient(address, port);
            client.tryTime = 0;

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

        // 根据域名解析出IP
        private static IPAddress GetHostEntry(string domian)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(domian);
            IPAddress myip = ipHostInfo.AddressList[0];
            return myip;
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
        private static void Send(PacketType msgId, object cmd)
        {
            byte[] buffer = MakeBuffer(msgId, cmd);
            client.Send(buffer);
        }
        private static void SendAsync(PacketType msgId, object cmd)
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

            var cmd = new C2S_LoginPacket { Username = usr, Password = pwd };
            Debug.Log($"[C2S] {cmd.Username}, {cmd.Password}");
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
        public static void SendGetRoomList(int page)
        {
            var cmd = new C2S_RoomListPacket { Page = page };
            SendAsync(PacketType.C2S_RoomList, cmd);
        }
        public static bool SendCreateRoom(string name, string pwd, int num)
        {
            if (name.Length < 3)
            {
                Debug.LogError($"房间名称至少3个字:{name.Length}"); //TODO: Toast
                return false;
            }

            var cmd = new C2S_CreateRoomPacket { RoomName = name, RoomPwd = pwd, LimitNum = num };
            SendAsync(PacketType.C2S_CreateRoom, cmd);
            return true;
        }
        public static void SendJoinRoom(int roomId, string pwd)
        {
            var cmd = new C2S_JoinRoomPacket { RoomID = roomId, RoomPwd = pwd };
            SendAsync(PacketType.C2S_JoinRoom, cmd);
        }
        public static void SendLeaveRoom()
        {
            EmptyPacket cmd = new EmptyPacket();
            SendAsync(PacketType.C2S_LeaveRoom, cmd);
        }
        public static void SendGameStart()
        {
            if (m_ClientRoom.m_PlayerDic.Count < m_ClientRoom.RoomLimit)
            {
                Debug.LogError($"人数不足，请等待：{m_ClientRoom.m_PlayerDic.Count} < {m_ClientRoom.RoomLimit}");
                var ui_toast = UIManager.Get().Push<UI_Toast>();
                ui_toast.Show("人数不足，请等待");
                return;
            }
            Debug.Log("[C2S] 请求开始比赛");
            EmptyPacket cmd = new EmptyPacket();
            SendAsync(PacketType.C2S_GameStart, cmd);
        }
        public static void SendOperateSeat(int seatId, SeatOperate op)
        {
            //Debug.Log($"对座位{seatId}，操作{op}");
            var cmd = new C2S_OperateSeatPacket { SeatID = seatId, Operate = (int)op };
            SendAsync(PacketType.C2S_OperateSeat, cmd);
        }
        public static void SendGamePlay(int cardId, int color = -1)
        {
            var cmd = new C2S_PlayCardPacket { CardID = cardId, Color = color };
            SendAsync(PacketType.C2S_GamePlay, cmd);
        }

        public static void SendTestMessage1()
        {
            byte[] buffer = new byte[1] { 3 };
            client.SendAsync(buffer);
        }
        public static void SendTestMessage2()
        {
            byte[] buffer = new byte[2] { 3, 1 };
            client.SendAsync(buffer);
        }
    }
}