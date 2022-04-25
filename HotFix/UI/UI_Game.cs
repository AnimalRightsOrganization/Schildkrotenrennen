using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ET;

namespace HotFix
{
    public class UI_Game : UIBase
    {
        #region 界面组件
        public Dictionary<string, Sprite> dic_sp; //手牌
        public Button m_CloseBtn;
        public Image m_PlayerImage; //本人
        public Image[] m_Seats; //所有玩家座位
        public Transform[] m_MapPoints; //地图
        public Item_Card[] myCards; //手牌
        public Item_Card otherCard; //动画牌（别人出牌，我收到的新牌）
        public Item_Chess[] gameChess; //棋子
        public int handIndex; //选中手牌，数组Id
        public int selectedCardId; //选中手牌，出列
        public int selectedCardColor; //彩色龟，所选颜色
        public GameObject m_PlayPanel; //选中手牌，出牌或选颜色
        public Button m_PlayBtn; //出牌
        public Button m_CancelBtn; //点击背景，取消选中
        System.Action CancelAction;
        System.Action PlayAction;
        public GameObject m_ColorPanel; //彩色龟，选颜色面板
        public Button[] m_ColorBtns; //彩色龟，选颜色按钮
        public RectTransform m_ColorSelected; //彩色龟，选中框
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
                handObj.name = $"HandCard_{i}";
                var handScript = handObj.AddComponent<Item_Card>();
                myCards[i] = handScript;
            }
            var otherHandObj = Instantiate(cardPrefab, transform);
            otherHandObj.name = "OtherCard";
            otherCard = otherHandObj.AddComponent<Item_Card>(); //屏幕中央
            otherCard.m_Rect.anchoredPosition3D = new Vector3(-320, 555, 0);
            otherCard.UnBind();

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

