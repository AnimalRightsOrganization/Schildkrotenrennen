using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix
{
    public class UI_Game : UIBase
    {
        #region 界面组件
        public Dictionary<string, Sprite> dic_sp; //手牌
        public Button m_CloseBtn;

        public Transform[] startPoints; //棋子
        public Transform[] wayPoints; //路径
        public Transform[] handPoints; //手牌

        public Image[] m_Seats; //房间内所有玩家
        public Image m_PlayerImage; //本人
        public Item_Card[] myCards;
        #endregion

        #region 内置方法
        void Awake()
        {
            m_CloseBtn = transform.Find("closeButton").GetComponent<Button>();
            m_CloseBtn.onClick.AddListener(OnCloseBtnClick);

            var startPointsRoot = transform.Find("background/startPoints");
            startPoints = new Transform[startPointsRoot.childCount];
            for (int i = 0; i < startPointsRoot.childCount; i++)
            {
                startPoints[i] = startPointsRoot.GetChild(i);
            }

            var wayPointsRoot = transform.Find("background/wayPoints");
            wayPoints = new Transform[wayPointsRoot.childCount];
            for (int i = 0; i < wayPointsRoot.childCount; i++)
            {
                wayPoints[i] = wayPointsRoot.GetChild(i);
            }

            var handPointsRoot = transform.Find("background/handPoints");
            handPoints = new Transform[handPointsRoot.childCount];
            for (int i = 0; i < handPointsRoot.childCount; i++)
            {
                handPoints[i] = handPointsRoot.GetChild(i);
            }

            m_PlayerImage = transform.Find("chessColor").GetComponent<Image>();

            myCards = new Item_Card[5];
            var cardPrefab = ResManager.LoadPrefab("Prefabs/Card");
            for (int i = 0; i < 5; i++)
            {
                var handObj = Instantiate(cardPrefab, handPoints[i]);
                var handScript = handObj.AddComponent<Item_Card>();
                myCards[i] = handScript;
            }

            var chessPrefab = ResManager.LoadPrefab("Prefabs/Chess");
            for (int i = 0; i < 5; i++)
            {
                var chessObj = Instantiate(chessPrefab, startPoints[i]);
                var chessScript = chessObj.AddComponent<Item_Chess>();
                chessScript.InitData(i);
            }
        }

        void Start()
        {
            NetPacketManager.RegisterEvent(OnNetCallback);
        }

        void OnDestroy()
        {
            NetPacketManager.UnRegisterEvent(OnNetCallback);
        }
        #endregion

        #region 按钮事件
        public void UpdateUI()
        {
            TcpChatClient.m_ClientRoom.PrintRoom();
            dic_sp = ResManager.LoadSprite("Sprites/identify");
            int colorId = (int)TcpChatClient.m_ClientRoom.chessColor;
            m_PlayerImage.sprite = dic_sp[$"identify_{colorId}"]; //红0黄1绿2蓝3紫4
            Debug.Log($"Color={TcpChatClient.m_ClientRoom.chessColor}");

            for (int i = 0; i < 5; i++)
            {
                var card = TcpChatClient.m_ClientRoom.handCards[i];
                myCards[i].InitData(card);
            }
        }
        void OnCloseBtnClick()
        {
            Debug.Log("退出游戏");
        }
        #endregion

        #region 网络事件
        void OnNetCallback(PacketType type, object reader)
        {
            switch (type)
            {
                case PacketType.S2C_GameDeal:
                    OnDeal(type, reader);
                    break;
                case PacketType.S2C_GamePlay:
                    OnPlay(type, reader);
                    break;
                case PacketType.S2C_GameResult:
                    OnMatchResult(type, reader);
                    break;
            }
        }
        // 发牌消息
        void OnDeal(PacketType type, object reader) { }
        // 出牌消息
        void OnPlay(PacketType type, object reader) { }
        // 结算消息
        void OnMatchResult(PacketType type, object reader) { }
        #endregion
    }
}