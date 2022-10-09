using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix
{
    public class UI_GameResult : UIBase
    {
        public Text m_ResultText;
        public Button m_ExitBtn;
        public List<Text> m_RankList;

        void Awake()
        {
            m_ResultText = transform.Find("Blue/Headline/Text").GetComponent<Text>();
            m_ExitBtn = transform.Find("ExitBtn").GetComponent<Button>();
            m_ExitBtn.onClick.AddListener(OnExitBtnClick);

            m_RankList = new List<Text>();
            var rankPanel = transform.Find("RankPanel");
            for (int i = 0; i < rankPanel.childCount; i++)
            {
                var child = rankPanel.GetChild(i);
                var script = child.GetComponentInChildren<Text>();
                m_RankList.Add(script);
            }
        }


        static List<string> ColorName = new List<string> { "红色", "黄色", "绿色", "蓝色", "紫色" };
        public void UpdateUI(List<int> rankList)
        {
            var colorRank = TcpChatClient.m_ClientRoom.GetRank(); //5,3,2,4,1

            //dic[座位号] = 排名
            for (int i = 0; i < m_RankList.Count; i++)
            {
                Text txt_item = m_RankList[i];
                var colorIndex = colorRank[i];
                var colorCN = ColorName[colorIndex];
                txt_item.text = $"{colorCN}:---";

                for (int t = 0; t < rankList.Count; t++)
                {
                    int rank = rankList[t];

                    if (rank == i)
                    {
                        var playerData = TcpChatClient.m_ClientRoom.m_PlayerDic[t];
                        txt_item.text = $"{colorCN}:{playerData.NickName}";
                    }
                }
            }
        }

        void OnExitBtnClick()
        {
            MapManager.Instance.Reset();
            UIManager.Get().PopAll();
            UIManager.Get().Push<UI_Login>();
            UIManager.Get().Push<UI_Main>();
        }
    }
}