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
    public const string CONFIG_URL = "http://192.168.1.101/download/present.json"; //游戏启动首先指向的配置

    /// <summary>
    /// ab包下载地址
    /// </summary>
    private static string dataUrl;
    public static string DataUrl 
    {
        get
        {
            if (string.IsNullOrEmpty(dataUrl))
            {
                dataUrl = Path.Combine(GameManager.gameConfig.ab_url, PLATFORM_NAME);
            }
            return dataUrl;
        }
    }

    /// <summary>
    /// ab包本地保存位置
    /// </summary>
    private static string dataPath;
    public static string DataPath 
    {
        get 
        {
            if (string.IsNullOrEmpty(dataPath)) 
            {
                dataPath = Path.Combine(Application.persistentDataPath, PLATFORM_NAME);
            }
            return dataPath;
        }
    }
}
