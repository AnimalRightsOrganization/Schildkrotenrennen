using UnityEngine;
using UnityEngine.UI;

namespace HotFix
{
    public class UI_Toast : UIBase
    {
        public RectTransform m_Panel;
        public CanvasGroup m_CanvasGroup;
        public Text m_Content;

        //private Tweener tw2;

        void Awake()
        {
            m_Panel = transform.Find("Panel").GetComponent<RectTransform>();
            m_CanvasGroup = m_Panel.GetComponent<CanvasGroup>();
            m_Content = transform.Find("Panel/Label").GetComponent<Text>();

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

        public void Show(string message)
        {
            /*
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

            tw2 = m_CanvasGroup.DOFade(0, 0.2f);
            tw2.SetDelay(0.8f);
            tw2.Play();

            tw2.OnComplete(Reset);
            */
        }
    }
}