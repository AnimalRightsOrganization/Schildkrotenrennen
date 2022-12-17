using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ET;
using kcp2k.Examples;

namespace HotFix
{
    public class UI_Room : UIBase
    {
        #region 界面组件
        public Text m_NameText;
        public Button m_CloseBtn;
        public Button m_StartBtn;
        public List<Item_Room> SeatList;
        public Button m_PwdBtn;
        public Text m_PwdText;
        #endregion

        #region 内置方法
        void Awake()
        {
            Transform seatRoot = transform.Find("SeatPanel");
            SeatList = new List<Item_Room>();
            for (int i = 0; i < seatRoot.childCount; i++)
            {
                var seatItem = seatRoot.GetChild(i).GetComponent<Button>();
                var script = seatItem.gameObject.AddComponent<Item_Room>();
                SeatList.Add(script);
            }

            m_NameText = transform.Find("Blue/Headline/Text").GetComponent<Text>();
            m_CloseBtn = transform.Find("CloseBtn").GetComponent<Button>();
            m_StartBtn = transform.Find("StartBtn").GetComponent<Button>();
            m_CloseBtn.onClick.AddListener(OnSendLeaveRoom);
            m_StartBtn.onClick.AddListener(OnSendStartGame);

            m_PwdBtn = transform.Find("PwdBtn").GetComponent<Button>();
            m_PwdText = transform.Find("PwdBtn/ActiveMessages/Circle/NumberText").GetComponent<Text>();
            m_PwdBtn.onClick.AddListener(OnPwdBtnClick);
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
            KcpChatClient.SendLeaveRoom();
        }
        void OnSendStartGame()
        {
            KcpChatClient.SendGameStart();
        }
        void OnPwdBtnClick()
        {
            var dialog = UIManager.Get().Push<UI_Dialog>();
            dialog.ShowInput(m_PwdText.text,
            () =>
            {
                Debug.Log("修改密码");
                dialog.Pop();
            }, "修改密码",
            () =>
            {
                Debug.Log("删除密码");
                dialog.Pop();
            }, "删除密码");
        }
        #endregion

        #region 网络事件
        public void UpdateUI(BaseRoomData roomData)
        {
            Debug.Log($"房间初始化：#{roomData.RoomID}，密码：{roomData.RoomPwd}，人数={roomData.Players.Count}/{roomData.RoomLimit}");

            m_NameText.text = roomData.RoomName;
            m_PwdText.text = roomData.RoomPwd;
            m_PwdBtn.gameObject.SetActive(!string.IsNullOrEmpty(roomData.RoomPwd));
            bool isHost = roomData.Players[0].UserName == KcpChatClient.m_PlayerManager.LocalPlayer.UserName;
            m_StartBtn.gameObject.SetActive(isHost);

            for (int i = 0; i < SeatList.Count; i++)
            {
                int index = i;

                var script_seat = SeatList[index];
                if (index >= roomData.RoomLimit)
                {
                    // 控制座位总数显示
                    script_seat.gameObject.SetActive(false);
                }
                else
                {
                    script_seat.gameObject.SetActive(true);

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
                        script_seat.UpdateUI(playerData, index);
                    }
                    else
                    {
                        script_seat.UpdateUI(null, index);
                    }
                }
            }
        }
        void OnNetCallback(PacketType type, object reader)
        {
            if (gameObject.activeInHierarchy == false)
            {
                //Debug.Log($"{this.name}处于不活动状态，不处理消息");
                return;
            }
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
            UIManager.Get().Push<UI_Main>();

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

            KcpChatClient.m_PlayerManager.LocalPlayer.ResetToLobby();
        }
        void OnGetRoomInfo(object reader)
        {
            var response = (S2C_RoomInfo)reader;
            Debug.Log($"[UI_Room.获取房间信息(别人加入/离开): [{response.Room.RoomName}#{response.Room.RoomID}]，Count={response.Room.Players.Count}/{response.Room.LimitNum}");

            //别人加入/离开
            var roomData = new BaseRoomData
            {
                RoomID = response.Room.RoomID,
                RoomName = response.Room.RoomName,
                RoomPwd = response.Room.Pwd,
                RoomLimit = response.Room.LimitNum,
                Players = new List<BasePlayerData>(),
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
                //Debug.Log($"创建用户数据--{i}--");
                roomData.Players.Add(playerData);
            }
            //Debug.Log("更新ClientRoom");
            KcpChatClient.m_ClientRoom.UpdateData(roomData);

            //Debug.Log("更新: UI_Room.UpdateUI");
            UpdateUI(roomData);
        }
        void OnGameStart(object reader)
        {
            var packet = (S2C_GameStartPacket)reader;
            KcpChatClient.m_ClientRoom.OnGameStart_Client(packet);

            UIManager.Get().PopAll();
            var ui_game = UIManager.Get().Push<UI_Game>();
            ui_game.UpdateUI();
        }
        #endregion
    }
}