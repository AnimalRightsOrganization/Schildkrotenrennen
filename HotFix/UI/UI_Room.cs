using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ET;

namespace HotFix
{
    public class UI_Room : UIBase
    {
        public Text m_NameText;
        public Text m_PwdText;
        public Button m_CloseBtn;
        public Button m_StartBtn;
        //public List<Button> Seats;
        public List<Item_Room> SeatList;

        void Awake()
        {
            Transform seatRoot = transform.Find("SeatPanel/Layout");
            //Seats = new List<Button>();
            SeatList = new List<Item_Room>();
            for (int i = 0; i < seatRoot.childCount; i++)
            {
                var seatItem = seatRoot.GetChild(i).GetComponent<Button>();
                //Seats.Add(seatItem);
                var script = seatItem.gameObject.AddComponent<Item_Room>();
                SeatList.Add(script);
            }

            m_NameText = transform.Find("Background/Text").GetComponent<Text>();
            m_CloseBtn = transform.Find("Background/CloseBtn").GetComponent<Button>();
            m_StartBtn = transform.Find("StartBtn").GetComponent<Button>();
            m_CloseBtn.onClick.AddListener(OnSendLeaveRoom);
            m_StartBtn.onClick.AddListener(OnSendStartGame);
        }

        void Start()
        {
            NetPacketManager.RegisterEvent(OnNetCallback);
        }

        void OnDestroy()
        {
            NetPacketManager.UnRegisterEvent(OnNetCallback);

            m_CloseBtn.onClick.RemoveListener(OnSendLeaveRoom);
            m_StartBtn.onClick.RemoveListener(OnSendStartGame);
            m_CloseBtn = null;
            m_StartBtn = null;
        }

        public void OnNetCallback(PacketType type, object packet)
        {
            switch (type)
            {
                case PacketType.S2C_LeaveRoom:
                    UIManager.Get().Pop(this);
                    break;
                case PacketType.S2C_GameStart:
                    UIManager.Get().Pop(this);
                    UIManager.Get().Push<UI_Game>();
                    break;
            }
        }

        public void InitUI(BaseRoomData roomData)
        {
            m_NameText.text = roomData.RoomName;

            Debug.Log($"房间初始化：ID={roomData.RoomID}，人数={roomData.Players.Count}/{roomData.RoomLimit}");

            // 控制座位总数显示
            for (int i = 0; i < SeatList.Count; i++)
            {
                var scriptItem = SeatList[i];
                if (i >= roomData.RoomLimit)
                {
                    scriptItem.gameObject.SetActive(false);
                }
                else
                {
                    scriptItem.gameObject.SetActive(true);

                    //BasePlayerData playerData = roomData.Players.Find(x => x.SeatId == i); //不能用IList.Find
                    BasePlayerData playerData = null;
                    foreach (var data in roomData.Players)
                    {
                        if (data.SeatId == i) 
                        {
                            playerData = data;
                            break;
                        }
                    }
                    if (playerData != null)
                    {
                        Debug.Log($"playerName={playerData.NickName}");
                        scriptItem.InitUI(playerData.NickName);
                    }
                    else
                    {
                        scriptItem.InitUI("空");
                    }
                }
            }
        }

        void OnSendLeaveRoom()
        {
            Debug.Log("请求离开房间");
            TcpChatClient.SendAsync(PacketType.C2S_LeaveRoom, new Empty());
        }

        void OnSendStartGame()
        {
            Debug.Log("请求开始比赛");
            TcpChatClient.SendAsync(PacketType.C2S_GameStart, new Empty());
        }
    }
}