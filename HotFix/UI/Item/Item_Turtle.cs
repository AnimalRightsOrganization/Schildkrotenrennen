using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

namespace HotFix
{
    public class Item_Turtle : UIBase
    {
        const float TURTLE_HEIGHT = 0.25f;
        // 底层0.25; 叠1层0.5;
        static float TURTLE_Y(int layer)
        {
            float result = TURTLE_HEIGHT * (layer + 1);
            return result;
        }

        private MeshRenderer m_Render;
        private Texture2D m_Texture;
        public TurtleColor mColor;

        private int CurrentPos; //当前所在格子[0～9]
        private bool IsLock; //移动中锁定
        private UI_Game ui_game;

        void Awake()
        {
            m_Render = GetComponent<MeshRenderer>();
        }

        public void InitData(int id)
        {
            m_Texture = ResManager.LoadTexture2D($"Sprites/{(TurtleColor)id}");
            m_Render.material.mainTexture = m_Texture;

            mColor = (TurtleColor)id;
            CurrentPos = 0;

            IsLock = false;
        }

        public Tweener MoveOnce(int step)
        {
            Tweener tw = null;
            if (IsLock)
            {
                Debug.LogError("移动中...稍后再试");
                return tw;
            }

            int dest_id = Mathf.Clamp(CurrentPos + step, 0, 9);
            if (dest_id == CurrentPos)
            {
                Debug.Log($"原地不动：{CurrentPos} → {dest_id}");
                return tw;
            }
            CurrentPos = dest_id;

            Vector3 dest_pos = ui_game.m_Rocks[dest_id].position;
            if (dest_id == 0) //退回起点，读取配置
            {
                dest_pos = MapManager.START_POS[(int)mColor];
            }
            else
            {
                List<TurtleColor> dest_turtles = ui_game.m_Room.GridData[dest_id];
                // 移动完成后，我是第几层
                int myLayer = dest_turtles.IndexOf(mColor);
                dest_pos.y = TURTLE_Y(myLayer);
            }

            tw = transform.DOMove(dest_pos, 0.5f);
            tw.OnPlay(() =>
            {
                IsLock = true;
            });
            tw.OnComplete(() =>
            {
                IsLock = false;
            });
            return tw;
        }
        public void Move(TurtleColor colorKey, int step)
        {
            if (ui_game == null)
                ui_game = UIManager.Get().GetUI<UI_Game>();

            if (colorKey != mColor)
                Debug.LogError($"{gameObject.name}移动错误，颜色不一致{mColor}:{colorKey}");
            Debug.Log($"棋子{Card.LogColor(colorKey, step)}，走{step}步。");

            if (step == 2) //+2
            {
                var tw = MoveOnce(1);
                //这里再次注册委托，相当于把Move1里面的委托覆盖了
                tw.OnPlay(() =>
                {
                    IsLock = true;
                });
                tw.OnComplete(() =>
                {
                    IsLock = false;

                    MoveOnce(1);
                });
            }
            else //+1, -1
            {
                MoveOnce(step);
            }
        }
    }
}