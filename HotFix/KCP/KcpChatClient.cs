using System;
using System.Net;
using UnityEngine;
using HotFix;
using ET;

namespace kcp2k.Examples
{
    public class KcpChatClient
    {
        // configuration
        public const ushort Port = 7777;
        public static string Address = "127.0.0.1";
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
            Debug.Log($"KCP: OnClientDataReceived({BitConverter.ToString(message.Array, message.Offset, message.Count)} @ {channel})");
        }
        static void OnDisonnected()
        {
            Debug.Log($"KCP: OnDisonnected");
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
            System.Array.Copy(header, 0, buffer, 0, header.Length);
            System.Array.Copy(body, 0, buffer, header.Length, body.Length);
            //Debug.Log($"[SendAsync] header:{header.Length},body:{body.Length},buffer:{buffer.Length},");
            return buffer;
        }
        private static void Send(PacketType msgId, object cmd, KcpChannel channel)
        {
            byte[] buffer = MakeBuffer(msgId, cmd);
            //client.Send(new ArraySegment<byte>(new byte[] { 0x01, 0x02 }), KcpChannel.Reliable);
            //client.Send(new ArraySegment<byte>(new byte[] { 0x03, 0x04 }), KcpChannel.Unreliable);
            client.Send(new ArraySegment<byte>(new byte[] { 0x01, 0x02 }), KcpChannel.Reliable);
        }

        // 业务
        public static void SendLogin(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError($"token不能为空");
                return;
            }

            var cmd = new C2S_LoginTokenPacket { Token = token };
            Debug.Log($"[C2S] {cmd.Token}");
            Send(PacketType.C2S_LoginToken, cmd, KcpChannel.Reliable);
        }
        public static void SendLogin(string usr, string pwd)
        {
            if (string.IsNullOrEmpty(usr) || string.IsNullOrEmpty(pwd))
            {
                string err_str = "用户名或密码不能为空";
                Debug.LogError(err_str);

                var toast = UIManager.Get().Push<UI_Toast>();
                toast.Show(err_str);
                return;
            }
            if (pwd.Length < 6)
            {
                string err_str = "密码长度过短";
                Debug.LogError(err_str);

                var toast = UIManager.Get().Push<UI_Toast>();
                toast.Show(err_str);
                return;
            }

            var cmd = new C2S_LoginPacket { Username = usr, Password = HotFix.Md5Utils.GetMD5String(pwd) };
            Debug.Log($"[C2S] {cmd.Username}, {cmd.Password}");
            Send(PacketType.C2S_LoginReq, cmd, KcpChannel.Reliable);
        }
        public static void SendSignUp(string usr, string pwd)
        {
            if (string.IsNullOrEmpty(usr) || string.IsNullOrEmpty(pwd))
            {
                Debug.LogError($"用户名或密码不能为空");
                return;
            }
            var cmd = new C2S_LoginPacket { Username = usr, Password = pwd };
            Debug.Log($"[C2S] {cmd.Username}, {cmd.Password}");
            Send(PacketType.C2S_RegisterReq, cmd, KcpChannel.Reliable);
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