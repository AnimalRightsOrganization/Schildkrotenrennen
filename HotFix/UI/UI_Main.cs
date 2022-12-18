using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using kcp2k.Examples;

namespace HotFix
{
    public class UI_Main : UIBase
    {
        public static Color FONT_BLACK = new Color(.196f, .196f, .196f);
        public static Color FONT_BLUE = new Color(43 / 255f, 114 / 255f, 143 / 255f, 0.5f);

        #region 界面组件
        private int playerNum;

        public Animator m_EnterAnime;
        public Button m_JoinBtn;
        public Button m_CreateBtn;
        public Button m_SettingsBtn;
        public Button m_ExitBtn;

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
        public Item_Lobby[] m_Rooms;
        public Button m_PrevPageBtn;
        public Button m_NextPageBtn;
        public Text m_PageText;
        public GameObject m_Notice;
        public Transform m_RoomRoot;

        public Text m_NameText;

        private int Page;
        #endregion

        #region 内置方法
        void Awake()
        {
            playerNum = 2;
            m_EnterAnime = transform.Find("Menu").GetComponent<Animator>();
            m_JoinBtn = transform.Find("Menu/JoinBtn").GetComponent<Button>();
            m_CreateBtn = transform.Find("Menu/CreateBtn").GetComponent<Button>();
            m_SettingsBtn = transform.Find("Menu/SettingsBtn").GetComponent<Button>();
            m_ExitBtn = transform.Find("Menu/ExitBtn").GetComponent<Button>();
            m_JoinBtn.onClick.AddListener(OnJoinBtnClick);
            m_CreateBtn.onClick.AddListener(OnCreateBtnClick);
            m_SettingsBtn.onClick.AddListener(OnSettingsBtnClick);
            m_ExitBtn.onClick.AddListener(OnExitBtnClick);

            var createTrans = transform.Find("CreatePanel");
            m_CreatePanel = createTrans.gameObject;
            m_CreatePanel.SetActive(true);
            m_CloseCreatePanel = createTrans.Find("CloseBtn").GetComponent<Button>();
            m_NameInput = createTrans.Find("NameInput").GetComponent<InputField>();
            m_KeyInput = createTrans.Find("KeyInput").GetComponent<InputField>();
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
            m_ListPanel.SetActive(true); //Prefab中开始是关闭的，导致Awake执行了两次？
            m_CloseListPanel = transform.Find("ListPanel/CloseBtn").GetComponent<Button>();
            m_CloseListPanel.onClick.AddListener(() => { m_ListPanel.SetActive(false); });
            m_PrevPageBtn = transform.Find("ListPanel/PrevBtn").GetComponent<Button>();
            m_NextPageBtn = transform.Find("ListPanel/NextBtn").GetComponent<Button>();
            m_PageText = transform.Find("ListPanel/PageText").GetComponent<Text>();
            m_Notice = transform.Find("ListPanel/Blue/Notice").gameObject;
            m_PrevPageBtn.onClick.AddListener(OnPrevBtnClick);
            m_NextPageBtn.onClick.AddListener(OnNextBtnClick);

            m_Rooms = new Item_Lobby[10];
            m_RoomRoot = transform.Find("ListPanel/Root");
            for (int i = 0; i < 10; i++)
            {
                var roomObj = m_RoomRoot.GetChild(i).gameObject;
                var item_room = roomObj.AddComponent<Item_Lobby>();
                m_Rooms[i] = item_room;
            }

            m_NameText = transform.Find("UserPanel/NameText").GetComponent<Text>();
        }
        void OnEnable()
        {
            Debug.Log("UI_Main.OnEnable");
            Enter();
        }
        void Start()
        {
            playerNum = 2;
            RefreshPlayerNum();
            m_CreatePanel.SetActive(false);
            m_ListPanel.SetActive(false);

            NetPacketManager.RegisterEvent(OnNetCallback);

            Enter();
        }
        void OnDestroy()
        {
            NetPacketManager.UnRegisterEvent(OnNetCallback);
        }
        #endregion

        #region 按钮事件
        void Enter()
        {
            m_EnterAnime.SetBool("enter", true);

            m_NameInput.text = string.Empty;
            m_KeyInput.text = string.Empty;

            Page = 1;
            m_PageText.text = $"第{Page}页";
            m_NameText.text = $"用户名:{KcpChatClient.m_PlayerManager.LocalPlayer.UserName}\n昵称:{KcpChatClient.m_PlayerManager.LocalPlayer.NickName}";
        }

