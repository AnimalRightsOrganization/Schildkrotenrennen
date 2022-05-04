using UnityEngine;

namespace HotFix
{
    public class Main
    {
        #region Adapter

        // 不需要了，主工程不调用UI
        public static void UI_LoginAdapter(GameObject go)
        {
            // UI出现时，会执行这里
        }
        public static void UI_RegisterAdapter(GameObject go)
        {
            //Debug.Log($"AddComponent<UI_Register>");
        }

        // Manager注册在这里。给主工程调用。
        public static void UIManagerAdapter(GameObject go)
        {
            go.AddComponent<UIManager>();
        }
        public static void EventManagerAdapter(GameObject go)
        {
            go.AddComponent<EventManager>();
        }

        #endregion

        public static void Init()
        {
            UIManager.Get().Push<UI_Login>();
            //UIManager.Get().Push<UI_Connect>();
        }

        public static void Dispose()
        {
            TcpChatClient.Dispose(); //关闭网络线程
        }
    }
}