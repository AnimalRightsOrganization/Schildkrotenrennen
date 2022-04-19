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

        void OnDestroy()
        {
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

        void OnHelpBtnClick()
        {
            TheMsg cmd = new TheMsg { Name = "lala", Content = "say hello" };
            TcpHelper.SendAsync(PacketType.C2S_LoginReq, cmd);
        }

        void OnQQBtnClick()
        {
            Debug.Log("[Hotfix] QQ登录");

            TcpHelper.Connect();
        }

        void OnWXBtnClick()
        {
            Debug.Log("[Hotfix] 微信登录");

            TcpHelper.Disconnect();
        }

        void OLoginBtnClick()
        {
            UIManager.Get().Push<UI_Main>();
        }

        void OnRegisterBtnClick()
        {
            UIManager.Get().Push<UI_Register>();
        }
    }
}