            selectedCardId = 0;
            selectedCardColor = 0;
            CancelAction = null;
            PlayAction = null;
            m_PlayPanel = transform.Find("PlayPanel").gameObject;
            m_PlayPanel.SetActive(false);
            m_PlayBtn = transform.Find("PlayPanel/PlayBtn").GetComponent<Button>();
            m_CancelBtn = m_PlayPanel.GetComponent<Button>();
            m_CancelBtn.onClick.AddListener(HidePlayPanel);
            m_PlayBtn.onClick.AddListener(OnPlayBtnClick);
            m_ColorPanel = transform.Find("PlayPanel/ColorPanel").gameObject;
            m_ColorPanel.SetActive(false);
            m_ColorBtns = transform.Find("PlayPanel/ColorPanel").GetComponentsInChildren<Button>();
            m_ColorSelected = transform.Find("PlayPanel/Selected").GetComponent<RectTransform>();
            for (int i = 0; i < m_ColorBtns.Length; i++)
            {
                int index = i;
                Button btn = m_ColorBtns[i];
                btn.onClick.AddListener(() =>
                {
                    selectedCardColor = index;
                    Debug.Log($"彩色龟，选颜色{(ChessColor)selectedCardColor}");
                    m_ColorSelected.SetParent(btn.transform);
                    m_ColorSelected.anchoredPosition = Vector2.zero;
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
                int index = i;
                var card = TcpChatClient.m_ClientRoom.handCards[i];
                myCards[i].InitData(card);
                myCards[i].Index = index;
            }
        }
        void OnCloseBtnClick()
        {
            Debug.Log("退出游戏");
            TcpChatClient.SendLeaveRoom();
        }
        public void ShowPlayPanel(int carcdid, System.Action noAction, System.Action yesAction)
        {
            //Debug.Log($"ShowPlayPanel: id={carcdid}");
            selectedCardId = carcdid;
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
            //CancelAction = null;
            m_PlayPanel.SetActive(false);
        }
        void OnPlayBtnClick()
        {
            Debug.Log($"点击出牌：{selectedCardId}");
            Card card = ClientRoom.lib.library[selectedCardId];
            //Debug.Log($"出牌：{card.Log()}");
            TcpChatClient.SendPlayCard(card.id, selectedCardColor);
        }
        #endregion

        #region 网络事件
        void OnNetCallback(PacketType type, object reader)
        {
            switch (type)
            {
                case PacketType.S2C_YourTurn:
                    OnYourTurn();
                    break;
                case PacketType.S2C_GamePlay:
                    OnPlay(reader);
                    break;
                case PacketType.S2C_GameDeal:
                    OnDeal(reader);
                    break;
                case PacketType.S2C_GameResult:
                    OnGameResult(reader);
                    break;
            }
        }
        // 提示出牌
        void OnYourTurn()
        {
            Debug.Log("[S2C_YourTurn] 收到提示出牌");

        }
        // 出牌消息
        async void OnPlay(object reader)
        {
            var packet = (S2C_PlayCardPacket)reader;
            Debug.Log($"[S2C_GamePlay] 收到出牌消息，座位#{packet.SeatID}出牌{packet.CardID}-{packet.Color}");

            // ①解析牌型
            int colorId = packet.Color;
            Card card = ClientRoom.lib.library[packet.CardID];
            //Debug.Log(card.Log());
            TcpChatClient.m_ClientRoom.handCards.RemoveAt(handIndex);

            // ②出牌动画
            // 放大0.2f，移动0.3f，停留1.0f，消失0.5f => 2.0f
            if (packet.SeatID != TcpChatClient.m_PlayerManager.LocalPlayer.SeatId)
            {
                Debug.Log($"是别人出牌，座位#{packet.SeatID}");

                var srcSeat = m_Seats[packet.SeatID].transform;
                Vector3 src = srcSeat.position;
                otherCard.transform.position = src;
                otherCard.m_Group.alpha = 1;
                otherCard.InitData(card);
                otherCard.PlayCardAnime();
            }
            else
            {
                Debug.Log("是自己出牌，执行委托Item_Card.PlayCardAnime()");
                PlayAction?.Invoke();
                //PlayAction = null;
                m_PlayPanel.SetActive(false);
            }

            await Task.Delay(2000);

            // 如果是彩色，转成实际的颜色
            bool colorful = card.cardColor == CardColor.COLOR || card.cardColor == CardColor.SLOWEST;
            ChessColor colorKey = colorful ? (ChessColor)colorId : (ChessColor)card.cardColor; //哪只乌龟
            int step = (int)card.cardNum; //走几步

            // ③动画控制走棋子
            var chess = gameChess[(int)colorKey];
            chess.Move(colorKey, step);
        }
        // 发牌消息
        async void OnDeal(object reader)
        {
            var packet = (S2C_DealPacket)reader;
            Debug.Log($"[S2C_GameDeal] 收到新的牌: Card:{packet.CardID}, to#{packet.SeatID}");

            // 解析牌型
            Card card = ClientRoom.lib.library[packet.CardID];
            Debug.Log(card.Log());
            TcpChatClient.m_ClientRoom.handCards.Add(card);

            // 发牌动画
            await Task.Delay(3000); //等待出牌和走棋动画
            myCards[handIndex].transform.SetAsLastSibling();
            myCards[handIndex].gameObject.SetActive(true);

            otherCard.InitData(card);
            otherCard.transform.position = new Vector3(Screen.width, Screen.height) / 2;
            otherCard.m_Group.alpha = 1;

            Vector3 dst = myCards[myCards.Length - 1].transform.position; //该位置放一堆牌
            //Debug.Log($"移动到：{dst}");
            Tweener tw1 = otherCard.transform.DOMove(dst, 0.3f);
            await Task.Delay(1000);

            otherCard.m_Group.DOFade(0, 0.5f);
            myCards[handIndex].InitData(card);
            myCards[handIndex].m_Group.alpha = 1;
        }
        // 结算消息
        void OnGameResult(object reader)
        {
            Debug.Log("[S2C_GameResult] 结算消息");
            var packet = (S2C_GameResultPacket)reader;
            var ui_result = UIManager.Get().Push<UI_GameResult>();
            ui_result.UpdateUI(packet.Rank);
        }
        #endregion
    }
}