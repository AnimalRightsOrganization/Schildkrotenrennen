using System.IO;
using UnityEngine;

public class ConstValue
{
#region AssetBundle
    public const string PATCH_NAME = "Bundles";
    // ab打包源文件
    static string _srcPath;
    public static string srcPath
    {
        get
        {
            if (string.IsNullOrEmpty(_srcPath))
            {
                _srcPath = Path.Combine(Application.dataPath, PATCH_NAME);
            }
            return _srcPath;
        }
    }
    // ab第一次输出（MD5命名后删除）
    static string _outputPath1st;
    public static string outputPath1st
    {
        get
        {
            if (string.IsNullOrEmpty(_outputPath1st))
            {
                _outputPath1st = Path.Combine(UnityDir, PATCH_NAME);
            }
            return _outputPath1st;
        }
    }
    // ab第二次输出（App、Web部署后删除）
    static string _outputPath2nd;
    public static string outputPath2nd
    {
        get
        {
            if (string.IsNullOrEmpty(_outputPath2nd))
            {
                _outputPath2nd = Path.Combine(UnityDir, PLATFORM_NAME);
            }
            return _outputPath2nd;
        }
    }

    // ab包本地下载位置
    static string ab_path;
    public static string AB_AppPath
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
    // ab包远程部署地址
    static string ab_url;
    public static string AB_WebURL
    {
        get
        {
            if (string.IsNullOrEmpty(ab_url))
            {
                ab_url = Path.Combine(Client.GameManager.present.res_url, PLATFORM_NAME);
            }
            return ab_url;
        }
    }

    // Unity工程根目录
    static string _unity_dir;
    public static string UnityDir
    {
        get
        {
            if (string.IsNullOrEmpty(_unity_dir))
            {
                _unity_dir = System.Environment.CurrentDirectory;
            }
            return _unity_dir;
        }
    }
    public static string BuildDir = $"{UnityDir}\\Build";
    // 局域网部署根目录
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
    // 部署，本地目录
    public static string ZipDeploy = $"{UnityDir}\\Deploy";
    #endregion


    #region Application
    public const string APP_NANE = "turtlerace";
    public const string COMPANY_NANE = "moegijinka";

#if UNITY_ANDROID
    public const string PLATFORM_NAME = "Android";
    public static string LocationPath = $"{BuildDir}\\{PLATFORM_NAME}\\{Application.productName}.apk";
#elif UNITY_IOS
    public const string PLATFORM_NAME = "iOS";
    public static string LocationPath = $"{BuildDir}\\{PLATFORM_NAME}\\{Application.productName}.ipa";
#else
    public const string PLATFORM_NAME = "StandaloneWindows64";
    public static string LocationPath =  $"{BuildDir}\\{PLATFORM_NAME}\\{Application.productName}.exe";
#endif
#endregion


#region URL
    public const string API_DOMAIN = "http://restapi.moegijinka.cn"; //使用Http请求
    public const string GAME_DATA = "api/v1/GameCenter/game_data";
    public const string PRESENT_GET = "turtlerace/v1/GetPresent/get";
    public const string PRESENT_DEPLOY = "turtlerace/v1/GetPresent/deploy";
#endregion


#region GameLogic
    static string _replay_folder;
    static string REPLAY_FOLDER
    {
        get
        {
            if (string.IsNullOrEmpty(_replay_folder))
            {
                _replay_folder = $"{Application.persistentDataPath}/Replay";
                if (Directory.Exists(_replay_folder) == false)
                    Directory.CreateDirectory(_replay_folder);
            }
            return _replay_folder;
        }
    }
    static string LOCAL_PLAYER_NAME
    {
        get
        {
            var localPlayer = "";
            string userName = localPlayer == null ? "Unknown" : localPlayer;
            return userName;
        }
    }
    public static string MY_REPLAY_FOLDER => $"{REPLAY_FOLDER}/{LOCAL_PLAYER_NAME}";
    public static string DUMP_FOLDER = $"{Application.persistentDataPath}/Dump";

    public const int DROP_WAIT_TIME = 30; //掉线等待30s，未重连判负
    public const int TOTAL_SECOND = 90; //比赛时间（s）
#endregion
}