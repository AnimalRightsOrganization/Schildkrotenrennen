using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace Client
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Get;

        private static bool Initialized = false;
        private readonly IPC _ipc = new IPC { ReceiveTimeout = 1000 };
        public static string Token;
        public static Present present; //通过请求返回

        private Transform canvasRoot;
        public UI_CheckUpdate ui_check;

        void Awake()
        {
#if USE_ASSETBUNDLE
            Debug.Log($"渠道:{ConstValue.CHANNEL_NAME}，使用热更，初始化:{Initialized}");
#else
            Debug.Log($"渠道:{ConstValue.CHANNEL_NAME}，不是热更，初始化:{Initialized}");
#endif

            if (!Initialized)
            {
                Get = this;
                DontDestroyOnLoad(gameObject);

                SystemSetting();

                BindAssets();

#if Channel_101 //内测PC，从大厅启动
                IPC_Login();
#endif

                GetConfig();
            }
            else
            {
                OnInited();
            }
        }

        void OnApplicationQuit()
        {
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
            // 初始化目录
            if (!Directory.Exists(ConstValue.AB_AppPath))
                Directory.CreateDirectory(ConstValue.AB_AppPath);

            // 初始化各种管理器
            transform.Find("ILGlobal").gameObject.AddComponent<ILGlobal>();

            // 初始UI
            canvasRoot = GameObject.Find("Canvas").transform;
            //Debug.Assert(canvasRoot);
            string ui_name = "UI_CheckUpdate";
            GameObject asset = Resources.Load<GameObject>(ui_name);
            //Debug.Assert(asset);
            GameObject obj = Instantiate(asset, canvasRoot);
            //Debug.Assert(obj);
            obj.name = ui_name;
            if (obj.GetComponent<UI_CheckUpdate>() == false)
                obj.AddComponent<UI_CheckUpdate>();
            ui_check = obj.GetComponent<UI_CheckUpdate>();
            //Debug.Assert(ui_check);
        }

        // 请求游戏配置
        async void GetConfig()
        {
            string text = await HttpHelper.TryGetAsync(ConstValue.PRESENT_GET);
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogError($"配置请求失败: {ConstValue.PRESENT_GET}");
                return;
            }
            Debug.Log($"success: {text}");
            var obj = JsonConvert.DeserializeObject<ServerResponse>(text);
            present = JsonConvert.DeserializeObject<Present>(obj.data);


#if UNITY_EDITOR && !USE_ASSETBUNDLE
            // 不检查更新
            OnInited();
#else
            // 加载配置（需要启动资源服务器）
            StartCoroutine(ui_check.StartCheck(OnInited));
#endif
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