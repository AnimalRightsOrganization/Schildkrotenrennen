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
            m_CloseBtn.onClick.AddListener(OnCloseBtnClick);
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

            }
        }

        public void InitUI(BaseRoomData roomData)
        {
            m_NameText.text = roomData.RoomName;
        }

        void OnCloseBtnClick()
        {
            UIManager.Get().Pop(this);

        }
    }
}