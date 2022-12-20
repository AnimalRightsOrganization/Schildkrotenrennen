using System;
using System.Collections.Generic;
using UnityEngine;
using kcp2k.Examples;

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
            Debug.Log($"控制权转交ILRuntime:{p.ToString()}");
            present = p;

            Debug.Log("创建对象池.1");
            var ILGlobal = GameObject.Find("ILGlobal").transform;
            var poolManager = new GameObject("IL_PoolManager");
            poolManager.transform.SetParent(ILGlobal);
            if (poolManager.GetComponent<PoolManager>() == false)
                poolManager.AddComponent<PoolManager>();
            //MapManager
            GameObject map_manager = PoolManager.Get.Spawn("MapManager");
            if (map_manager.GetComponent<MapManager>() == false)
                map_manager.AddComponent<MapManager>();
            MapManager.Get.InitAssets();
            PoolManager.Get.Despawn(map_manager);
            //Card
            List<GameObject> card_cache = new List<GameObject>();
            for (int i = 0; i < 10; i++)
            {
                GameObject card_item = PoolManager.Get.Spawn("Card");
                if (card_item.GetComponent<Item_Card>() == false)
                    card_item.AddComponent<Item_Card>();
                card_cache.Add(card_item);
            }
            for (int i = card_cache.Count - 1; i >=0 ; i--)
            {
                GameObject card_item = card_cache[i];
                PoolManager.Get.Despawn(card_item);
                card_cache.RemoveAt(i);
            }


            Debug.Log("创建UI");
            UIManager.Get().Push<UI_Login>();

            // 检查App版本
            var remote = new Version(present.app_version);
            var local = new Version(Application.version);
            Debug.Log($"{present.app_version} vs {Application.version}");
            if (remote > local)
            {
                var ui_dialog = UIManager.Get().Push<UI_Dialog>();
                ui_dialog.Show("请更新客户端", () =>
                {
                    Application.OpenURL(present.app_url);
                    Application.Quit();
                }, "确定");
            }
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