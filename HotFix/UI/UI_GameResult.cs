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
                var script = child.GetComponent<Text>();
                m_RankList.Add(script);
            }
        }

        public void UpdateUI(List<int> rankList)
        {
            string rankStr = string.Empty;
            for (int i = 0; i < rankList.Count; i++)
            {
                rankStr += $"{rankList[i]}、";
            }
            Debug.Log($"更新UI：{rankStr}");

            for (int i = 0; i < rankList.Count; i++)
            {
                int seatId = rankList[i]; //排名i --- 座位seatId

                var rankItem = m_RankList[i]; //第i名
                var playerData = TcpChatClient.m_ClientRoom.Players[seatId];
                rankItem.text = playerData.NickName;
            }
        }

        void OnExitBtnClick()
        {
            UIManager.Get().Push<UI_Main>();
            this.Pop();
        }
    }
}