using UnityEngine;
using UnityEngine.UI;

namespace HotFix
{
    public class UI_Settings : UIBase
    {
        public Button m_CloseBtn;

        void Awake()
        {
            m_CloseBtn = transform.Find("CloseButton").GetComponent<Button>();
            m_CloseBtn.onClick.AddListener(OnCloseBtnClick);
        }

        void OnCloseBtnClick()
        {
            UIManager.Get().Pop(this);
        }
    }
}