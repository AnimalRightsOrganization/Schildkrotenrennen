using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using kcp2k.Examples;

namespace HotFix
{
    public class Item_Turtle : UIBase
    {
        const float GROUND_HEIGHT = 0.25f;
        const float TURTLE_HEIGHT = 1;
        // 底层0.25; 叠1层0.5;
        static float TURTLE_Y(int layer)
        {
            float result = TURTLE_HEIGHT * (layer) + GROUND_HEIGHT;
            return result;
        }

        public TurtleColor mColor;

        private int CurrentPos; //当前所在格子[0～9]
        private bool IsLock; //移动中锁定
        private UI_Game ui_game;
        private Animator m_Animator;

        void Awake()
        {
            m_Animator = GetComponent<Animator>();
        }

        public void InitData(int id)
        {
            mColor = (TurtleColor)id;
            CurrentPos = 0;

            var start_pos = MapManager.START_POS[(int)mColor];
            transform.position = start_pos;

            IsLock = false;
        }

        public Tweener MoveOnce(int step, int layer)
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
                UI_Game.onSetHandCard?.Invoke(true);
                KcpChatClient.m_ClientRoom.SetStatus(TurtleAnime.Wait);
                m_Animator.SetTrigger("Shake");
                return tw;
            }
            CurrentPos = dest_id;
            Debug.Log($"目标格子: {dest_id}/9");

            Vector3 dest_pos = ui_game.m_Rocks[dest_id].position;
            if (dest_id == 0) //退回起点，读取配置
            {
                dest_pos = MapManager.START_POS[(int)mColor];
            }
            else
            {
                List<TurtleColor> dest_turtles = ui_game.m_Room.GridData[dest_id];
                int dest_count = dest_turtles.Count;
                // 移动完成后，我是第几层
                // （IndexOf）不在数组中，会返回-1
                // 如果中间格子有龟，则也返回-1。取到dest_count，使经过时走在最顶上
                int index = dest_turtles.IndexOf(mColor);
                int myLayer = index == -1 ? (dest_count == 0 ? layer : dest_count) : index;
                //Debug.Log($"移动完成后，我是第几层？？myLayer={myLayer}");
                dest_pos.y = TURTLE_Y(myLayer);
            }

            tw = transform.DOMove(dest_pos, 0.5f)
                .OnPlay(() =>
                {
                    IsLock = true;
                    ui_game.m_Room.SetStatus(TurtleAnime.Anime);//乌龟移动开始
                })
                .OnComplete(() =>
                {
                    IsLock = false;
                    ui_game.m_Room.SetStatus(TurtleAnime.Wait);//乌龟移动结束

                    Debug.Log($"{mColor}乌龟移动完成----{System.DateTime.Now.ToString("HH: mm:ss.fff")}");
                    UI_Game.onSetHandCard?.Invoke(true);
                    KcpChatClient.m_ClientRoom.SetStatus(TurtleAnime.Wait);//出牌动画结束
                });
            return tw;
        }
        public void Move(TurtleColor colorKey, int step, int layer)
        {
            if (ui_game == null)
                ui_game = UIManager.Get.GetUI<UI_Game>();

            if (colorKey != mColor)
                Debug.LogError($"{gameObject.name}移动错误，颜色不一致{mColor}:{colorKey}");
            Debug.Log($"[[移动中]]乌龟{Card.LogColor(colorKey, step)}，走{step}步。");

            if (step == 2) //+2
            {
                Debug.Log("移动+2步: 1/2");
                //这里再次注册委托，相当于把Move1里面的委托覆盖了
                var tw = MoveOnce(1, layer)
                    .OnPlay(() =>
                    {
                        IsLock = true;
                    })
                    .OnComplete(() =>
                    {
                        IsLock = false;

                        Debug.Log("移动+2步: 2/2");
                        MoveOnce(1, layer);
                    });
            }
            else //+1, -1
            {
                MoveOnce(step, layer);
            }
        }
    }
}