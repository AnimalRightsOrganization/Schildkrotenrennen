using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Networking;
using LitJson;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private static bool Initialized = false;
    public static Present present; //通过请求返回

    void Awake()
    {
        if (!Initialized)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 系统设置
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = 0.002f;
            Screen.fullScreen = false;
            //Screen.SetResolution(540, 960);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = 60; //30帧Dotween看起来卡
            QualitySettings.vSyncCount = 0;

            // 绑定组件
            transform.Find("ILGlobal").gameObject.AddComponent<Client.ILGlobal>();

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

    class AcceptAllCertificatesSignedWithASpecificPublicKey : CertificateHandler
    {
        // Encoded RSAPublicKey
        private static string PUB_KEY = "30818902818100C4A06B7B52F8D17DC1CCB47362" +
            "C64AB799AAE19E245A7559E9CEEC7D8AA4DF07CB0B21FDFD763C63A313A668FE9D764E" +
            "D913C51A676788DB62AF624F422C2F112C1316922AA5D37823CD9F43D1FC54513D14B2" +
            "9E36991F08A042C42EAAEEE5FE8E2CB10167174A359CEBF6FACC2C9CA933AD403137EE" +
            "2C3F4CBED9460129C72B0203010001";

        protected override bool ValidateCertificate(byte[] certificateData)
        {
            X509Certificate2 certificate = new X509Certificate2(certificateData);

            string pk = certificate.GetPublicKeyString();

            return pk.Equals(PUB_KEY);
        }
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
        if (!Directory.Exists(ConstValue.AB_FilePath))
            Directory.CreateDirectory(ConstValue.AB_FilePath);

        Transform root = GameObject.Find("Canvas").transform;
        var request = Resources.LoadAsync<GameObject>("UI_CheckUpdate");
        yield return request;

        var asset = request.asset as GameObject;
        GameObject prefab = Instantiate(asset, root);
        var script = prefab.AddComponent<UI_CheckUpdate>();

        yield return script.StartCheck(action);
    }

    // 初始化完成，控制权移交ILR
    void OnInited()
    {
        Initialized = true;

        Client.ILGlobal.Instance.GlobalInit(); //加载dll
    }
}