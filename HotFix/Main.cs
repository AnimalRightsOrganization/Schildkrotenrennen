using System;
using System.Threading.Tasks;
using UnityEngine;
using kcp2k.Examples;

namespace HotFix
{
    public class Main
    {
        // Manager注册在这里。给主工程调用。
        public static void UIManagerAdapter(GameObject go)
        {
            go.AddComponent<UIManager>();
        }
        public static void EventManagerAdapter(GameObject go)
        {
            go.AddComponent<EventManager>();
        }
        public static void PoolManagerAdapter(GameObject go)
        {
            go.AddComponent<PoolManager>();
        }

        public static Present present;
        static UI_Loading ui_loading;

        static void IL_InitAdapter<T>() where T : MonoBehaviour
        {
            //string adapterName = typeof(T).ToString();
            string adapterName = typeof(T).Name;
            GameObject obj = new GameObject($"IL_{adapterName}");
            obj.transform.SetParent(Client.ILGlobal.Instance.transform);
            obj.AddComponent<T>();
            var script = obj.AddComponent<Client.ILMonoBehaviour>();
            script.className = adapterName;
            script.Run();
        }
        public static void Init(Present p)
        {
            //IL_InitAdapter<UIManager>();
            //IL_InitAdapter<EventManager>();
            //IL_InitAdapter<PoolManager>();

            Debug.Log($"控制权转交ILRuntime:{p.ToString()}");
            present = p;

            // 检查App版本
            var remote = new Version(present.app_version);
            var local = new Version(Application.version);
            Debug.Log($"remote:{present.app_version} vs local:{Application.version}");
            if (remote > local)
            {
                var ui_dialog = UIManager.Get.Push<UI_Dialog>();
                ui_dialog.Show("请更新客户端", () =>
                {
                    Application.OpenURL(present.app_url);
                    Application.Quit();
                }, "确定");
                return;
            }

            Debug.Log("创建UI");
            ui_loading = UIManager.Get.Push<UI_Loading>();
            Client.GameManager.Get.ui_check.gameObject.SetActive(false);

            TryConnect();

            int index = 0;
            ui_loading.OnStart(total);
            ui_loading_update = () =>
            {
                index++;
                ui_loading.OnUpdate(index);
            };
            CreatePoolAsync();
        }

        const int total = 11;
        public static Action ui_loading_update;
        static async void CreatePoolAsync()
        {
            //对象池①MapManager,1个
            await PoolManager.Get.SpawnNum<MapManager>("MapManager", 1, ui_loading_update);
            //对象池②Card,10个
            await PoolManager.Get.SpawnNum<Item_Card>("Card", 10, ui_loading_update);
            //对象池回收
            PoolManager.Get.DespawnAll();

            UIManager.Get.Push<UI_Login>();
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
                    var ui_connect = UIManager.Get.GetUI<UI_Connect>();
                    ui_connect?.Pop();
                    break;
                }
            }
        }
        static void ConnectToServer(Action action)
        {
            action?.Invoke();
            KcpChatClient.Connect();
            UIManager.Get.Push<UI_Connect>();

            try_times++;
            Debug.Log($"Connect: {try_times} / 3");
        }

        public static void Print()
        {
            PoolManager.Get.Print();
            //PoolManager.Get.Spawn("MapManager");
        }
    }
}