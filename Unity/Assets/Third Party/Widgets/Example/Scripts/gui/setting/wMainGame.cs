using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using eeGames.Widget;
using PathologicalGames;

public class wMainGame : Widget
{
    private SpawnPool spawnPool;
    [HideInInspector] public Transform[] handPoints = new Transform[5];
    [HideInInspector] public Transform[] startPoints = new Transform[5];
    [HideInInspector] public Transform[] wayPoints = new Transform[10];
    [SerializeField] Button m_closeButton;
    [SerializeField] Button m_startButton;
    [SerializeField] Transform playerRoot;
    [SerializeField] Transform runnerRoot;
    [SerializeField] Transform handRoot;//手牌位置

    protected override void Awake()
    {
        base.Awake();

        spawnPool = PoolManager.Pools["GameAssets"];

        m_closeButton.onClick.AddListener(OnCloseButtonClick);
        m_startButton.onClick.AddListener(OnStartButtonClick);
    }

    void Start()
    {
        InitGame();
    }

    void OnDestroy()
    {
        m_closeButton.onClick.RemoveListener(OnCloseButtonClick);
        m_startButton.onClick.RemoveListener(OnStartButtonClick);

        base.DestroyWidget();
    }
    
    private void OnCloseButtonClick()
    {
        WidgetManager.Instance.Pop(this.Id, false);
    }

    private void OnStartButtonClick()
    {
        Debug.Log("开始游戏");
    }

    public void InitGame()
    {
        //创建乌龟
        for (int i = 0; i < startPoints.Length; i++)
        {
            Transform obj = spawnPool.Spawn("runner");
            obj.SetParent(runnerRoot);
            obj.position = startPoints[i].position;

            Runner runner = obj.GetComponent<Runner>();
            runner.InitData(i);
        }
        //创建玩家
        for (int i = 0; i < GameManager.Instance.playerList.Count; i++)
        {
            Transform obj = spawnPool.Spawn("player");
            obj.SetParent(playerRoot);

            PlayerItem item = obj.GetComponent<PlayerItem>();
            item.InitData();
        }
        //创建手牌
        for (int i = 0; i < handPoints.Length; i++)
        {
            Transform obj = spawnPool.Spawn("card");
            obj.SetParent(handRoot);
            obj.position = handPoints[i].position;

            var data = GameManager.Instance.playerList[0].handCardsList[i];
            Card card = obj.GetComponent<Card>();
            card.InitData(data);
        }
    }
}
