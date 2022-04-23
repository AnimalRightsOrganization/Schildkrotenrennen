using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ET;

namespace HotFix
{
    public class UI_Main : UIBase
    {
        #region 界面组件
        private int playerNum;

        public Button m_ListBtn;
        public Button m_CreateButton;
        public Button m_SettingsButton;
        public Button m_ExitButton;

        public GameObject m_CreatePanel;
        public Button m_CloseCreatePanel;
        public InputField m_NameInput;
        public InputField m_KeyInput;
        public Text m_NumText;
        public Button m_LeftBtn;
        public Button m_RightBtn;
        public Button m_ConfirmBtn;

        public GameObject m_ListPanel;
        public Button m_CloseListPanel;
        #endregion

        #region 内置方法
        void Awake()
        {
            playerNum = 2;

            m_ListBtn = transform.Find("ListBtn").GetComponent<Button>();
            m_CreateButton = transform.Find("CreateBtn").GetComponent<Button>();
            m_SettingsButton = transform.Find("SettingsBtn").GetComponent<Button>();
            m_ExitButton = transform.Find("ExitBtn").GetComponent<Button>();
            m_ListBtn.onClick.AddListener(OnListBtnClick);
            m_CreateButton.onClick.AddListener(OnCreateBtnClick);
            m_SettingsButton.onClick.AddListener(OnSettingsBtnClick);
            m_ExitButton.onClick.AddListener(OnExitBtnClick);

            var createTrans = transform.Find("CreatePanel");
            m_CreatePanel = createTrans.gameObject;
            m_CloseCreatePanel = createTrans.Find("CloseBtn").GetComponent<Button>();
            m_NameInput = createTrans.Find("NamePanel/NameInput").GetComponent<InputField>();
            m_KeyInput = createTrans.Find("KeyPanel/KeyInput").GetComponent<InputField>();
            m_NumText = createTrans.Find("NumPanel/Num").GetComponent<Text>();
            m_LeftBtn = createTrans.Find("NumPanel/LeftBtn").GetComponent<Button>();
            m_RightBtn = createTrans.Find("NumPanel/RightBtn").GetComponent<Button>();
            m_ConfirmBtn = createTrans.Find("ConfirmBtn").GetComponent<Button>();
            m_CloseCreatePanel.onClick.AddListener(() => { m_CreatePanel.SetActive(false); });
            m_LeftBtn.onClick.AddListener(OnLeftBtnClick);
            m_RightBtn.onClick.AddListener(OnRightBtnClick);
            m_ConfirmBtn.onClick.AddListener(OnConfirmBtnClick);
            RefreshPlayerNum();
            m_CreatePanel.SetActive(false);

            m_ListPanel = transform.Find("ListPanel").gameObject;
            m_CloseListPanel = transform.Find("ListPanel/CloseBtn").GetComponent<Button>();
            m_CloseListPanel.onClick.AddListener(() => { m_ListPanel.SetActive(false); });
            m_ListPanel.SetActive(false);
        }

        void Start()
        {
            playerNum = 2;
            RefreshPlayerNum();
            m_CreatePanel.SetActive(false);
            m_ListPanel.SetActive(false);

            NetPacketManager.RegisterEvent(OnNetCallback);
        }

        void OnDestroy()
        {
            NetPacketManager.UnRegisterEvent(OnNetCallback);
        }
        #endregion

        #region 按钮事件
        void OnListBtnClick()
        {
            TcpChatClient.SendGetRoomList();

            //弹出大厅列表
            m_ListPanel.SetActive(true);
        }
        void OnCreateBtnClick()
        {
            m_CreatePanel.SetActive(true);
        }
        void OnSettingsBtnClick()
        {
            UIManager.Get().Push<UI_Settings>();
        }
        void OnExitBtnClick()
        {
            var ui_dialog = UIManager.Get().Push<UI_Dialog>();
            ui_dialog.Show("确定退出吗？",
                () => { ui_dialog.Hide(); }, "取消",
                () => { this.Pop(); ui_dialog.Hide(); }, "确定");
        }

        void OnLeftBtnClick()
        {
            playerNum--;
            if (playerNum < 2)
            {
                playerNum = 5;
            }
            RefreshPlayerNum();
        }
        void OnRightBtnClick()
        {
            playerNum++;
            if (playerNum > 5)
            {
                playerNum = 2;
            }
            RefreshPlayerNum();
        }
        void RefreshPlayerNum()
        {
            m_NumText.text = $"{playerNum}";
            //Debug.Log($"人数={playerNum}");
        }
        void OnConfirmBtnClick()
        {
            bool result = TcpChatClient.SendCreateRoom(m_NameInput.text, m_KeyInput.text, playerNum);
            if (!result)
                return;

            m_CreatePanel.SetActive(false);
        }
        #endregion

        #region 网络事件
        void OnNetCallback(PacketType type, object reader)
        {
            switch (type)
            {
                case PacketType.S2C_RoomInfo:
                    OnGetRoomInfo(type, reader);
                    break;
                case PacketType.S2C_RoomList:
                    OnGetRoomList(type, reader);
                    break;
            }
        }
        void OnGetRoomInfo(PacketType type, object reader)
        {
            S2C_RoomInfo response = (S2C_RoomInfo)reader;
            Debug.Log($"S2C_RoomInfo: [{response.Room.RoomName}#{response.Room.RoomID}]，Count={response.Room.Players.Count}/{response.Room.LimitNum}");

            BaseRoomData roomData = new BaseRoomData
            {
                RoomID = response.Room.RoomID,
                RoomName = response.Room.RoomName,
                //RoomPwd = "",
                RoomLimit = response.Room.LimitNum,
                Players = new List<BasePlayerData>(),
            };
            for (int i = 0; i < response.Room.Players.Count; i++)
            {
                PlayerInfo playerInfo = response.Room.Players[i];
                BasePlayerData bp = new BasePlayerData
                { 
                    UserName = playerInfo.UserName,
                    NickName = playerInfo.NickName, 
                    SeatId = playerInfo.SeatID,
                };
                roomData.Players.Add(bp);
            }
            var ui_room = UIManager.Get().Push<UI_Room>();
            ui_room.InitUI(roomData);
        }
        void OnGetRoomList(PacketType type, object reader)
        {
            S2C_GetRoomList data = (S2C_GetRoomList)reader;
            Debug.Log($"派发房间列表: count={data.Rooms.Count}");
        }
        #endregion
    }
}