using System.IO;
using UnityEngine;

public class ConstValue
{
#if UNITY_ANDROID
    public const string PLATFORM_NAME = "Android";
#elif UNITY_IOS
    public const string PLATFORM_NAME = "iOS";
#else
    public const string PLATFORM_NAME = "StandaloneWindows64";
#endif

    //public const string CONFIG_URL = "http://192.168.1.101/download/present.json"; //游戏启动首先指向的配置
    public const string CONFIG_URL = "https://moegijinka.cn/turtle/download/present.json";

    // ab包下载地址
    private static string ab_url;
    public static string AB_URL
    {
        get
        {
            if (string.IsNullOrEmpty(ab_url))
            {
                ab_url = Path.Combine(GameManager.present.ab_url, PLATFORM_NAME);
            }
            return ab_url;
        }
    }

    // ab包本地保存位置
    private static string ab_path;
    public static string AB_FilePath
    {
        get
        {
            if (string.IsNullOrEmpty(ab_path))
            {
                ab_path = Path.Combine(Application.persistentDataPath, PLATFORM_NAME);
            }
            return ab_path;
        }
    }
}