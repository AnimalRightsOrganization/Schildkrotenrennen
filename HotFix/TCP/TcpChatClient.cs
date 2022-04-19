using System;
using System.Threading;
using System.Net.Sockets;
using Debug = UnityEngine.Debug;
using IMessage = Google.Protobuf.IMessage;

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
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            //string message = System.Text.Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            //Debug.Log($"S2C: {message}({size})");
            //return;

            // ����msgId
            byte msgId = buffer[0];
            byte[] body = new byte[buffer.Length - 1];
            Array.Copy(buffer, 1, body, 0, buffer.Length - 1);

            PacketType type = (PacketType)msgId;
            //Debug.Log($"PacketType={type}");
            switch (type)
            {
                case PacketType.Connected:
                    break;
                case PacketType.S2C_LoginResult:
                    {
                        S2C_Login msg = ProtobufferTool.Deserialize<S2C_Login>(body); //���
                        Debug.Log($"[{type}] Code={msg.Code}, Nickname={msg.Nickname}");

                        // �������߳��У���Ҫ�ɷ���ȥִ��
                        //NetPacketManager.Trigger(type);
                        Debug.Log("Trigger...");
                        Push.Trigger(type); //�ɷ�
                        Debug.Log("Trigger OK");
                    }
                    break;
                case PacketType.S2C_CreateRoom:
                    {
                        S2C_CreateRoom msg = ProtobufferTool.Deserialize<S2C_CreateRoom>(body); //���
                        Debug.Log($"[{type}] Name={msg.Id}");
                        Push.Trigger(type); //�ɷ�
                    }
                    break;
                case PacketType.S2C_Chat:
                    {
                        TheMsg msg = ProtobufferTool.Deserialize<TheMsg>(body); //���
                        Debug.Log($"[{type}] {msg.Name}˵: {msg.Content}");
                        //NetPacketManager.Trigger(type); //�ɷ�
                    }
                    break;
                default:
                    Debug.LogError($"�޷�ʶ�����Ϣ: {type}");
                    break;
            }
            //TODO: ͨ��ί�зַ���ȥ
        }

        protected override void OnError(SocketError error)
        {
            Debug.Log($"Chat TCP client caught an error with code {error}");
        }

        private bool _stop;

        private int retry = 5; //TODO:���ݶϿ���ʽ�������������Ͽ���������
    }

    public class TcpChatClient
    {
        protected static ChatClient client;
        const string address = "127.0.0.1";
        const int port = 1111;

        public static void Dispose()
        {
            Debug.Log("�ر�����");
            client?.DisconnectAndStop();
            client = null;
        }
        public static void Connect()
        {
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
            client?.DisconnectAndStop();
        }

        public static void Send(PacketType msgId, IMessage cmd)
        {
            byte[] header = new byte[1] { (byte)msgId };
            byte[] body = ProtobufferTool.Serialize(cmd);
            byte[] buffer = new byte[header.Length + body.Length];
            System.Array.Copy(header, 0, buffer, 0, header.Length);
            System.Array.Copy(body, 0, buffer, header.Length, body.Length);
            Debug.Log($"[Send] header:{header.Length},body:{body.Length},buffer:{buffer.Length},");
            client.Send(buffer);
        }
        public static void SendAsync(PacketType msgId, IMessage cmd)
        {
            byte[] header = new byte[1] { (byte)msgId };
            byte[] body = ProtobufferTool.Serialize(cmd);
            byte[] buffer = new byte[header.Length + body.Length];
            System.Array.Copy(header, 0, buffer, 0, header.Length);
            System.Array.Copy(body, 0, buffer, header.Length, body.Length);
            Debug.Log($"[SendAsync] header:{header.Length},body:{body.Length},buffer:{buffer.Length},");
            client.SendAsync(buffer);
        }
    }
}