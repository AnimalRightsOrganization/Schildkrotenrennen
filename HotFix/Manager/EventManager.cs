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

                        // 这里是线程中，需要派发出去执行
                        Debug.Log("Trigger...");
                        Push.Trigger(type); //派发（为什么在这创建UI，会堵塞接收线程？？）
                        Debug.Log("Trigger OK");
                    }
                    break;
                case PacketType.S2C_CreateRoom:
                    {
                        S2C_CreateRoom msg = ProtobufferTool.Deserialize<S2C_CreateRoom>(body); //解包
                        Debug.Log($"[{type}] Name={msg.Id}");
                        Push.Trigger(type); //派发
                    }
                    break;
                case PacketType.S2C_Chat:
                    {
                        TheMsg msg = ProtobufferTool.Deserialize<TheMsg>(body); //解包
                        Debug.Log($"[{type}] {msg.Name}说: {msg.Content}");
                        Push.Trigger(type); //派发
                    }
                    break;
                default:
                    Debug.LogError($"无法识别的消息: {type}");
                    break;
            }
            //TODO: 通过委托分发出去
        }
    }

    public class Push
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

    /*
    public class MyEvent<T0> : UnityEvent<T0> { }
    public class MyEvent<T0, T1> : UnityEvent<T0, T1> { }
    public class MyEvent<T0, T1, T2> : UnityEvent<T0, T1, T2> { }

    public class NetStateManager
    {
        private static MyEvent<int> eventList = new MyEvent<int>();
        public static void RegisterEvent(UnityAction<int> action)
        {
            eventList.AddListener(action);
        }
        public static void UnRegisterEvent(UnityAction<int> action)
        {
            eventList.RemoveListener(action);
        }
        public static void Trigger(int peer)
        {
            eventList.Invoke(peer);
        }
    }

    public class NetPacketManager
    {
        private static MyEvent<PacketType> eventList = new MyEvent<PacketType>();
        public static void RegisterEvent(UnityAction<PacketType> action)
        {
            eventList.AddListener(action);
        }
        public static void UnRegisterEvent(UnityAction<PacketType> action)
        {
            eventList.RemoveListener(action);
        }
        public static void Trigger(PacketType peer)
        {
            eventList.Invoke(peer);
        }
    }
    */
}