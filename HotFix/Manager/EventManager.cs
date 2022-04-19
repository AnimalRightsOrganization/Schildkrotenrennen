using System;
using System.Collections.Generic;
using UnityEngine;

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
                case PacketType.S2C_LoginResult:
                    {
                        S2C_Login msg = ProtobufferTool.Deserialize<S2C_Login>(body); //解包
                        Debug.Log($"[{type}] Code={msg.Code}, Nickname={msg.Nickname}");
                        NetPacketManager.Trigger(type); //派发（为什么在这创建UI，会堵塞接收线程？？）
                    }
                    break;
                case PacketType.S2C_CreateRoom:
                    {
                        S2C_CreateRoom msg = ProtobufferTool.Deserialize<S2C_CreateRoom>(body); //解包
                        Debug.Log($"[{type}] RoomId={msg.Id}, RoomName={msg.Name}, Num={msg.Num}");
                        NetPacketManager.Trigger(type); //派发
                    }
                    break;
                case PacketType.S2C_Chat:
                    {
                        TheMsg msg = ProtobufferTool.Deserialize<TheMsg>(body); //解包
                        Debug.Log($"[{type}] {msg.Name}说: {msg.Content}");
                        NetPacketManager.Trigger(type); //派发
                    }
                    break;
                default:
                    Debug.LogError($"无法识别的消息: {type}");
                    break;
            }
            //TODO: 通过委托分发出去
        }
    }

    public class NetPacketManager
    {
        public delegate void SampleEventHandler(PacketType t);
        public static event SampleEventHandler SampleEvent;
        public static void RegisterEvent(SampleEventHandler action)
        {
            SampleEvent += action;
        }
        public static void UnRegisterEvent(SampleEventHandler action)
        {
            SampleEvent -= action;
        }
        public static void Trigger(PacketType type)
        {
            SampleEvent?.Invoke(type);
        }
    }

    public class NetStateManager
    {
        public delegate void SampleEventHandler(int t);
        public static event SampleEventHandler SampleEvent;
        public static void RegisterEvent(SampleEventHandler action)
        {
            SampleEvent += action;
        }
        public static void UnRegisterEvent(SampleEventHandler action)
        {
            SampleEvent -= action;
        }
        public static void Trigger(int type)
        {
            SampleEvent?.Invoke(type);
        }
    }
}