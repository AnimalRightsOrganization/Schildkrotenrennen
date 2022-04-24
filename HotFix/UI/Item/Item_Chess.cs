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

        public void Move(ChessColor colorKey, int step)
        {
            Debug.Log($"棋子{mColor}/{colorKey}，走{step}步");

            int TargetIndex = Mathf.Clamp(CurrentIndex + step, 0, 9);

            var ui_game = UIManager.Get().GetUI<UI_Game>();
            //Vector3 src = ui_game.m_MapPoints[CurrentIndex].position;
            Vector3 dst = ui_game.m_MapPoints[TargetIndex].position;
            transform.DOMove(dst, 0.5f);

            CurrentIndex = TargetIndex;

            transform.SetParent(ui_game.m_MapPoints[TargetIndex]);
        }
    }
}