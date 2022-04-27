using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace HotFix
{
    public class Item_Chess : UIBase
    {
        public Image mColorImage;
        public Dictionary<string, Sprite> mColorSp;

        public ChessColor mColor;
        public int CurrentIndex; //当前在的格子[0~9]

        void Awake()
        {
            mColorImage = GetComponentInChildren<Image>();
            mColorSp = ResManager.LoadSprite("Sprites/Models");
        }

        public void InitData(int id)
        {
            mColorImage.sprite = mColorSp[$"models_{id}"];

            mColor = (ChessColor)id;
            CurrentIndex = 0;
        }

        //public async Task Move(ChessColor colorKey, int step)
        public void Move(ChessColor colorKey, int step)
        {
            //对出牌、发牌的动画牌不影响
            if (colorKey != mColor)
                Debug.LogError($"{gameObject.name}移动错误，颜色不一致{mColor}:{colorKey}");

            Debug.Log($"{Card.LogColor(colorKey, step)} -> 棋子{colorKey}，走{step}步。");

            int TargetIndex = Mathf.Clamp(CurrentIndex + step, 0, 9);
            if (TargetIndex == CurrentIndex)
            {
                Debug.LogError($"原地，不移动：{CurrentIndex} --> {TargetIndex}");
                return;
            }

            var ui_game = UIManager.Get().GetUI<UI_Game>();
            Vector3 dst = ui_game.m_MapPoints[TargetIndex].position;
            Tweener tw = transform.DOMove(dst, 0.5f);

            CurrentIndex = TargetIndex;

            tw.OnComplete(() =>
            {
                transform.SetParent(ui_game.m_MapPoints[TargetIndex]);
            });
            //await Task.Delay(500);
            //transform.SetParent(ui_game.m_MapPoints[TargetIndex]);
        }
    }
}