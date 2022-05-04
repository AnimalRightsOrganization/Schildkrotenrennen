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
        public GameObject m_LoginPanel;
        public InputField m_UserInput;
        public InputField m_PwdInput;
        public Button m_LoginBtn;

        public GameObject m_RegistPanel;
        public Button m_RegisterBtn;

        public Button m_OAuthBtn;
        #endregion

        #region 内置方法
        void Awake()
        {
            m_LoginPanel = transform.Find("LoginPanel").gameObject;
            m_UserInput = transform.Find("LoginPanel/UserInput").GetComponent<InputField>();
            m_PwdInput = transform.Find("LoginPanel/PwdInput").GetComponent<InputField>();
            m_LoginBtn = transform.Find("LoginPanel/LoginBtn").GetComponent<Button>();

            m_RegistPanel = transform.Find("RegistPanel").gameObject;
            m_RegisterBtn = transform.Find("RegistPanel/RegisterBtn").GetComponent<Button>();

            m_OAuthBtn = transform.Find("OAuthBtn").GetComponent<Button>();

            m_LoginBtn.onClick.AddListener(OnLoginBtnClick);
            m_RegisterBtn.onClick.AddListener(OnRegistBtnClick);
            m_OAuthBtn.onClick.AddListener(OnOAuthBtnClick);
        }

        void Start()
        {
            NetPacketManager.RegisterEvent(OnNetCallback);
            TcpChatClient.Connect();
        }

        void OnDestroy()
        {
            NetPacketManager.UnRegisterEvent(OnNetCallback);
        }
        #endregion

        #region 按钮事件
        void OnLoginBtnClick()
        {
            string username = m_UserInput.text;
            if (string.IsNullOrEmpty(username))
                username = "admin";
            TcpChatClient.SendLogin(username, "123456");
        }
        void OnRegistBtnClick()
        {
            UIManager.Get().Push<UI_Register>();
        }
        void OnOAuthBtnClick()
        {
            //TODO: 判断渠道号。弹出登录或三方View。
            Debug.Log("登录");
            m_LoginPanel.SetActive(true);
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
            this.Pop();
        }
        #endregion
    }
}