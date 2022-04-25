using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ET;

namespace HotFix
{
    public class Item_Lobby : UIBase
    {
        public Text m_Name;
        public Button m_SelfBtn;

        public RoomInfo data;

        void Awake()
        {
            //Debug.Log($"{gameObject.name}.Awake"); //Prefab中开始是关闭的，导致Awake执行了两次？
            m_Name = transform.Find("Text").GetComponent<Text>();
            m_SelfBtn = transform.GetComponent<Button>();
            //m_SelfBtn.onClick.RemoveAllListeners();
            m_SelfBtn.onClick.AddListener(OnSelfClick);
        }

        public void UpdateUI(RoomInfo roomInfo)
        {
            data = roomInfo;
            m_Name.text = data.RoomName;
        }

        void OnSelfClick()
        {
            Debug.Log($"加入房间：{data.ToString()}");
        }
    }
}