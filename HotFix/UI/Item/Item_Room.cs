using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ET;

namespace HotFix
{
    public class Item_Room : UIBase
    {
        public BasePlayerData playerData;

        public Text m_NameText;
        public Button m_SelfBtn;

        void Awake()
        {
            m_NameText = transform.Find("Text").GetComponent<Text>();
            m_SelfBtn = this.GetComponent<Button>();
            m_SelfBtn.onClick.AddListener(OnSelfClick);
        }

        public void InitUI(BasePlayerData data)
        {
            playerData = data;
            m_NameText.text = playerData == null ? "空" : playerData.NickName;
        }

        void OnSelfClick()
        {
            Debug.Log($"座位上的人是: {(playerData == null ? "空" : playerData.NickName)}");

            if (playerData == null)
            {
                //空，加入机器人
            }
            else
            {
                //自己，没效果
                //if (playerData.UserName == localPlayer.UserName)
                //别人，踢人
                //else
            }
        }
    }
}