using System.Collections.Generic;
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
            if (TcpChatClient.m_ClientRoom.m_PlayerList.Count < TcpChatClient.m_ClientRoom.RoomLimit)
            {
                Debug.LogError($"人数不足，请等待：{TcpChatClient.m_ClientRoom.m_PlayerList.Count} < {TcpChatClient.m_ClientRoom.RoomLimit}");
                var ui_toast = UIManager.Get().Push<UI_Toast>();
                ui_toast.Show("人数不足，请等待");
                return;
            }
            Debug.Log("请求开始比赛");
            EmptyPacket cmd = new EmptyPacket();
            TcpChatClient.SendAsync(PacketType.C2S_GameStart, cmd);
        }
        #endregion

        #region 网络事件
        public void UpdateUI(BaseRoomData roomData)
        {
            Debug.Log($"房间初始化：ID={roomData.RoomID}，人数={roomData.Players.Count}/{roomData.RoomLimit}");

            m_NameText.text = roomData.RoomName;

            // 控制座位总数显示
            for (int i = 0; i < SeatList.Count; i++)
            {
                int index = i;

                var scriptItem = SeatList[index];
                if (index >= roomData.RoomLimit)
                {
                    scriptItem.gameObject.SetActive(false);
                }
                else
                {
                    scriptItem.gameObject.SetActive(true);

                    BasePlayerData playerData = null;
                    foreach (var data in roomData.Players)
                    {
                        if (data.SeatId == index)
                        {
                            playerData = data;
                            break;
                        }
                    }
                    if (playerData != null)
                    {
                        Debug.Log($"InitUI: {playerData.ToString()}");
                        scriptItem.UpdateUI(playerData, index);
                    }
                    else
                    {
                        scriptItem.UpdateUI(null, index);
                    }
                }
            }
        }
        void OnNetCallback(PacketType type, object reader)
        {
            switch (type)
            {
                case PacketType.S2C_LeaveRoom:
                    OnLeaveRoom(reader);
                    break;
                case PacketType.S2C_RoomInfo: //别人加入/离开
                    OnGetRoomInfo(reader);
                    break;
                case PacketType.S2C_GameStart:
                    OnGameStart(reader);
                    break;
            }
        }
        void OnLeaveRoom(object reader)
        {
            UIManager.Get().Pop(this);

            var packet = (S2C_LeaveRoomPacket)reader;
            string message = string.Empty;
            switch ((LeaveRoomType)packet.LeaveBy)
            {
                case LeaveRoomType.SELF:
                    message = $"离开了房间{packet.RoomName}";
                    break;
                case LeaveRoomType.KICK:
                    message = "被房主移除房间";
                    break;
                case LeaveRoomType.DISSOLVE:
                    message = "房间已解散";
                    break;
                case LeaveRoomType.GAME_END:
                    message = "游戏结束";
                    break;
            }
            var ui_toast = UIManager.Get().Push<UI_Toast>();
            ui_toast.Show(message);

            TcpChatClient.m_PlayerManager.LocalPlayer.ResetToLobby();
        }
        void OnGetRoomInfo(object reader)
        {
            var response = (S2C_RoomInfo)reader;
            Debug.Log($"S2C_RoomInfo: [{response.Room.RoomName}#{response.Room.RoomID}]，Count={response.Room.Players.Count}/{response.Room.LimitNum}");

            //别人加入/离开
            var roomData = new BaseRoomData
            {
                RoomID = response.Room.RoomID,
                RoomName = response.Room.RoomName,
                RoomLimit = response.Room.LimitNum,
            };
            for (int i = 0; i < response.Room.Players.Count; i++)
            {
                var playerInfo = response.Room.Players[i];
                var playerData = new BasePlayerData
                {
                    UserName = playerInfo.UserName,
                    NickName = playerInfo.NickName,
                    RoomId = response.Room.RoomID,
                    SeatId = playerInfo.SeatID,
                };
                roomData.Players.Add(playerData);
            }
            TcpChatClient.m_ClientRoom.UpdateData(roomData);

            UpdateUI(roomData);
        }
        void OnGameStart(object reader)
        {
            //TODO: 解包，获取完整比赛信息，座位号和颜色。重连也下发这个消息。
            var packet = (S2C_GameStartPacket)reader;
            //packet.Color; //自己的颜色
            //packet.Cards; //自己的手牌

            UIManager.Get().Pop(this);
            UIManager.Get().Push<UI_Game>();
        }
        #endregion
    }
}