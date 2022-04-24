using UnityEngine;
using UnityEngine.UI;

namespace HotFix
{
    public class UI_Game : UIBase
    {
        #region 界面组件
        public Button m_CloseBtn;

        public Transform[] startPoints; //棋子
        public Transform[] wayPoints; //路径
        public Transform[] handPoints; //手牌

        public Transform m_PlayerRoot; //玩家
        public Transform m_ChessRoot; //棋子
        public Transform m_HandRoot; //手牌
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

            m_PlayerRoot = transform.Find("playerRoot");
            m_ChessRoot = transform.Find("runnerRoot");
            m_HandRoot = transform.Find("handRoot");

            var obj2 = ResManager.LoadPrefab("Prefabs/player");
            Instantiate(obj2, m_PlayerRoot);

            var obj1 = ResManager.LoadPrefab("Prefabs/card");
            for (int i = 0; i < 5; i++)
            {
                var handObj = Instantiate(obj1, handPoints[i]);
            }

            var runnerPrefab = ResManager.LoadPrefab("Prefabs/runner");
            for (int i = 0; i < 5; i++)
            {
                var runnerObj = Instantiate(runnerPrefab, startPoints[i]);
                //var runnerScript = runnerObj.AddComponent<Runner>();
                //runnerScript.InitData(i);
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