using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix
{
    // 外部通过窗口句柄传入登录Token，转圈，验证后直接跳转界面。
    // 如果没有Token，则留在本页面，让用户手动登录。
    public class UI_Login : UIBase
    {
        public Button m_HelpBtn;
        public Button m_QQBtn;
        public Button m_WXBtn;
        public Button m_LoginBtn;
        public Button m_RegisterBtn;

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
            m_LoginBtn.onClick.AddListener(OLoginBtnClick);
            m_RegisterBtn.onClick.AddListener(OnRegisterBtnClick);
        }

        void Start()
        {
            Debug.Log("UI_Login.Start");
            Push.RegisterEvent(OnNetCallback);
            TcpChatClient.Connect();
        }

        void OnDestroy()
        {
            Debug.Log("UI_Login.OnDestroy");
            Push.UnRegisterEvent(OnNetCallback);

            m_HelpBtn.onClick.RemoveListener(OnHelpBtnClick);
            m_QQBtn.onClick.RemoveListener(OnQQBtnClick);
            m_WXBtn.onClick.RemoveListener(OnWXBtnClick);
            m_RegisterBtn.onClick.RemoveListener(OnRegisterBtnClick);

            Debug.Log("释放临时变量");
            m_HelpBtn = null;
            m_QQBtn = null;
            m_WXBtn = null;
            m_RegisterBtn = null;
        }

        public void OnNetCallback(PacketType type)
        {
            switch (type)
            {
                case PacketType.S2C_LoginResult:
                    {
                        Debug.Log("推送成功");
                        UIManager.Get().Push<UI_Main>(); //成功回调中执行
                        Debug.Log("创建UI");
                    }
                    break;
            }
        }

        void OnHelpBtnClick()
        {
            UIManager.Get().Push<UI_Main>();
        }

        void OnQQBtnClick()
        {
            Debug.Log("[Hotfix] QQ登录");
            //TcpChatClient.Connect();
        }

        void OnWXBtnClick()
        {
            Debug.Log("[Hotfix] 微信登录");
            //TcpChatClient.Disconnect();
        }

        void OLoginBtnClick()
        {
            //C2S_Login cmd = new C2S_Login { Username = "lala", Password = "123456" };
            //TcpChatClient.SendAsync(PacketType.C2S_LoginReq, cmd);
            TcpChatClient.SendLogin("lala", "123456");
        }

        void OnRegisterBtnClick()
        {
            UIManager.Get().Push<UI_Register>();
        }
    }
}