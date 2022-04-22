using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using ET;

namespace HotFix
{
    // 处理子线程的推送
    public class EventManager : MonoBehaviour
    {
        static EventManager _instance;
        public static EventManager Get()
        {
            return _instance;
        }

        public Queue<byte[]> queue;

        void Awake()
        {
            _instance = this;
            queue = new Queue<byte[]>();
        }

        void Update()
        {
            if (queue.Count > 0)
            {
                var data = queue.Dequeue();
                Handle(data);
            }
        }

        void Handle(byte[] buffer)
        {
            // 解析msgId
            byte msgId = buffer[0];
            byte[] body = new byte[buffer.Length - 1];
            Array.Copy(buffer, 1, body, 0, buffer.Length - 1);

            PacketType type = (PacketType)msgId;
            //Debug.Log($"PacketType={type}");
            switch (type)
            {
                case PacketType.Connected:
                    break;
                case PacketType.Disconnect:
                    {
                        // 销毁所有UI，返回登录页
                        //Debug.Log($"[Handle:{type}]");
                        //UIManager.Get().PopAll();
                        //UIManager.Get().Push<UI_Login>();
                        break;
                    }
                case PacketType.S2C_LoginResult:
                    {
                        MemoryStream stream = new MemoryStream(body, 0, body.Length); //解包
                        S2C_Login packet = ProtobufHelper.FromStream(typeof(S2C_Login), stream) as S2C_Login;
                        Debug.Log($"[Handle:{type}] Code={packet.Code}, Nickname={packet.Nickname}");
                        NetPacketManager.Trigger(type, packet); //派发（为什么在这创建UI，会堵塞接收线程？？）
                        break;
                    }
                case PacketType.S2C_RoomList:
                    {
                        Debug.Log($"[Handle:{type}]");
                        MemoryStream stream = new MemoryStream(body, 0, body.Length); //解包
                        S2C_GetRoomList packet = ProtobufHelper.FromStream(typeof(S2C_GetRoomList), stream) as S2C_GetRoomList;
                        Debug.Log($"[Handle:{type}] RoomCount={packet.Rooms.Count}");
                        if (packet.Rooms.Count > 0)
                        {
                            Debug.Log($"Room.0={packet.Rooms[0].RoomID}");
                        }
                        NetPacketManager.Trigger(type, packet); //派发
                        break;
                    }
                case PacketType.S2C_RoomInfo:
                    {
                        MemoryStream stream = new MemoryStream(body, 0, body.Length); //解包
                        S2C_RoomInfo packet = ProtobufHelper.FromStream(typeof(S2C_RoomInfo), stream) as S2C_RoomInfo;
                        Debug.Log($"[Handle:{type}] RoomId={packet.Room.RoomID}, RoomName={packet.Room.RoomName}, Num={packet.Room.LimitNum}");
                        NetPacketManager.Trigger(type, packet); //派发
                        break;
                    }
                case PacketType.S2C_LeaveRoom:
                    {
                        Empty packet = new Empty();
                        Debug.Log($"[Handle:{type}]");
                        NetPacketManager.Trigger(type, packet); //派发
                        break;
                    }
                case PacketType.S2C_Chat:
                    {
                        MemoryStream stream = new MemoryStream(body, 0, body.Length); //解包
                        TheMsg packet = ProtobufHelper.FromStream(typeof(TheMsg), stream) as TheMsg;
                        Debug.Log($"[Handle:{type}] {packet.Name}说: {packet.Content}");
                        NetPacketManager.Trigger(type, packet); //派发
                        break;
                    }
                case PacketType.S2C_GameStart:
                    {
                        Debug.Log($"[Handle:{type}]");
                        NetPacketManager.Trigger(type, new Empty()); //派发
                        break;
                    }
                default:
                    Debug.LogError($"Handle:无法识别的消息: {type}");
                    break;
            }
        }
    }

    public class NetPacketManager
    {
        public delegate void EventHandler(PacketType t, object packet);
        public static event EventHandler Event;
        public static void RegisterEvent(EventHandler action)
        {
            Event += action;
        }
        public static void UnRegisterEvent(EventHandler action)
        {
            Event -= action;
        }
        public static void Trigger(PacketType type, object packet)
        {
            Event?.Invoke(type, packet);
        }
    }

    public class NetStateManager
    {
        public delegate void EventHandler(int t);
        public static event EventHandler Event;
        public static void RegisterEvent(EventHandler action)
        {
            Event += action;
        }
        public static void UnRegisterEvent(EventHandler action)
        {
            Event -= action;
        }
        public static void Trigger(int type)
        {
            Event?.Invoke(type);
        }
    }
}