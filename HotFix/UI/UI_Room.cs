﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ET;

namespace HotFix
{
    public class UI_Room : UIBase
    {
        #region 界面组件
        public Text m_NameText;
        public Text m_PwdText;
        public Button m_CloseBtn;
        public Button m_StartBtn;
        public List<Item_Room> SeatList;
        #endregion

        #region 内置方法
        void Awake()
        {
            Transform seatRoot = transform.Find("SeatPanel/Layout");
            SeatList = new List<Item_Room>();
            for (int i = 0; i < seatRoot.childCount; i++)
            {
                var seatItem = seatRoot.GetChild(i).GetComponent<Button>();
                var script = seatItem.gameObject.AddComponent<Item_Room>();
                SeatList.Add(script);
            }

            m_NameText = transform.Find("Background/Text").GetComponent<Text>();
            m_CloseBtn = transform.Find("Background/CloseBtn").GetComponent<Button>();
            m_StartBtn = transform.Find("StartBtn").GetComponent<Button>();
            m_CloseBtn.onClick.AddListener(OnSendLeaveRoom);
            m_StartBtn.onClick.AddListener(OnSendStartGame);
        }

        void Start()
        {
            NetPacketManager.RegisterEvent(OnNetCallback);
        }

        void OnDestroy()
        {
            NetPacketManager.UnRegisterEvent(OnNetCallback);

            m_CloseBtn.onClick.RemoveListener(OnSendLeaveRoom);
            m_StartBtn.onClick.RemoveListener(OnSendStartGame);
            m_CloseBtn = null;
            m_StartBtn = null;
        }
        #endregion

        #region 按钮事件
        void OnSendLeaveRoom()
        {
            Debug.Log("请求离开房间");
            EmptyPacket cmd = new EmptyPacket();
            TcpChatClient.SendAsync(PacketType.C2S_LeaveRoom, cmd);
        }
        void OnSendStartGame()
        {
            Debug.Log("请求开始比赛");
            EmptyPacket cmd = new EmptyPacket();
            TcpChatClient.SendAsync(PacketType.C2S_GameStart, cmd);
        }
        #endregion

        #region 网络事件
        public void InitUI(BaseRoomData roomData)
        {
            m_NameText.text = roomData.RoomName;

            Debug.Log($"房间初始化：ID={roomData.RoomID}，人数={roomData.Players.Count}/{roomData.RoomLimit}");

            // 控制座位总数显示
            for (int i = 0; i < SeatList.Count; i++)
            {
                var scriptItem = SeatList[i];
                if (i >= roomData.RoomLimit)
                {
                    scriptItem.gameObject.SetActive(false);
                }
                else
                {
                    scriptItem.gameObject.SetActive(true);

                    BasePlayerData playerData = null;
                    foreach (var data in roomData.Players)
                    {
                        if (data.SeatId == i)
                        {
                            playerData = data;
                            break;
                        }
                    }
                    if (playerData != null)
                    {
                        Debug.Log($"InitUI: {playerData.ToString()}");
                        scriptItem.InitUI(playerData);
                    }
                    else
                    {
                        scriptItem.InitUI(null);
                    }
                }
            }
        }
        public void OnNetCallback(PacketType type, object reader)
        {
            switch (type)
            {
                case PacketType.S2C_LeaveRoom:
                    UIManager.Get().Pop(this);
                    break;
                case PacketType.S2C_GameStart:
                    //TODO: 解包，获取完整比赛信息，座位号和颜色。重连也下发这个消息。
                    UIManager.Get().Pop(this);
                    UIManager.Get().Push<UI_Game>();
                    break;
            }
        }
        #endregion
    }
}