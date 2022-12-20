using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace HotFix
{
    public class Item_Card : UIBase
    {
        public Dictionary<string, Sprite> cardArray;
        public CanvasGroup m_Group;
        public Button m_SelfBtn;

        public int Index; //界面中的摆放顺序
        public Card card;
        private Vector3 src;
        private UI_Game ui_game;

        void Awake()
        {
            cardArray = ResManager.LoadSprite("Sprites/cards");
            m_Group = transform.GetComponent<CanvasGroup>();
            m_SelfBtn = transform.Find("Image").GetComponent<Button>();
            m_SelfBtn.onClick.AddListener(OnSelect);
        }

        // 使动画牌不可交互
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
            if (ui_game == null)
                ui_game = UIManager.Get.GetUI<UI_Game>();

            string combName = (data.cardColor.ToString().ToLower() + "" + (data.cardNum > 0 ? "+" : "") + (int)data.cardNum);
            //Debug.Log($"初始化新牌Item_Card: {combName}");
            m_SelfBtn.image.sprite = cardArray[combName];
        }

        void OnSelect()
        {
            if (ui_game == null)
                ui_game = UIManager.Get.GetUI<UI_Game>();

            if (ui_game.m_Room.gameStatus == TurtleAnime.Anime)
            {
                Debug.Log("取消动画没播完");
                return;
            }
            if (ui_game.IsMyTurn == false)
            {
                var ui_toast = UIManager.Get.Push<UI_Toast>();
                ui_toast.Show("不是你的回合，请等待");
                return;
            }
            if (ui_game.m_Room.gameStatus == TurtleAnime.End)
            {
                var ui_toast = UIManager.Get.Push<UI_Toast>();
                ui_toast.Show("已经结束嘞");
                return;
            }

            
            // 实例化创建出来的，要在创建完成后获取坐标
            src = transform.position;
            float dest_pos_y = ui_game.HandSlotRoot.position.y;
            m_SelfBtn.interactable = false;
            ui_game.m_Room.SetStatus(TurtleAnime.Anime); //选中牌开始
            Tweener tw_show = transform.DOMoveY(dest_pos_y, 0.2f);
            tw_show.OnComplete(()=>
            {
                ui_game.m_Room.SetStatus(TurtleAnime.Wait); //选中牌结束
                m_SelfBtn.interactable = true;
            });

            ui_game.ShowPlayPanel(card.id, CancelCardAnime, PlayCardAnime);
            ui_game.handIndex = Index;
        }
        void CancelCardAnime()
        {
            ui_game.m_Room.SetStatus(TurtleAnime.Anime);//取消选中牌
            Tweener tw_hide = transform.DOMove(src, 0.3f);
            tw_hide.OnComplete(() =>
            {
                ui_game.m_Room.SetStatus(TurtleAnime.Wait);//取消选中结束
            });
        }
        public async void PlayCardAnime()
        {
            Debug.Log($"[[[出牌动画开始----{DateTime.Now.ToString("HH: mm:ss.fff")}"); //此时不要熄灭
            if (ui_game == null)
                ui_game = UIManager.Get.GetUI<UI_Game>();

            transform.localScale = Vector3.one;
            Tweener tw1 = transform.DOScale(1.1f, 0.2f);
            await Task.Delay(200);

            Vector3 dest_pos = ui_game.transform.position; //屏幕中心
            Tweener tw2 = transform.DOMove(dest_pos, 0.3f);
            await Task.Delay(300);
            Debug.Log($"移动结束]]]----{DateTime.Now.ToString("HH: mm:ss.fff")}");
            UI_Game.onSetHandCard?.Invoke(false); //只有我的手牌，才熄灭
            m_SelfBtn.interactable = true;


            //如果是我出牌，此时整理一遍手牌
            await Task.Delay(1000);
            this.transform.SetParent(null); //移出Slot
            ui_game.HandCardViews.Remove(this); //移出实体列表
            ui_game.SortHandCards(); //整理手牌

            Tweener tw3 = m_Group.DOFade(0, 0.5f);
            await Task.Delay(500);
            transform.localScale = Vector3.one;
            gameObject.SetActive(false);

            // 被Pool回收
            ui_game.DespawnCard(this);
        }

        public void SetInteractable(bool value)
        {
            m_SelfBtn.interactable = value;
        }
    }
}