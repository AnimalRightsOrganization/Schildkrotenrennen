using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using kcp2k.Examples;

namespace HotFix
{
    public delegate void SetHandCard(bool value);

    public class UI_Game : UIBase
    {
        private Action CancelAction;
        private Action PlayAction;
        private Action GameEndAction;
        public static Action<bool> onSetHandCard;
        //public static SetHandCard onSetHandCard;
        void SetHandCard(bool value)
        {
            string content = value ? "<color=green>激活</color>" : "<color=red>熄灭</color>";
            Debug.Log($"{content}:下轮是我？{IsMyTurn}----{DateTime.Now.ToString("HH:mm:ss.fff")}");

            for (int i = 0; i < HandCardViews.Count; i++)
            {
                var handCard = HandCardViews[i];
                handCard.SetInteractable(value && IsMyTurn);
            }
        }

        public Transform[] m_Rocks; //地图
        public Item_Turtle[] m_Turtles; //棋子（按颜色索引存、取）

        #region 界面组件
        public Button m_MenuBtn;
        public Dictionary<string, Sprite> idSprites; //身份图集
        public RectTransform NextIcon; //轮次指示
        public Image[] m_Seats; //所有玩家座位
        public Text[] m_SeatNames; //所有玩家名字
        public Image m_MyTurtleColor; //我的身份色卡

        public int selectedCardId; //选中手牌，出列
        public GameObject m_PlayPanel; //选中手牌，出牌或选颜色
        public Button m_CancelBtn; //点击背景，取消选中
        public Button m_PlayBtn; //出牌
        public int selectedCardColor; //彩色龟，所选颜色
        public GameObject m_ColorPanel; //彩色龟，选颜色面板
        public Button[] m_ColorBtns; //彩色龟，选颜色按钮
        public RectTransform m_ColorSelected; //彩色龟，选中框

        public ClientRoom m_Room; //游戏房间（逻辑）
        private ClientPlayer m_LocalPlayer; //缓存数据，方便读取
        public bool IsMyTurn
        {
            get { return m_Room.NextTurn == m_LocalPlayer.SeatId; }
        }
        #endregion

        #region 内置方法
        void Awake()
        {
            //var map_asset = ResManager.LoadPrefab("Prefabs/MapManager");
            //var map_manager = Instantiate(map_asset);
            //map_manager.name = "MapManager";
            //map_manager.AddComponent<MapManager>();
            PoolManager.Get.Spawn("MapManager");
            MapManager.Get.InitAssets();


            m_MenuBtn = transform.Find("MenuBtn").GetComponent<Button>();
            m_MenuBtn.onClick.AddListener(OnMenuBtnClick);

            // 地图
            m_Rocks = MapManager.Get.Rock;
            // 棋子
            m_Turtles = MapManager.Get.Turtle;

            // 成员
            idSprites = ResManager.LoadSprite("Sprites/identify");
            NextIcon = transform.Find("NextIcon").GetComponent<RectTransform>();
            var seatPanel = transform.Find("SeatPanel");
            int length = seatPanel.childCount;
            m_Seats = new Image[length];
            m_SeatNames = new Text[length];
            for (int i = 0; i < length; i++)
            {
                var child = seatPanel.GetChild(i);
                m_Seats[i] = child.GetComponent<Image>();
                m_SeatNames[i] = child.GetComponentInChildren<Text>();
            }
            m_MyTurtleColor = transform.Find("MyTurtleColor").GetComponent<Image>();

            // 卡牌
            InitCardPool();
            handIndex = 0;
            HandSlots = new RectTransform[5];
            HandCardViews = new List<Item_Card>();
            HandSlotRoot = transform.Find("HandSlots");
            for (int i = 0; i < HandSlotRoot.childCount; i++)
            {
                var slot = HandSlotRoot.GetChild(i);
                HandSlots[i] = slot.GetComponent<RectTransform>();

                var handCard = SpawnCard();
                handCard.transform.SetParent(slot);
                handCard.transform.localPosition = Vector3.zero;
                HandCardViews.Add(handCard);
            }

            // 出牌面板
            selectedCardId = 0;
            CancelAction = null;
            PlayAction = null;
            GameEndAction = null;
            m_PlayPanel = transform.Find("PlayPanel").gameObject;
            m_PlayPanel.SetActive(false);
            m_CancelBtn = m_PlayPanel.GetComponent<Button>();
            m_PlayBtn = transform.Find("PlayPanel/PlayBtn").GetComponent<Button>();
            m_CancelBtn.onClick.AddListener(HidePlayPanel);
            m_PlayBtn.onClick.AddListener(OnPlayBtnClick);
            // 彩色选项
            selectedCardColor = 0;
            m_ColorPanel = transform.Find("PlayPanel/ColorPanel").gameObject;
            m_ColorPanel.SetActive(false);
            m_ColorBtns = transform.Find("PlayPanel/ColorPanel").GetComponentsInChildren<Button>();
            m_ColorSelected = transform.Find("PlayPanel/Selected").GetComponent<RectTransform>();
            for (int i = 0; i < m_ColorBtns.Length; i++)
            {
                int index = i;
                Button btn = m_ColorBtns[index];
                btn.onClick.AddListener(() =>
                {
                    //Debug.Log($"Click:{index} Card:{i}");
                    selectedCardColor = index;
                    //Debug.Log($"彩色龟，选颜色{(TurtleColor)selectedCardColor}");
                    m_ColorSelected.SetParent(btn.transform);
                    m_ColorSelected.anchoredPosition = Vector2.zero;
                });
            }
        }
        void OnEnable()
        {
            NetPacketManager.RegisterEvent(OnNetCallback);

            onSetHandCard = SetHandCard;

            NextIcon.SetParent(m_SeatNames[0].transform);
            NextIcon.anchoredPosition = Vector3.zero;
        }
        void OnDisable()
        {
            NetPacketManager.UnRegisterEvent(OnNetCallback);

            NextIcon.SetParent(m_SeatNames[0].transform);
            NextIcon.anchoredPosition = Vector3.zero;
            GetNewCard = false;
        }
        #endregion

