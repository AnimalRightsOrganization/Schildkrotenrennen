using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//using LiteNetLib;
//using Code.Shared;
//using Code.Client;

public class UI_Main : UIBase
{
    private Coroutine connect;

    [SerializeField] GameObject m_InfoPanel;
    [SerializeField] Button m_ConnectButton;
    [SerializeField] Button m_ToServerButton;
    [SerializeField] Button m_CancelButton;
    [SerializeField] Text m_ConnectInfoText;

    void Awake()
    {
        //组件绑定
        /*
        m_InfoPanel = transform.Find("InfoPanel").gameObject;
        m_ConnectButton = transform.Find("MatchmakerSubPanel/Connect").GetComponent<Button>();
        m_ToServerButton = transform.Find("MatchmakerSubPanel/ToServer").GetComponent<Button>();
        m_ConnectInfoText = transform.Find("InfoPanel/ConnectingPanel/Text").GetComponent<Text>();
        m_CancelButton = transform.Find("InfoPanel/ConnectingPanel/CancelButton").GetComponent<Button>();

        m_InfoPanel.SetActive(false);
        m_ConnectButton.onClick.AddListener(OnStartConnect);
        m_ToServerButton.onClick.AddListener(() =>
        {
            this.Pop();
            SceneManager.LoadScene("Server");
        });
        m_CancelButton.onClick.AddListener(OnCancelConnect);
        */
    }

    void OnEnable()
    {
        //处理网络相关委托事件
        //Client.GetInstance().m_OnPeerConnected_Callback += OnConnected;
        //Client.GetInstance().m_OnPeerDisconnected_Callback += OnDisconnected;
    }

    void OnDisable()
    {
        //if (Client.GetInstance() == null) return;
        //Client.GetInstance().m_OnPeerConnected_Callback -= OnConnected;
        //Client.GetInstance().m_OnPeerDisconnected_Callback -= OnDisconnected;
    }

    IEnumerator ConnectInfo()
    {
        int count = 0;
        string baseStr = "CONNECTING";
        m_ConnectInfoText.text = baseStr;
        while (true)
        {
            string output = baseStr;
            yield return new WaitForSeconds(1);
            count++;
            count %= 4;
            for (int i = 0; i < count; i++)
            {
                output += ".";
            }
            m_ConnectInfoText.text = output;
            Debug.Log(output);
        }
    }

    private void OnStartConnect()
    {
        m_InfoPanel.SetActive(true);
        //Client.GetInstance().ConnectToServer();
        connect = StartCoroutine(ConnectInfo());
    }

    private void OnConnected()
    {
        StopCoroutine(connect);
        m_InfoPanel.SetActive(false);
        //UIManager.Get().Push<UI_Login>();
        this.Pop();
    }

    private void OnDisconnected()
    {
        if (connect != null)
            StopCoroutine(connect);
        m_ConnectInfoText.text = "连接超时，请稍后再试";
    }

    private void OnCancelConnect()
    {
        StopCoroutine(connect);
        m_InfoPanel.SetActive(false);
        //Client.GetInstance().Disconnect();
    }
}