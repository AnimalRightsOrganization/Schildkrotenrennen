using System;
using System.Threading.Tasks;
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

            Debug.Log("创建UI");
            ui_loading = UIManager.Get().Push<UI_Loading>();
            Client.GameManager.Instance.ui_check.gameObject.SetActive(false);

            TryConnect();

            int index = 0;
            ui_loading.OnStart(total);
            ui_loading_update = () =>
            {
                index++;
                ui_loading.OnUpdate(index);
            };
            CreatePoolAsync();

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

        static UI_Loading ui_loading;

        const int total = 12;
        public static Action ui_loading_update;

        static async void CreatePoolAsync()
        {
            ui_loading_update?.Invoke();

            Debug.Log("创建对象池");
            var ILGlobal = GameObject.Find("ILGlobal").transform;
            var poolManager = new GameObject("IL_PoolManager");
            poolManager.transform.SetParent(ILGlobal);
            if (poolManager.GetComponent<PoolManager>() == false)
                poolManager.AddComponent<PoolManager>();
            await Task.Delay(1);
            ui_loading_update?.Invoke();

            //①MapManager
            //var map_manager = PoolManager.Get.Spawn("MapManager");
            //if (map_manager.GetComponent<MapManager>() == false)
            //    map_manager.AddComponent<MapManager>();
            ////MapManager.Get.InitAssets();
            ////PoolManager.Get.Despawn(map_manager);
            //await Task.Delay(1);
            //ui_loading_update?.Invoke();
            await PoolManager.Get.SpawnNum<MapManager>("MapManager", 1, ui_loading_update);

            //②Card
            //for (int i = 0; i < 10; i++)
            //{
            //    GameObject card_item = PoolManager.Get.Spawn("Card");
            //    if (card_item.GetComponent<Item_Card>() == false)
            //        card_item.AddComponent<Item_Card>();
            //    await Task.Delay(1);
            //    ui_loading_update?.Invoke();
            //}
            await PoolManager.Get.SpawnNum<Item_Card>("Card", 10, ui_loading_update);

            PoolManager.Get.DespawnAll();

            UIManager.Get().Push<UI_Login>();
        }


        static int try_times = 0;
        public static bool trying = false;
        public static async void TryConnect(Action action = null)
        {
            if (KcpChatClient.IsConnected() || trying)
                return; //已连接，或连接中

            trying = true;
            while (true)
            {
                ConnectToServer(action);
                await Task.Delay(3000);

                if (try_times >= 3 || KcpChatClient.IsConnected())
                {
                    Debug.Log($"Finish: {try_times}");
                    trying = false;
                    var ui_connect = UIManager.Get().GetUI<UI_Connect>();
                    ui_connect?.Pop();
                    break;
                }
            }
        }
        static void ConnectToServer(Action action)
        {
            action?.Invoke();
            KcpChatClient.Connect();
            UIManager.Get().Push<UI_Connect>();

            try_times++;
            Debug.Log($"Connect: {try_times} / 3");
        }
    }
}