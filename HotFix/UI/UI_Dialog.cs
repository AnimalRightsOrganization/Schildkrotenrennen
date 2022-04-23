using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace HotFix
{
    public class UI_Dialog : UIBase
    {
        public RectTransform m_Panel;
        public CanvasGroup m_CanvasGroup;
        public Text m_Content;
        public Button m_NoBtn;
        public Text m_NoBtnDesc;
        public Button m_YesBtn;
        public Text m_YesBtnDesc;

        private Tweener tw2;

        void Awake()
        {
            m_Panel = transform.Find("Panel").GetComponent<RectTransform>();
            m_CanvasGroup = m_Panel.GetComponent<CanvasGroup>();
            m_Content = transform.Find("Panel/Content").GetComponent<Text>();
            m_NoBtn = transform.Find("Panel/NoBtn").GetComponent<Button>();
            m_NoBtnDesc = transform.Find("Panel/NoBtn/Text").GetComponent<Text>();
            m_YesBtn = transform.Find("Panel/YesBtn").GetComponent<Button>();
            m_YesBtnDesc = transform.Find("Panel/YesBtn/Text").GetComponent<Text>();

            m_Panel.anchoredPosition = Vector3.zero;
            Reset();
        }

        void Reset()
        {
            m_Panel.anchoredPosition = new Vector3(0, -200, 0);
            m_CanvasGroup.alpha = 0;
            m_Content.text = string.Empty;
            gameObject.SetActive(false);
        }

        public void Show(string message, Action action1 = null, string btnStr1 = "", Action action2 = null, string btnStr2 = "")
        {
            if (tw2 != null && (tw2.IsActive() || tw2.IsPlaying()))
            {
                Debug.LogError("动画播放中。。。");
                return;
            }

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            m_Content.text = message;
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
