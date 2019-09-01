using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using eeGames.Widget;
using PathologicalGames;

//因为UnityEvent<T0>是抽象类，所以需要声明一个类来继承它
public class MyEvent : UnityEvent<CardAttribute> { }

public class wMainGame : Widget
{
    public static wMainGame Instance;

    private SpawnPool spawnPool;

    public static MyEvent playCardEvent = new MyEvent();
    public UnityAction<CardAttribute> action;

    public int identify;
    [HideInInspector] public Transform[] handPoints = new Transform[5];
    [HideInInspector] public Transform[] startPoints = new Transform[5];
    [HideInInspector] public Transform[] wayPoints = new Transform[10];
    [SerializeField] Button m_closeButton;
    [SerializeField] Button m_startButton;
    [SerializeField] Transform playerRoot;
    [SerializeField] Transform runnerRoot;
    [SerializeField] Transform handRoot;//手牌位置
    [SerializeField] List<Runner> runnerList = new List<Runner>(); //实例化乌龟
    public List<Card> cardList = new List<Card>(); //实例化手牌

    protected override void Awake()
    {
        Instance = this;

        base.Awake();

        spawnPool = PoolManager.Pools["GameAssets"];

        action += Callback;
        playCardEvent.AddListener(action);

        m_closeButton.onClick.AddListener(OnCloseButtonClick);
        m_startButton.onClick.AddListener(OnPlayButtonClick);
    }

    void Start()
    {
        InitGame();
    }

    void OnDestroy()
    {
        m_closeButton.onClick.RemoveListener(OnCloseButtonClick);
        m_startButton.onClick.RemoveListener(OnPlayButtonClick);

        base.DestroyWidget();
    }
    
    private void OnCloseButtonClick()
    {
        WidgetManager.Instance.Pop(this.Id, false);
    }

    private void OnPlayButtonClick()
    {
        Debug.Log("出牌");
    }

    public void Callback(CardAttribute card)
    {
        Debug.Log("出牌：" + card.cardColor + " - " + card.cardNum);

        //TODO:让对应颜色的乌龟走移动 //考虑彩色（可选）和最慢（可选）
        //runnerList.FirstOrDefault(x => x.mColor == card.cardColor);
        if (card.cardColor == CardColor.Slow)
        {
            //找最慢的array(1-5种)
        }
        else if (card.cardColor == CardColor.Color)
        {
            //任意颜色（5种）
        }
        else
        {
            //指定颜色（1种）
        }

        //TODO:再抽一张牌

    }

    public void InitGame()
    {
        //玩家索引
        identify = Random.Range(0, GameManager.Instance.playerList.Count);
        //创建乌龟
        for (int i = 0; i < startPoints.Length; i++)
        {
            Transform obj = spawnPool.Spawn("runner");
            obj.SetParent(runnerRoot);
            obj.position = startPoints[i].position;

            Runner runner = obj.GetComponent<Runner>();
            runner.InitData(i);

            runnerList.Add(runner);
        }
        //创建玩家
        for (int i = 0; i < GameManager.Instance.playerList.Count; i++)
        {
            Transform obj = spawnPool.Spawn("player");
            obj.SetParent(playerRoot);

            PlayerItem item = obj.GetComponent<PlayerItem>();
            item.InitData(i);
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
