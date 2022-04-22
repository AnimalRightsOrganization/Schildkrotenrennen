using UnityEngine;
using UnityEngine.UI;

namespace HotFix
{
    public class UI_Game : UIBase
    {
        [SerializeField] Transform[] startPoints; //棋子
        [SerializeField] Transform[] wayPoints; //路径
        [SerializeField] Transform[] handPoints; //手牌

        [SerializeField] Transform m_PlayerRoot; //玩家
        [SerializeField] Transform m_RunnerRoot; //棋子
        [SerializeField] Transform m_HandRoot; //手牌

        //[SerializeField] Button m_SettingsButton;

        void Awake()
        {
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
            m_RunnerRoot = transform.Find("runnerRoot");
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

        #region 网络消息
        void OnAllotColor()
        {

        }

        void OnShuffe()
        {

        }

        void OnDeal()
        {

        }

        void OnMatchResult()
        {

        }
        #endregion
    }
}