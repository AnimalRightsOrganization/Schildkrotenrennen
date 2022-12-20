using System.IO;
using UnityEngine;
using LitJson;

namespace Client
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        private static bool Initialized = false;
        public static Present present; //通过请求返回
        private readonly IPC _ipc = new IPC { ReceiveTimeout = 10 };
        public static string Token { get; private set; }

        private Transform sceneRoot;
        public UI_CheckUpdate ui_check;

        void Awake()
        {
#if USE_ASSETBUNDLE
            Debug.Log($"使用热更 Initialized:{Initialized}");
#else
            Debug.Log($"不是热更 Initialized:{Initialized}");
#endif

            if (!Initialized)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                SystemSetting();

                BindAssets();

#if Channel_101 //官方大厅渠道
                //IPC_Login();
#endif

#if UNITY_EDITOR && !USE_ASSETBUNDLE
                Debug.Log("不检查更新");
                present = new Present();
                OnInited();
#else
                // 请求配置（需要启动资源服务器）
                RequestConfig();
#endif
            }
            else
            {
                OnInited();
            }
        }

        void OnApplicationQuit()
        {
            //Debug.Log("Quit");
            Initialized = false;
        }

        // 系统设置
        void SystemSetting()
        {
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = 0.002f; //50fps
            Application.targetFrameRate = 60; //30帧Dotween看起来卡
            QualitySettings.vSyncCount = 0;
            Screen.fullScreen = false;
            Debug.Log($"{Application.platform}: {Screen.width}x{Screen.height}"); //WindowsEditor: 1125x2436
            //Screen.SetResolution(540, 960, false);
            Screen.SetResolution(Screen.width, Screen.height, false);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            //Debug.unityLogger.logEnabled = false; //release版关闭
        }

        // 绑定组件
        void BindAssets()
        {
            if (!Directory.Exists(ConstValue.AB_AppPath))
                Directory.CreateDirectory(ConstValue.AB_AppPath);

            transform.Find("ILGlobal").gameObject.AddComponent<Client.ILGlobal>();
            sceneRoot = GameObject.Find("Canvas").transform;
            Debug.Assert(sceneRoot);

            string ui_name = "UI_CheckUpdate";
            GameObject asset = Resources.Load<GameObject>(ui_name);
            Debug.Assert(asset);

            GameObject obj = Instantiate(asset, sceneRoot);
            Debug.Assert(obj);

            obj.name = ui_name;
            if (obj.GetComponent<UI_CheckUpdate>() == false)
                obj.AddComponent<UI_CheckUpdate>();
            ui_check = obj.GetComponent<UI_CheckUpdate>();
            Debug.Assert(ui_check);
        }

        // 请求游戏配置
        async void RequestConfig()
        {
            string text = await HttpHelper.TryGetAsync(ConstValue.PRESENT_GET);
            Debug.Log($"success: {text}");
            ServerResponse resp = JsonMapper.ToObject<ServerResponse>(text);
            present = JsonMapper.ToObject<Present>(resp.data);

            StartCoroutine(ui_check.StartCheck(OnInited));
        }

        // 初始化完成，控制权移交ILR
        void OnInited()
        {
            Initialized = true;

            ILGlobal.Instance.GlobalInit(); //加载dll
        }

        async void IPC_Login()
        {
            try
            {
                string result = await _ipc.Send("Login 0");
                Token = result;
                Debug.Log($"IPC返回：{result}");
            }
            catch (System.Exception e)
            {
                Debug.Log($"未启动大厅：{e.Message}");
            }
        }
    }
}