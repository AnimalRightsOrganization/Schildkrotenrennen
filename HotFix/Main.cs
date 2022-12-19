using kcp2k.Examples;
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
            //Debug.Log($"AddComponent<UI_Login>"); //UIManager做了
        }
        public static void UI_RegisterAdapter(GameObject go)
        {

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

        public static Present present;

        public static void Init(Present p)
        {
            Debug.Log($"控制权转交ILRuntime:{p}");
            present = p;

            Debug.Log("创建对象池");
            var ILGlobal = GameObject.Find("ILGlobal").transform;
            var poolManager = new GameObject("IL_PoolManager");
            poolManager.transform.SetParent(ILGlobal);
            poolManager.AddComponent<PoolManager>();


            GameObject map_manager = PoolManager.Get.Spawn("MapManager");
            map_manager.AddComponent<MapManager>();
            MapManager.Get.InitAssets();
            PoolManager.Get.Despawn(map_manager);


            Debug.Log("创建UI");
            UIManager.Get().Push<UI_Login>();
        }

        public static void Dispose()
        {
            KcpChatClient.Disconnect(); //关闭网络线程
        }

        public static void PushUI()
        {
            UIManager.Get().Push<UI_Login>();
            //UIManager.Get().Push<UI_Room>(); //关闭网络线程
        }
    }
}