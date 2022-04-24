using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ET;

namespace HotFix
{
    public class UI_Game : UIBase
    {
        #region 界面组件
        public Dictionary<string, Sprite> dic_sp; //手牌
        public Button m_CloseBtn;
        public Image m_PlayerImage; //本人
        public Image[] m_Seats; //基础座位
        public Transform[] m_MapPoints; //地图
        public Item_Card[] myCards; //手牌
        public Item_Chess[] gameChess; //棋子
        public int selectedCardId;
        public int selectedCardColor;
        public GameObject m_PlayPanel;
        public Button m_PlayBtn;
        public Button m_CancelBtn;
        System.Action CancelAction;
        System.Action PlayAction;
        public GameObject m_ColorPanel;
        public Button[] m_ColorBtns;
        #endregion

        #region 内置方法
        void Awake()
        {
            dic_sp = ResManager.LoadSprite("Sprites/identify");
            m_CloseBtn = transform.Find("CloseBtn").GetComponent<Button>();
            m_CloseBtn.onClick.AddListener(OnCloseBtnClick);
            m_PlayerImage = transform.Find("ChessColor").GetComponent<Image>();
            m_Seats = transform.Find("SeatPanel").GetComponentsInChildren<Image>();

            // 地图
            var mapRoot = transform.Find("MapPoints");
            m_MapPoints = new Transform[mapRoot.childCount];
            for (int i = 0; i < mapRoot.childCount; i++)
            {
                m_MapPoints[i] = mapRoot.GetChild(i);
            }

            // 手牌
            var handPointsRoot = transform.Find("HandPoints");
            myCards = new Item_Card[5];
            var cardPrefab = ResManager.LoadPrefab("Prefabs/Card");
            for (int i = 0; i < 5; i++)
            {
                var handObj = Instantiate(cardPrefab, handPointsRoot);
                var handScript = handObj.AddComponent<Item_Card>();
                myCards[i] = handScript;
            }

            // 棋子
            gameChess = new Item_Chess[5]; 
            var chessPrefab = ResManager.LoadPrefab("Prefabs/Chess");
            for (int i = 0; i < 5; i++)
            {
                var chessObj = Instantiate(chessPrefab, m_MapPoints[0]);
                var chessScript = chessObj.AddComponent<Item_Chess>();
                gameChess[i] = chessScript;
                chessScript.InitData(i);
            }

            m_PlayPanel = transform.Find("PlayPanel").gameObject;
            m_PlayPanel.SetActive(false);
            m_PlayBtn = transform.Find("PlayPanel/PlayBtn").GetComponent<Button>();
            m_CancelBtn = m_PlayPanel.GetComponent<Button>();
            m_CancelBtn.onClick.AddListener(HidePlayPanel);
            m_PlayBtn.onClick.AddListener(OnPlayBtnClick);
            m_ColorPanel = transform.Find("PlayPanel/ColorPanel").gameObject;
            m_ColorPanel.SetActive(false);
            m_ColorBtns = transform.Find("PlayPanel/ColorPanel").GetComponentsInChildren<Button>();
            for (int i = 0; i < m_ColorBtns.Length; i++)
            {
                int index = i;
                Button btn = m_ColorBtns[i];
                btn.onClick.AddListener(() =>
                {
                    selectedCardColor = index;
                    Debug.Log(index);
                });
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

            for (int i = 0; i < m_Seats.Length; i++)
            {
                var seat = m_Seats[i];
                if (i > TcpChatClient.m_ClientRoom.Players.Count)
                {
                    seat.gameObject.SetActive(false);
                }
                else
                {
                    seat.gameObject.SetActive(true);
                }
            }

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
        public void ShowPlayPanel(int id, System.Action noAction, System.Action yesAction)
        {
            selectedCardId = id;
            CancelAction = noAction;
            PlayAction = yesAction;
            m_PlayPanel.SetActive(true);
            Card card = ClientRoom.lib.library[selectedCardId];
            //Debug.Log($"显示：{card.Log()}");
            if (card.cardColor == CardColor.COLOR ||
                card.cardColor == CardColor.SLOWEST)
            {
                Debug.Log($"<color=yellow>显示颜色选择</color>");
                m_ColorPanel.SetActive(true);
            }
            else
            {
                m_ColorPanel.SetActive(false);
            }
        }
        public void HidePlayPanel()
        {
            CancelAction?.Invoke();
            m_PlayPanel.SetActive(false);
        }
        void OnPlayBtnClick()
        {
            Card card = ClientRoom.lib.library[selectedCardId];
            Debug.Log($"出牌：{card.Log()}");

            var cmd = new C2S_PlayCardPacket
            {
                CardID = card.id,
                Color = selectedCardColor,
            };
            TcpChatClient.SendAsync(PacketType.C2S_GamePlay, cmd);
        }
        #endregion

        #region 网络事件
        void OnNetCallback(PacketType type, object reader)
        {
            switch (type)
            {
                case PacketType.S2C_GameDeal:
                    OnDeal(reader);
                    break;
                case PacketType.S2C_GamePlay:
                    OnPlay(reader);
                    break;
                case PacketType.S2C_GameResult:
                    OnMatchResult(reader);
                    break;
            }
        }
        // 发牌消息
        void OnDeal(object reader)
        {
            
        }
        // 出牌消息
        void OnPlay(object reader)
        {
            var packet = (S2C_PlayCardPacket)reader;
            Debug.Log($"[S2C_GamePlay] 座位#{packet.SeatID}出牌{packet.CardID}-{packet.Color}");

            PlayAction?.Invoke();
            m_PlayPanel.SetActive(false);

            // 解析牌型
            int colorId = packet.Color;
            Card card = ClientRoom.lib.library[packet.CardID];
            // 如果是彩色，转成实际的颜色
            bool colorful = card.cardColor == CardColor.COLOR || card.cardColor == CardColor.SLOWEST;
            ChessColor colorKey = colorful ? (ChessColor)colorId : (ChessColor)card.cardColor; //哪只乌龟
            int step = (int)card.cardNum; //走几步

            // 动画控制走棋子
            var chess = gameChess[(int)card.cardColor];
            chess.Move(colorKey, step);
        }
        // 结算消息
        void OnMatchResult(object reader) { }
        #endregion
    }
}