        #region 按钮事件
        public void UpdateUI()
        {
            GameEndAction = null;
            m_Room = KcpChatClient.m_ClientRoom;
            m_LocalPlayer = KcpChatClient.m_PlayerManager.LocalPlayer;

            // 绘制成员头像、昵称
            var players = m_Room.Players;
            for (int i = 0; i < m_Seats.Length; i++)
            {
                int index = i;
                var seat = m_Seats[index];
                var seatName = m_SeatNames[index];
                if (index >= players.Count)
                {
                    seat.gameObject.SetActive(false);
                }
                else
                {
                    seat.gameObject.SetActive(true);
                    var player = players[index];
                    seatName.text = player.NickName;
                }
            }

            // 绘制本人身份色卡(红0,黄1,绿2,蓝3,紫4)
            int colorId = (int)m_Room.myTurtleColor;
            m_MyTurtleColor.sprite = idSprites[$"identify_{colorId}"];

            // 绘制手牌
            for (int i = 0; i < 5; i++)
            {
                int index = i;
                var card = m_Room.HandCardDatas[i];
                HandCardViews[i].InitData(card);
                HandCardViews[i].Index = index;
            }

            GetNewCard = false;
        }
        public void ShowPlayPanel(int carcdid, System.Action noAction, System.Action yesAction)
        {
            selectedCardColor = 0;

            selectedCardId = carcdid;
            CancelAction = noAction;
            PlayAction = yesAction;
            m_PlayPanel.SetActive(true);
            Card card = ClientRoom.lib.library[selectedCardId];
            //Debug.Log($"显示：{card.Log()}");

            if (card.cardColor == CardColor.COLOR)
            {
                //Debug.Log($"显示颜色选择");
                for (int i = 0; i < m_ColorBtns.Length; i++)
                {
                    var btn = m_ColorBtns[i];
                    btn.gameObject.SetActive(true);

                    if (i == 0)
                    {
                        m_ColorSelected.SetParent(btn.transform);
                        m_ColorSelected.anchoredPosition = Vector2.zero;
                    }
                }
                m_ColorPanel.SetActive(true);
            }
            else if (card.cardColor == CardColor.SLOWEST)
            {
                List<TurtleColor> slowestArray = m_Room.GetSlowest(); //0~4
                Debug.Log($"显示最慢选择: {slowestArray.Count}, {m_ColorBtns != null}/{m_ColorBtns.Length}");
                for (int i = m_ColorBtns.Length - 1; i >= 0; i--)
                {
                    var btn = m_ColorBtns[i];
                    //Debug.Log($"m_ColorBtns---{i}: {btn != null}");
                    bool available = slowestArray.Contains((TurtleColor)i);
                    btn.gameObject.SetActive(available);

                    if (available)
                    {
                        selectedCardColor = i;
                        m_ColorSelected.SetParent(btn.transform);
                        m_ColorSelected.anchoredPosition = Vector2.zero;
                    }
                }
                m_ColorPanel.SetActive(true);
            }
            else
            {
                m_ColorPanel.SetActive(false);
            }
        }
        public void HidePlayPanel()
        {
            if (m_Room.gameStatus == TurtleAnime.Anime)
            {
                return; //上移动画没播完
            }
            CancelAction?.Invoke();
            m_PlayPanel.SetActive(false);
        }
        // 游戏菜单
        void OnMenuBtnClick()
        {
            var ui_dialog = UIManager.Get().Push<UI_Dialog>();
            ui_dialog.Show("是否退出？",
                () => { ui_dialog.Pop(); }, "否",
                () => { KcpChatClient.SendLeaveRoom(); }, "是");
        }
        // 出牌按钮
        void OnPlayBtnClick()
        {
            Card card = ClientRoom.lib.library[selectedCardId];
            Debug.Log($">>[C2S_GamePlay]>> 我出牌 {card.Log()} 彩色牌？{selectedCardColor}");
            KcpChatClient.SendGamePlay(card.id, selectedCardColor);
        }
        #endregion

