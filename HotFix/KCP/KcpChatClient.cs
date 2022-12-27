using System;
using System.Linq;
using System.Net;
using UnityEngine;
using HotFix;

namespace kcp2k.Examples
{
    public class KcpChatClient
    {
        // configuration
        public const ushort Port = 7777;
        public static string Address;
        public static KcpConfig config = new KcpConfig();

        public static ClientPlayerManager m_PlayerManager;
        public static ClientRoom m_ClientRoom;

        // client
        public static KcpClient client = new KcpClient(OnConnected, OnData, OnDisonnected, OnError);

        // MonoBehaviour ///////////////////////////////////////////////////////
        public KcpChatClient()
        {
            // logging
            //Log.Info = Debug.Log;
            //Log.Warning = Debug.LogWarning;
            //Log.Error = Debug.LogError;
        }

        //public void LateUpdate() => client.Tick();

        static void OnConnected()
        {
            Debug.Log($"KCP: OnConnected");
        }
        static void OnData(ArraySegment<byte> message, KcpChannel channel)
        {
            //Debug.Log($"KCP: OnClientDataReceived({BitConverter.ToString(message.Array, message.Offset, message.Count)} @ {channel})");
            EventManager.Get.queue.Enqueue(message.ToArray());
        }
        static void OnDisonnected()
        {
            Debug.LogError($"KCP: OnDisonnected");

            //Debug.Log($"trying: {Main.trying}, times={Main.try_times}");
            if (Main.trying == false)
                Main.TryConnect();

            if (Main.try_times >= 3)
            {
                PoolManager.Get.DespawnAll();
                UIManager.Get.PopAll();
                UIManager.Get.Push<UI_Login>();
                UIManager.Get.Push<UI_Connect>(1);

                var ui_toast = UIManager.Get.Push<UI_Toast>();
                ui_toast.Show("与服务器断开连接");
            }
        }
        static void OnError(ErrorCode error, string reason)
        {
            Debug.LogWarning($"KCP: OnClientError({error}, {reason}");
        }

        public static bool IsConnected()
        {
            return client.connected;
        }
        public static void Connect()
        {
            m_PlayerManager = new ClientPlayerManager();

            Debug.Log($"读取配置文件:{Main.present.gate}");
            IPAddress IPAddr;
            if (IPAddress.TryParse(Main.present.gate, out IPAddr))
            {
                Address = Main.present.gate;
                Debug.Log($"是IP:{Address}");
            }
            else
            {
                Address = GetHostEntry(Main.present.gate).ToString();
                Debug.Log($"域名转IP:{Main.present.gate}→{Address}");
            }

            client.Connect(Address, Port, config);
            Debug.Log($"Connect to: {Address}:{Port}");
        }
        public static void Disconnect()
        {
            client.Disconnect();
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
            Array.Copy(header, 0, buffer, 0, header.Length);
            Array.Copy(body, 0, buffer, header.Length, body.Length);
            //Debug.Log($"[SendAsync] header:{header.Length},body:{body.Length},total:{buffer.Length},");
            return buffer;
        }
        private static void SendAsync(PacketType msgId, object cmd, KcpChannel channel = KcpChannel.Reliable)
        {
            byte[] buffer = MakeBuffer(msgId, cmd);
            client.Send(new ArraySegment<byte>(buffer), channel);
        }

        // 业务
        public static void SendLogin(string token)
        {
            Debug.Log("SendLogin.Start");
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError($"token不能为空");
                return;
            }
            var cmd = new C2S_LoginTokenPacket { Token = token };
            Debug.Log($"[C2S_LoginToken] {cmd.Token}");
            SendAsync(PacketType.C2S_LoginToken, cmd);
        }
        public static void SendLogin(string usr, string pwd)
        {
            if (string.IsNullOrEmpty(usr) || string.IsNullOrEmpty(pwd))
            {
                string err_str = "用户名或密码不能为空";
                Debug.LogError(err_str);

                var toast = UIManager.Get.Push<UI_Toast>();
                toast.Show(err_str);
                return;
            }
            if (pwd.Length < 6)
            {
                string err_str = "密码长度过短";
                Debug.LogError(err_str);

                var toast = UIManager.Get.Push<UI_Toast>();
                toast.Show(err_str);
                return;
            }

            var cmd = new C2S_LoginPacket { Username = usr, Password = HotFix.Md5Utils.GetMD5String(pwd) };
            Debug.Log($"[C2S_LoginReq] {cmd.Username}, {cmd.Password}");
            SendAsync(PacketType.C2S_LoginReq, cmd);
        }
        public static void SendLogout()
        {
            Debug.Log("SendLogout");
            var cmd = new EmptyPacket();
            SendAsync(PacketType.C2S_LogoutReq, cmd);
        }
        public static void SendSignUp(string usr, string pwd)
        {
            if (string.IsNullOrEmpty(usr) || string.IsNullOrEmpty(pwd))
            {
                Debug.LogError($"用户名或密码不能为空");
                return;
            }
            var cmd = new C2S_LoginPacket { Username = usr, Password = pwd };
            Debug.Log($"[C2S_RegisterReq] {cmd.Username}, {cmd.Password}");
            SendAsync(PacketType.C2S_RegisterReq, cmd);
        }
        public static void SendChat(string message)
        {
            if (message.Length <= 0)
            {
                var ui_toast = UIManager.Get.Push<UI_Toast>();
                ui_toast.Show("内容为空");
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
                Debug.LogError($"房间名称至少3个字:{name.Length}");
                var toast = UIManager.Get.Push<UI_Toast>();
                toast.Show("房间名称至少3个字");
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
                var ui_toast = UIManager.Get.Push<UI_Toast>();
                ui_toast.Show("人数不足，请等待");
                return;
            }
            //Debug.Log("[C2S] 请求开始比赛");
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
            client.Send(new ArraySegment<byte>(new byte[] { 0x01, 0x02 }), KcpChannel.Reliable);
        }
        public static void SendTestMessage2()
        {
            client.Send(new ArraySegment<byte>(new byte[] { 0x03, 0x04 }), KcpChannel.Unreliable);
        }
    }
}