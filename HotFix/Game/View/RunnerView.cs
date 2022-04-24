using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix
{
    public class RunnerView : MonoBehaviour
    {
        public ChessColor mColor;
        public Dictionary<string, Sprite> mColorSp;
        public Image mColorImage;
        public int CurrentWayPoint;

        void Awake()
        {
            mColorImage = GetComponentInChildren<Image>();

            var sp = ResManager.LoadSprite("Sprites/Models");
            mColorSp = sp;
        }

        public void InitData(int id)
        {
            //Debug.Log($"棋子：{id}");

            CurrentWayPoint = 0;
            mColor = (ChessColor)id;
            mColorImage.sprite = mColorSp[""];
        }
    }
}