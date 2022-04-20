using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix
{
    public class UI_Room : UIBase
    {
        public Button m_CloseBtn;
        public List<Button> Seats;

        void Awake()
        {
            Seats = new List<Button>();

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

        public void OnNetCallback(PacketType type)
        {
            switch (type)
            {

            }
        }

        void OnCloseBtnClick()
        {
            UIManager.Get().Pop(this);

        }
    }
}