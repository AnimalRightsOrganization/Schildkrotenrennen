using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace HotFix
{
    public class Item_Card : UIBase
    {
        public RectTransform m_Rect;
        public CanvasGroup m_Group;
        public Dictionary<string, Sprite> cardArray;
        public Button m_SelfBtn;

        public int Index; //界面中的摆放顺序
        public Card card;
        private Vector3 src;
        private Vector3 dst;

        void Awake()
        {
            m_Rect = transform.GetComponent<RectTransform>();
            m_Group = transform.GetComponent<CanvasGroup>();
            cardArray = ResManager.LoadSprite("Sprites/cards");
            m_SelfBtn = transform.Find("Image").GetComponent<Button>();
            m_SelfBtn.onClick.AddListener(OnSelect);
        }

        public void UnBind()
        {
            m_Group.interactable = false;
            m_Group.blocksRaycasts = false;
            m_SelfBtn.interactable = false;
            m_SelfBtn.onClick.RemoveAllListeners();
        }

        public void InitData(Card data)
        {
            card = data;

            string combName = (data.cardColor.ToString().ToLower() + "" + (data.cardNum > 0 ? "+" : "") + (int)data.cardNum);
            //Debug.Log($"初始化新牌Item_Card: {combName}");
            m_SelfBtn.image.sprite = cardArray[combName];
        }

        void OnSelect()
        {
            var ui_game = UIManager.Get().GetUI<UI_Game>();
            if (ui_game.IsMyTurn == false)
            {
                var ui_toast = UIManager.Get().Push<UI_Toast>();
                ui_toast.Show("不是你的回合，请等待");
                return;
            }
            if (ui_game.m_Room.gameStatus == TurtleAnime.End)
            {
                var ui_toast = UIManager.Get().Push<UI_Toast>();
                ui_toast.Show("已经结束嘞");
                return;
            }
            //Debug.Log($"选中：{card.Log()}");

            // 实例化创建出来的，要在创建完成后获取坐标
            src = transform.position;
            dst = src + Vector3.up * 100;
            m_SelfBtn.interactable = false;
            Tweener tw_show = transform.DOMove(dst, 0.3f);
            tw_show.OnComplete(()=>
            {
                m_SelfBtn.interactable = true;
            });

            ui_game.ShowPlayPanel(card.id, CancelCardAnime, PlayCardAnime);
            ui_game.handIndex = Index;
        }
        void CancelCardAnime()
        {
            transform.DOMove(src, 0.3f);
        }
        public async void PlayCardAnime()
        {
            transform.localScale = Vector3.one;
            Tweener tw1 = transform.DOScale(1.1f, 0.2f);
            await Task.Delay(200);
            //Debug.Log("tw1.等待0.2秒");

            Vector3 dst = new Vector3(Screen.width, Screen.height) / 2; //固定到屏幕中心
            Tweener tw2 = transform.DOMove(dst, 0.3f);
            await Task.Delay(1300);
            //Debug.Log("tw2.等待1.3秒");

            Tweener tw3 = m_Group.DOFade(0, 0.5f);
            await Task.Delay(500);
            //Debug.Log("tw3.等待0.5秒");
            transform.localScale = Vector3.one;
            gameObject.SetActive(false);
        }
    }
}