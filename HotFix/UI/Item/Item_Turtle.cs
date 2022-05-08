using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace HotFix
{
    public class Item_Turtle : UIBase
    {
        public MeshRenderer m_Render;
        public Texture2D m_Texture;

        public TurtleColor mColor;
        public int CurrentIndex; //当前在的格子[0~9]

        void Awake()
        {
            m_Render = GetComponent<MeshRenderer>();
        }

        public void InitData(int id)
        {
            m_Texture = ResManager.LoadTexture2D($"Sprites/{(TurtleColor)id}");
            m_Render.material.mainTexture = m_Texture;

            mColor = (TurtleColor)id;
            CurrentIndex = 0;
        }

        public void Move(TurtleColor colorKey, int step)
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
            Vector3 dst = ui_game.m_Rocks[TargetIndex].position;
            Tweener tw = transform.DOMove(dst, 0.5f);

            CurrentIndex = TargetIndex;

            tw.OnComplete(() =>
            {
                transform.SetParent(ui_game.m_Rocks[TargetIndex]);
            });
        }
    }
}