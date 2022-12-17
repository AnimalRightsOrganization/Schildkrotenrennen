using System;
using UnityEngine;

namespace kcp2k.Examples
{
    public class KcpChatClient
    {
        // configuration
        public const ushort Port = 7777;
        public static string Address = "127.0.0.1";
        public static KcpConfig config = new KcpConfig();

        // client
        protected static KcpClient client = new KcpClient(OnConnected, OnData, OnDisonnected, OnError);

        // MonoBehaviour ///////////////////////////////////////////////////////
        public KcpChatClient()
        {
            // logging
            Log.Info = Debug.Log;
            Log.Warning = Debug.LogWarning;
            Log.Error = Debug.LogError;
        }

        public void LateUpdate() => client.Tick();

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

        public static void Connect()
        {
            client.Connect(Address, Port, config);
        }
        public static void Send(KcpChannel channel)
        {
            client.Send(new ArraySegment<byte>(new byte[] { 0x01, 0x02 }), KcpChannel.Reliable);
            client.Send(new ArraySegment<byte>(new byte[] { 0x03, 0x04 }), KcpChannel.Unreliable);
        }
        public static void Disconnect()
        {
            client.Disconnect();
        }
    }
}