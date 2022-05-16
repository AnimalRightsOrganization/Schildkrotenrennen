using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using LitJson;

public class DeployEditor : EditorWindow
{
    const int WINDOW_WIDTH = 640;
    const int WINDOW_HEIGHT = 320;

    static string r_app_version = "0.0.0";
    static string r_res_version = "0";
    static string l_app_version = "0.0.0";
    static string l_res_version = "0";

    const string readme = "说明：" +
        "\n[线上版本] 请求服务器查询线上当前运营的应用和资源版本。" +
        "\n[开发版本] 指的是执行完打包，准备上传到服务器的安装包和资源包。" +
        "\n[压缩] 把安装包和资源包打包称 *.zip 文件上传。" +
        "\n[打包] 执行了资源打包+部署资源到模拟的本地和服务器，还需要实际上传包体，并更新SQL记录。" +
        "\n[部署] 通过POST通知PHP执行更新SQL，同时生成一条部署记录。";

    void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Box($"目标平台：{ConstValue.PLATFORM_NAME}");
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("打包", GUILayout.Width(100), GUILayout.Height(40)))
        {
            BuildTarget type = (BuildTarget)System.Enum.Parse(typeof(BuildTarget), ConstValue.PLATFORM_NAME);
            BundleTools.Build_Target(type);
            GetDevVersion();
        }
        GUILayout.BeginVertical(GUILayout.Height(50));
        {
            GUILayout.BeginHorizontal();
            {
                string path = ConstValue.GetDeployRoot;
                GUILayout.Space(10);
                GUILayout.Label("远程:", GUILayout.Width(50));
                GUILayout.TextField(path, GUILayout.Width(380));
                if (GUILayout.Button("打开", GUILayout.Width(60)))
                {
                    //Debug.Log(path.Length);
                    //EditorUtility.RevealInFinder(path + "\\res");
                    System.Diagnostics.Process.Start("explorer", ConstValue.GetDeployRes);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            {
                string path = Application.persistentDataPath.Replace("/", "\\");
                GUILayout.Space(10);
                GUILayout.Label("本地:", GUILayout.Width(50));
                GUILayout.TextField(path, GUILayout.Width(380));
                if (GUILayout.Button("打开", GUILayout.Width(60)))
                {
                    //Debug.Log(path.Length);
                    //EditorUtility.RevealInFinder(path + "\\Unity");
                    System.Diagnostics.Process.Start("explorer", path);
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.Space(10); //左10
        if (GUILayout.Button("线上版本", GUILayout.Width(100)))
        {
            AppInfo();
        }
        GUILayout.Space(10);
        GUILayout.Label("主包:", GUILayout.Width(50));
        GUILayout.TextField(r_app_version);
        GUILayout.Space(10);
        GUILayout.Label("资源:", GUILayout.Width(50));
        GUILayout.TextField(r_res_version);
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("开发版本", GUILayout.Width(100)))
        {
            GetDevVersion();
        }
        GUILayout.Space(10);
        GUILayout.Label("主包:", GUILayout.Width(50));
        GUILayout.TextField(l_app_version);
        GUILayout.Space(10);
        GUILayout.Label("资源:", GUILayout.Width(50));
        GUILayout.TextField(l_res_version);
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("压缩", GUILayout.Width(100)))
        {

        }
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("部署", GUILayout.Width(100)))
        {
            DeployRes();
        }
        GUILayout.Space(10);
        GUILayout.EndHorizontal();


        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.TextArea(readme, GUILayout.Width(620), GUILayout.Height(90));
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
    }

    [MenuItem("Tools/Deploy")]
    static void AddWindow()
    {
        Rect rect = new Rect(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT);
        DeployEditor window = (DeployEditor)GetWindowWithRect(typeof(DeployEditor), rect, true, "部署");
        window.Show();
    }

    static void PackZIP()
    {
    
    }

    // 查询数据库，当前游戏版本
    static async Task<string> GetAppInfo()
    {
        string responseBody = await HttpHelper.TryGetAsync(ConstValue.GAME_DATA);
        var obj = JsonMapper.ToObject<ServerResponse>(responseBody);
        var model = JsonMapper.ToObject<DBApp>(obj.data);
        r_app_version = model.app_version;
        r_res_version = model.res_version;
        return $"主包:{model.app_version}, 资源:{model.res_version}";
    }
    static async void AppInfo()
    {
        await GetAppInfo();
    }
    static void GetDevVersion()
    {
        l_app_version = Application.version;
        string ver_path = $"{Application.dataPath}/res_version.txt";
        l_res_version = File.ReadAllText(ver_path);
    }
    // 部署新版应用、资源
    static async Task<string> PostDeployRes()
    {
        C2S_Deploy data = new C2S_Deploy { app_version = l_app_version, res_version = l_res_version };
        string postJson = JsonMapper.ToJson(data);
        string responseBody = await HttpHelper.TryPostAsync(ConstValue.PRESENT_DEPLOY, postJson);
        return responseBody;
    }
    static async void DeployRes()
    {
        string log = await PostDeployRes();
        Debug.Log(log);
        var obj = JsonMapper.ToObject<ServerResponse>(log);
        Debug.Log($"部署完成: {obj.msg}");
    }
}