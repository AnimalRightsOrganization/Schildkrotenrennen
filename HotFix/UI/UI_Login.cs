using UnityEngine;
using UnityEngine.UI;
using ET;

namespace HotFix
{
    // 外部通过窗口句柄传入登录Token，转圈，验证后直接跳转界面。
    // 如果没有Token，则留在本页面，让用户手动登录。
    public class UI_Login : UIBase
    {
        #region 界面组件
        public Button m_HelpBtn;
        public Button m_QQBtn;
        public Button m_WXBtn;
        public Button m_LoginBtn;
        public Button m_RegisterBtn;
        #endregion

        #region 内置方法
        void Awake()
        {
            m_HelpBtn = transform.Find("HelpButton").GetComponent<Button>();
            m_QQBtn = transform.Find("QQButton").GetComponent<Button>();
            m_WXBtn = transform.Find("WXButton").GetComponent<Button>();
            m_LoginBtn = transform.Find("LoginBtn").GetComponent<Button>();
            m_RegisterBtn = transform.Find("RegisterBtn").GetComponent<Button>();

            m_HelpBtn.onClick.AddListener(OnHelpBtnClick);
            m_QQBtn.onClick.AddListener(OnQQBtnClick);
            m_WXBtn.onClick.AddListener(OnWXBtnClick);
            m_LoginBtn.onClick.AddListener(OnLoginBtnClick);
            m_RegisterBtn.onClick.AddListener(OnRegisterBtnClick);
        }

        void Start()
        {
            NetPacketManager.RegisterEvent(OnNetCallback);
            TcpChatClient.Connect();
        }

        void OnDestroy()
        {
            NetPacketManager.UnRegisterEvent(OnNetCallback);

            m_HelpBtn.onClick.RemoveListener(OnHelpBtnClick);
            m_QQBtn.onClick.RemoveListener(OnQQBtnClick);
            m_WXBtn.onClick.RemoveListener(OnWXBtnClick);
            m_LoginBtn.onClick.RemoveListener(OnLoginBtnClick);
            m_RegisterBtn.onClick.RemoveListener(OnRegisterBtnClick);

            Debug.Log("释放临时变量");
            m_HelpBtn = null;
            m_QQBtn = null;
            m_WXBtn = null;
            m_LoginBtn = null;
            m_RegisterBtn = null;
        }
        #endregion

        #region 按钮事件
        void OnHelpBtnClick()
        {
            //UIManager.Get().Push<UI_Main>();
            //TcpChatClient.Disconnect();
        }
        void OnQQBtnClick()
        {
            Debug.Log("[Hotfix] QQ登录");
        }
        void OnWXBtnClick()
        {
            Debug.Log("[Hotfix] 微信登录");
        }
        void OnLoginBtnClick()
        {
            TcpChatClient.SendLogin("lala", "123456");
        }
        void OnRegisterBtnClick()
        {
            UIManager.Get().Push<UI_Register>();
        }
        #endregion

        #region 网络事件
        void OnNetCallback(PacketType type, object reader)
        {
            switch (type)
            {
                case PacketType.S2C_LoginResult:
                    OnLoginResult(reader);
                    break;
            }
        }
        void OnLoginResult(object reader)
        {
            var packet = (S2C_LoginResultPacket)reader;
            var playerData = new BasePlayerData { UserName = packet.Username }; //没填的都是默认值
            var clientPlayer = new ClientPlayer(playerData);
            clientPlayer.ResetToLobby();
            TcpChatClient.m_PlayerManager.AddClientPlayer(clientPlayer, true);
            UIManager.Get().Push<UI_Main>();
        }
        #endregion
    }
}