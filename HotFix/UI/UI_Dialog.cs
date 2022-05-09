using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace HotFix
{
    public class UI_Dialog : UIBase
    {
        public RectTransform m_Panel;
        public CanvasGroup m_CanvasGroup;
        public Text m_Message;
        public InputField m_Input;
        public Button m_NoBtn;
        public Text m_NoBtnDesc;
        public Button m_YesBtn;
        public Text m_YesBtnDesc;

        private Tweener tw2;

        void Awake()
        {
            m_Panel = transform.Find("Panel").GetComponent<RectTransform>();
            m_CanvasGroup = m_Panel.GetComponent<CanvasGroup>();
            m_Message = transform.Find("Panel/Message").GetComponent<Text>();
            m_Input = transform.Find("Panel/InputField").GetComponent<InputField>();
            m_NoBtn = transform.Find("Panel/Layout/NoBtn").GetComponent<Button>();
            m_NoBtnDesc = transform.Find("Panel/Layout/NoBtn/Text").GetComponent<Text>();
            m_YesBtn = transform.Find("Panel/Layout/YesBtn").GetComponent<Button>();
            m_YesBtnDesc = transform.Find("Panel/Layout/YesBtn/Text").GetComponent<Text>();

            m_Panel.anchoredPosition = Vector3.zero;
            Reset();
        }

        void Reset()
        {
            m_Panel.anchoredPosition = new Vector3(0, -200, 0);
            m_CanvasGroup.alpha = 0;
            m_Message.text = string.Empty;
            m_Input.text = string.Empty;
            gameObject.SetActive(false);
        }

        public void Show(string message, Action action1 = null, string btnStr1 = "", Action action2 = null, string btnStr2 = "")
        {
            if (tw2 != null && (tw2.IsActive() || tw2.IsPlaying()))
            {
                //Debug.LogError("动画播放中。。。");
                return;
            }

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            m_Input.gameObject.SetActive(false);
            m_Message.gameObject.SetActive(true);
            m_Message.text = message;
            m_CanvasGroup.alpha = 1;
            m_Panel.DOLocalMoveY(0, 0.3f);

            m_NoBtn.gameObject.SetActive(false);
            m_YesBtn.gameObject.SetActive(false);
            if (action1 == null)
            {
                return;
            }
            m_NoBtn.gameObject.SetActive(true);
            m_NoBtn.onClick.RemoveAllListeners();
            m_NoBtn.onClick.AddListener(() => action1());
            m_NoBtnDesc.text = string.IsNullOrEmpty(btnStr1) ? string.Empty : btnStr1;
            if (action2 == null)
            {
                return;
            }
            m_YesBtn.gameObject.SetActive(true);
            m_YesBtn.onClick.RemoveAllListeners();
            m_YesBtn.onClick.AddListener(() => action2());
            m_YesBtnDesc.text = string.IsNullOrEmpty(btnStr2) ? string.Empty : btnStr2;
        }
        public void ShowInput(string message, Action action1 = null, string btnStr1 = "", Action action2 = null, string btnStr2 = "")
        {
            if (tw2 != null && (tw2.IsActive() || tw2.IsPlaying()))
            {
                //Debug.LogError("动画播放中。。。");
                return;
            }

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            m_Message.gameObject.SetActive(false);
            m_Input.gameObject.SetActive(true);
            m_Input.text = message;
            m_CanvasGroup.alpha = 1;
            m_Panel.DOLocalMoveY(0, 0.3f);

            m_NoBtn.gameObject.SetActive(false);
            m_YesBtn.gameObject.SetActive(false);
            if (action1 == null)
            {
                return;
            }
            m_NoBtn.gameObject.SetActive(true);
            m_NoBtn.onClick.RemoveAllListeners();
            m_NoBtn.onClick.AddListener(() => action1());
            m_NoBtnDesc.text = string.IsNullOrEmpty(btnStr1) ? string.Empty : btnStr1;
            if (action2 == null)
            {
                return;
            }
            m_YesBtn.gameObject.SetActive(true);
            m_YesBtn.onClick.RemoveAllListeners();
            m_YesBtn.onClick.AddListener(() => action2());
            m_YesBtnDesc.text = string.IsNullOrEmpty(btnStr2) ? string.Empty : btnStr2;
        }

        public void Hide()
        {
            tw2 = m_CanvasGroup.DOFade(0, 0.2f);
            tw2.Play();
            tw2.OnComplete(Reset);
        }
    }
}