using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ET;

namespace HotFix
{
    public class Item_Lobby : UIBase
    {
        public Text m_Name;
        public Button m_SelfBtn;
        public GameObject m_LockIcon;

        public RoomInfo data;

        void Awake()
        {
            //Debug.Log($"{gameObject.name}.Awake"); //Prefab中开始是关闭的，导致Awake执行了两次？
            m_Name = transform.Find("Text").GetComponent<Text>();
            m_SelfBtn = transform.GetComponent<Button>();
            m_LockIcon = transform.Find("Lock").gameObject;
            //m_SelfBtn.onClick.RemoveAllListeners();
            m_SelfBtn.onClick.AddListener(OnSelfClick);
        }

        public void UpdateUI(RoomInfo roomInfo)
        {
            data = roomInfo;
            m_Name.text = data.RoomName;
            m_LockIcon.SetActive(roomInfo.HasPwd);
        }

        void OnSelfClick()
        {
            Debug.Log($"[C2S] 加入房间：{data.ToString()}");

            if (data.HasPwd)
            {
                // 弹出密码输入框
                var ui_dialog = UIManager.Get().Push<UI_Dialog>();
                ui_dialog.ShowInput("输入密码",
                () =>
                {
                    ui_dialog.Pop();
                }, "取消",
                () =>
                {
                    TcpChatClient.SendJoinRoom(data.RoomID, ui_dialog.m_Input.text);
                    ui_dialog.Pop();
                }, "确定");
                return;
            }

            TcpChatClient.SendJoinRoom(data.RoomID, string.Empty);
        }
    }
}