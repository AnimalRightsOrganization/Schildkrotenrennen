using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ET;

namespace HotFix
{
    public class UI_GameResult : UIBase
    {
        public List<Text> m_RankList;
        public Button m_CloseBtn;

        void Awake()
        {
            m_CloseBtn = transform.Find("CloseBtn").GetComponent<Button>();
            m_CloseBtn.onClick.AddListener(OnCloseBtnClick);
        }

        public void UpdateUI(List<int> rank)
        {

        }

        void OnCloseBtnClick()
        {
            Debug.Log("关闭");
        }
    }
}