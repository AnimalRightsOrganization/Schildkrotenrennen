using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IMessage = Google.Protobuf.IMessage;

namespace HotFix
{
    public class Item_Room : UIBase
    {
        public Text m_NameText;

        void Awake()
        {
            m_NameText = transform.Find("Text").GetComponent<Text>();
        }

        public void InitUI(string playerName)
        {
            m_NameText.text = playerName;
        }
    }
}