        private Transform DeskCards;
        private List<Item_Card> m_CardPool;
        private void InitCardPool()
        {
            //cardPrefab = ResManager.LoadPrefab("Prefabs/Card");
            DeskCards = transform.Find("DeskCards");

            m_CardPool = new List<Item_Card>();
            for (int i = 0; i < 10; i++)
            {
                //var card_obj = Instantiate(cardPrefab, DeskCards);
                var card_obj = PoolManager.Get.Spawn("Card");
                card_obj.transform.SetParent(DeskCards);
                card_obj.transform.localScale = Vector3.one;
                if (card_obj.GetComponent<Item_Card>() == false)
                    card_obj.AddComponent<Item_Card>();
                var script = card_obj.GetComponent<Item_Card>();
                m_CardPool.Add(script);
                card_obj.SetActive(false);
                script.m_Group.alpha = 0;
            }
        }
        private Item_Card SpawnCard()
        {
            Item_Card script = null;
            if (m_CardPool.Count > 0)
            {
                script = m_CardPool[0];
                m_CardPool.Remove(script);
            }
            else
            {
                var card_obj = PoolManager.Get.Spawn("Card");
                card_obj.transform.SetParent(DeskCards);
                card_obj.transform.localScale = Vector3.one;
                //var card_obj = Instantiate(cardPrefab, DeskCards);
                if (card_obj.GetComponent<Item_Card>() == false)
                    card_obj.AddComponent<Item_Card>();
                script = card_obj.GetComponent<Item_Card>();
            }
            script.gameObject.SetActive(true);
            script.m_Group.alpha = 1;
            return script;
        }
        public void DespawnCard(Item_Card card)
        {
            m_CardPool.Add(card);
            card.m_Group.alpha = 0;
            card.transform.SetParent(DeskCards);
            card.transform.position = Vector3.zero;
            card.gameObject.SetActive(false);
        }
        public int handIndex; //选中手牌[0～4]
        public Transform HandSlotRoot;
        public RectTransform[] HandSlots;
        public List<Item_Card> HandCardViews; //我的手牌（实体）
        public void SortHandCards()
        {
            //Debug.Log($"整理手牌：当前手牌数={HandCardViews.Count}");
            for (int i = 0; i < HandSlots.Length; i++)
            {
                var slot = HandSlots[i];
                if (i >= HandCardViews.Count)
                    return;
                var card = HandCardViews[i];
                card.Index = i;
                card.transform.SetParent(slot);
                card.transform.localPosition = Vector3.zero;
                //Debug.Log($"{i}------整理手牌：{card.card.Log()}放到槽{i}中");
            }
        }

