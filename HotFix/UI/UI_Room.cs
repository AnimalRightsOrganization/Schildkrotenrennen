using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ET;

namespace HotFix
{
    public class UI_Room : UIBase
    {
        public Text m_NameText;
        public Button m_CloseBtn;
        public List<Button> Seats;

        void Awake()
        {
            Seats = new List<Button>();

            m_NameText = transform.Find("Background/Text").GetComponent<Text>();
            m_CloseBtn = transform.Find("Background/CloseBtn").GetComponent<Button>();
            m_CloseBtn.onClick.AddListener(OnSendLeaveRoom);
        }

        void Start()
        {
            NetPacketManager.RegisterEvent(OnNetCallback);
        }

        void OnDestroy()
        {
            NetPacketManager.UnRegisterEvent(OnNetCallback);
        }

        public void OnNetCallback(PacketType type, object packet)
        {
            switch (type)
            {
                case PacketType.S2C_LeaveRoom:
                    UIManager.Get().Pop(this);
                    break;
            }
        }

        public void InitUI(BaseRoomData roomData)
        {
            m_NameText.text = roomData.RoomName;
        }

        void OnSendLeaveRoom()
        {
            Debug.Log("请求离开房间");
            TcpChatClient.SendAsync(PacketType.C2S_LeaveRoom, new Empty());
        }
    }
}