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

    public static string BuildDir = System.Environment.CurrentDirectory + "\\Build\\";
#if UNITY_ANDROID
    public const string PLATFORM_NAME = "Android";
    public static string LocationPath = BuildDir + $"{Application.productName}.apk";
#elif UNITY_IOS
    public const string PLATFORM_NAME = "iOS";
    public static string LocationPath = BuildDir;
#else
    public const string PLATFORM_NAME = "StandaloneWindows64";
    public static string LocationPath = BuildDir + $"{Application.productName}.exe";
#endif

    //public const string PRESENT_URL = "http://app.moegijinka.cn/turtlerace/present.json"; //（不要文件配置了，使用Http请求）
    public const string API_BASE = "http://restapi.moegijinka.cn";
    public const string GAME_DATA = "api/v1/GameCenter/game_data";
    public const string PRESENT_GET = "turtlerace/v1/GetPresent/get";
    public const string PRESENT_DEPLOY = "turtlerace/v1/GetPresent/deploy";
}