        #region 网络事件
        void OnNetCallback(PacketType type, object reader)
        {
            switch (type)
            {
                case PacketType.S2C_YourTurn:
                    OnYourTurn(reader);
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
        void OnYourTurn(object reader)
        {
            var packet = (S2C_NextTurnPacket)reader;
            Debug.Log($"[S2C_YourTurn] 下轮出牌的是 Seat{packet.SeatID}----{DateTime.Now.ToString("HH:mm:ss.fff")}");
            m_Room.NextTurn = packet.SeatID;
            NextIcon.SetParent(m_SeatNames[packet.SeatID].transform);
            NextIcon.anchoredPosition = Vector3.zero;
        }
        // 出牌消息
        async void OnPlay(object reader)
        {
            var packet = (S2C_PlayCardPacket)reader;
            Debug.Log($"<color=yellow><<[S2C_PlayCard] Seat{packet.SeatID} 出 Card{packet.CardID}, 是第{handIndex}张手牌----{DateTime.Now.ToString("HH:mm:ss.fff")}</color>");

            // ①解析牌型
            int colorId = packet.Color;
            Card card = ClientRoom.lib.library[packet.CardID];
            var moveChessList = m_Room.OnGamePlay_Client(packet); //通知逻辑层
            int handCardCount = m_Room.HandCardDatas.Count; //我的手牌数。如果是别人出牌，这里不会减少。

            // ②出牌动画
            // 放大0.2f，移动0.3f，停留1.0f，消失0.5f => 2.0f
            m_Room.SetStatus(TurtleAnime.Anime);//出牌动画开始
            if (packet.SeatID != m_LocalPlayer.SeatId) //别人出牌
            {
                var srcSeat = m_Seats[packet.SeatID].transform;
                Vector3 src = srcSeat.position;
                var otherCard = SpawnCard();
                otherCard.transform.position = src;
                otherCard.m_Group.alpha = 1;
                otherCard.InitData(card);
                otherCard.PlayCardAnime();
            }
            else //自己出牌
            {
                PlayAction?.Invoke(); //也是PlayCardAnime()
                m_PlayPanel.SetActive(false);
            }

            await Task.Delay(2000);

            // ③乌龟移动（0.5s）
            int step = (int)card.cardNum; //走几步
            // 考虑堆叠，颜色是计算得到的数组
            for (int i = 0; i < moveChessList.Count; i++)
            {
                int index = moveChessList[i];
                var chess = m_Turtles[index];
                Debug.Log($"[开始移动]乌龟{index}[{(TurtleColor)index}/{chess.mColor}]。----{DateTime.Now.ToString("HH:mm:ss.fff")}");
                chess.Move((TurtleColor)index, step, i);
            }

            // ④结算面板，等待乌龟移动结束再弹出
            Debug.Log($"OnPlay: {m_Room.gameStatus}----{DateTime.Now.ToString("HH:mm:ss.fff")}");
            if (m_Room.gameStatus == TurtleAnime.End)
            {
                await Task.Delay(1500);
                GameEndAction?.Invoke();
            }

            //⑤发牌动画
            //Debug.Log($"是否播放发牌动画？我的手牌数={handCardCount}");
            if (GetNewCard && handCardCount < 5)
            {
                await Task.Delay(500); //等待出牌和走棋动画
                int index = HandSlots.Length - 1;
                var newCardData = m_Room.HandCardDatas[index];
                Item_Card newCard = SpawnCard();
                newCard.InitData(newCardData);
                newCard.Index = index;
                var slot = HandSlots[index];
                Vector3 dst = slot.transform.position;

                Tweener tw1 = newCard.transform.DOMove(dst, 0.3f);
                tw1.OnComplete(() =>
                {
                    newCard.transform.SetParent(slot);
                    HandCardViews.Add(newCard);
                    GetNewCard = false;
                    newCard.SetInteractable(false);
                });
            }
        }
        // 发牌消息（和出牌消息同时返回，这里做暂存，
        // 动画直接在发牌消息中做，避免定时器误差）
        private bool GetNewCard;
        void OnDeal(object reader)
        {
            var packet = (S2C_DealPacket)reader;

            // ①解析牌型
            Card card = ClientRoom.lib.library[packet.CardID];
            Debug.Log($"<<[S2C_GameDeal]<< 我收到一张新牌：{card.Log()}");
            m_Room.OnGameDeal_Client(card); //存入数据层
            GetNewCard = true;
        }
        // 结算消息
        void OnGameResult(object reader)
        {
            var packet = (S2C_GameResultPacket)reader;
            Debug.Log($"<<[S2C_GameResult]<< {packet.Rank.Count}");

            string logStr = "打印结果：";
            for (int i = 0; i < packet.Rank.Count; i++)
            {
                logStr += $"座位{i}排名{packet.Rank[i]}；";
            }
            Debug.Log(logStr);

            // 等待棋子走完，再弹出。
            m_Room.OnGameResult_Client();
            GameEndAction = () =>
            {
                var ui_result = UIManager.Get().Push<UI_GameResult>();
                ui_result.UpdateUI(packet.Rank);
            };


            var ui_dialog = UIManager.Get().GetUI<UI_Dialog>();
            if (ui_dialog != null)
                GameEndAction?.Invoke();
            ui_dialog?.Pop();
        }
        #endregion
    }
}