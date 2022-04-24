using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix
{
    public class Item_Chess : UIBase
    {
        public Image mColorImage;
        public Dictionary<string, Sprite> mColorSp;

        public ChessColor mColor;
        public int CurrentWayPoint; //当前在的格子

        void Awake()
        {
            mColorImage = GetComponentInChildren<Image>();

            var sp = ResManager.LoadSprite("Sprites/Models");
            mColorSp = sp;
        }

        public void InitData(int id)
        {
            mColorImage.sprite = mColorSp[$"models_{id}"];

            mColor = (ChessColor)id;
            CurrentWayPoint = 0;
        }
    }
}