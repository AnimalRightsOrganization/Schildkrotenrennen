using System.IO;
using System.Collections;
using UnityEngine;
using LitJson;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private static bool Initialized = false;
    public static Present present; //通过请求返回
    private readonly IPC _ipc = new IPC { ReceiveTimeout = 10 };
    public static string Token { get; private set; }

    void Awake()
    {
        if (!Initialized)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 系统设置
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = 0.002f; //50fps
            Application.targetFrameRate = 60; //30帧Dotween看起来卡
            QualitySettings.vSyncCount = 0;
            Screen.fullScreen = false;
            Screen.SetResolution(540, 960, false);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            //Debug.unityLogger.logEnabled = false; //release版关闭

            // 绑定组件
            transform.Find("ILGlobal").gameObject.AddComponent<Client.ILGlobal>();

#if CHANNEL_1000 //官方大厅渠道
            IPC_Login();
#endif

#if UNITY_EDITOR && !USE_ASSETBUNDLE
            // 不检查更新
            present = new Present();
            OnInited();
#else
            // 加载配置（需要启动资源服务器）
            GetConfig();
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

    // 请求游戏配置
    async void GetConfig()
    {
        string text = await HttpHelper.TryGetAsync(ConstValue.PRESENT_GET);
        Debug.Log($"success: {text}");
        var obj = JsonMapper.ToObject<ServerResponse>(text);
        present = JsonMapper.ToObject<Present>(obj.data);

        StartCoroutine(CheckUpdateAsync(OnInited));
    }
    
    IEnumerator CheckUpdateAsync(System.Action action)
    {
        if (!Directory.Exists(ConstValue.AB_AppPath))
            Directory.CreateDirectory(ConstValue.AB_AppPath);

        Transform root = GameObject.Find("Canvas").transform;
        var request = Resources.LoadAsync<GameObject>("UI_CheckUpdate");
        yield return request;

        var asset = request.asset as GameObject;
        GameObject prefab = Instantiate(asset, root);
        var ui_checkupdate = prefab.AddComponent<UI_CheckUpdate>();

        yield return ui_checkupdate.StartCheck(action);
    }

    // 初始化完成，控制权移交ILR
    void OnInited()
    {
        Initialized = true;

        Client.ILGlobal.Instance.GlobalInit(); //加载dll
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