        void OnJoinBtnClick()
        {
            KcpChatClient.SendGetRoomList(1);

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
                () =>
                {
                    var login = UIManager.Get().Push<UI_Login>();
                    login.BackToLogin();
                    m_EnterAnime.SetBool("enter", false);
                    this.Pop();
                    ui_dialog.Hide();
                }, "确定");
        }

        void OnLeftBtnClick()
        {
            playerNum--;
            if (playerNum < 2)
                playerNum = 5;
            RefreshPlayerNum();
        }
        void OnRightBtnClick()
        {
            playerNum++;
            if (playerNum > 5)
                playerNum = 2;
            RefreshPlayerNum();
        }
        void RefreshPlayerNum()
        {
            m_NumText.text = $"{playerNum} 人";
        }
        void OnConfirmBtnClick()
        {
            bool result = KcpChatClient.SendCreateRoom(m_NameInput.text, m_KeyInput.text, playerNum);
            //m_NameInput.placeholder.color = result ? FONT_BLUE : Color.red;
            if (!result)
            {
                return;
            }
            m_CreatePanel.SetActive(false);
        }

        void OnPrevBtnClick()
        {
            //Page--;
            //Page = Mathf.Max(1, Page);
            KcpChatClient.SendGetRoomList(Page - 1);
        }
        void OnNextBtnClick()
        {
            KcpChatClient.SendGetRoomList(Page + 1);
        }
        #endregion

        #region 网络事件
        void OnNetCallback(PacketType type, object reader)
        {
            if (gameObject.activeInHierarchy == false)
            {
                //Debug.Log($"{this.name}处于不活动状态，不处理消息");
                return;
            }
            switch (type)
            {
                case PacketType.S2C_RoomInfo: //自己创建/加入
                    OnGetRoomInfo(reader);
                    break;
                case PacketType.S2C_RoomList:
                    OnGetRoomList(reader);
                    break;
            }
        }
        void OnGetRoomInfo(object reader)
        {
            var response = (S2C_RoomInfo)reader;
            Debug.Log($"[UI_Main.获取房间信息(自己创建/加入): [{response.Room.RoomName}#{response.Room.RoomID}密码:{response.Room.Pwd}]，Count={response.Room.Players.Count}/{response.Room.LimitNum}");

            //自己创建/加入
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
                Debug.Log($"创建用户数据--{i}--{playerData.SeatId}");
                roomData.Players.Add(playerData);

                if (playerData.UserName == KcpChatClient.m_PlayerManager.LocalPlayer.UserName)
                {
                    KcpChatClient.m_PlayerManager.LocalPlayer.SetRoomID(roomData.RoomID).SetSeatID(playerData.SeatId).SetStatus(PlayerStatus.ROOM);
                }
            }
            //Debug.Log("新建ClientRoom");
            KcpChatClient.m_ClientRoom = new ClientRoom(roomData);

            //Debug.Log("Push: UI_Room");
            var ui_room = UIManager.Get().Push<UI_Room>();
            //Debug.Log("更新: UI_Room.UpdateUI");
            ui_room.UpdateUI(roomData);

            m_CreatePanel.SetActive(false);
            m_ListPanel.SetActive(false);
            var ui_dialog = UIManager.Get().GetUI<UI_Dialog>();
            ui_dialog?.Hide();
        }
        void OnGetRoomList(object reader)
        {
            var data = (S2C_GetRoomList)reader;
            Page = data.Page == 0 ? Page : data.Page;
            Debug.Log($"获得房间列表: count={data.Rooms.Count}，Page={data.Page}");

            //count=0，Page=0
            m_Notice.SetActive(data.Rooms.Count == 0);
            m_RoomRoot.gameObject.SetActive(data.Rooms.Count > 0);

            //==0时不刷新。
            if (data.Page > 0)
            {
                m_PageText.text = $"第{Page}页";

                for (int i = 0; i < m_Rooms.Length; i++)
                {
                    var item_room = m_Rooms[i];
                    if (i >= data.Rooms.Count)
                    {
                        item_room.gameObject.SetActive(false);
                    }
                    else
                    {
                        var roomData = data.Rooms[i];
                        item_room.gameObject.SetActive(true);
                        item_room.UpdateUI(roomData);
                    }
                }
            }
        }
        #endregion
    }
}