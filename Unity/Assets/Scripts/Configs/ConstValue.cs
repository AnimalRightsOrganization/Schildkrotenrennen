using System.IO;
using UnityEngine;

public class ConstValue
{
    #region 路径
    const string patchName = "Bundles";

    // ab打包源文件
    static string _srcPath;
    public static string srcPath
    {
        get
        {
            if (string.IsNullOrEmpty(_srcPath))
            {
                _srcPath = Path.Combine(Application.dataPath, patchName);
            }
            return _srcPath;
        }
    }
    // ab打包输出（部署后删除）
    static string _outputPath;
    public static string outputPath
    {
        get
        {
            if (string.IsNullOrEmpty(_outputPath))
            {
                _outputPath = Path.Combine(Application.streamingAssetsPath, patchName);
            }
            return _outputPath;
        }
    }
    // ab包远程部署地址
    static string ab_url;
    public static string AB_URL
    {
        get
        {
            if (string.IsNullOrEmpty(ab_url))
            {
                ab_url = Path.Combine(GameManager.present.res_url, PLATFORM_NAME);
            }
            return ab_url;
        }
    }
    // ab包本地下载位置
    static string ab_path;
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

    // Unity工程根目录
    public static string GetUnityDir
    {
        get
        {
            var direction = new DirectoryInfo("Assets");
            return direction.Parent.ToString();
        }
    }
    // 远程部署根目录
    public static string GetDeployRoot
    {
        get
        {
            //注意！！！Windows中，用 '/' 的路径是无法打开的。要用 '\' 。
            string path = $"D:\\wamp64\\www\\app\\{Application.productName}";
            return path;
        }
    }
    public static string GetDeployRes { get { return $"{GetDeployRoot}\\res"; } }
    #endregion

#if UNITY_ANDROID
    public const string PLATFORM_NAME = "Android";
#elif UNITY_IOS
    public const string PLATFORM_NAME = "iOS";
#else
    public const string PLATFORM_NAME = "StandaloneWindows64";
#endif

    public const string PRESENT_URL = "http://app.moegijinka.cn/turtlerace/present.json"; //游戏启动首先指向的配置
}