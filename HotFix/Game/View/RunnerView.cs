using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix
{
    public class RunnerView : MonoBehaviour
    {
        public RunnerColor mColor;
        public Sprite[] mColorSp = new Sprite[5];
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
            mColor = (RunnerColor)id;
            mColorImage.sprite = mColorSp[id];
        }
    }
}