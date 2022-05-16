using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using LitJson;

public class DeployEditor : EditorWindow
{
    [MenuItem("Tools/Deploy")]
    static void AddWindow()
    {
        Rect rect = new Rect(0, 0, 640, 360);
        DeployEditor window = (DeployEditor)GetWindowWithRect(typeof(DeployEditor), rect, true, "部署");
        window.Show();
    }

    static string r_app_version = string.Empty;
    static string r_res_version = string.Empty;
    static string l_app_version = string.Empty;
    static string l_res_version = string.Empty;

    const string readme = "说明：" +
        "\n[线上版本] 请求服务器查询线上当前运营的应用和资源版本。" +
        "\n[打包版本] 指的是执行完打包，准备上传到服务器的安装包和资源包。" +
        "\n[压缩] 把安装包和资源包打包称 *.zip 文件上传。" +
        "\n[打包] 执行了资源打包+部署资源到模拟的本地和服务器，还需要实际上传包体，并更新SQL记录。" +
        "\n[部署] 通过POST通知PHP执行更新SQL，同时生成一条部署记录。";

    void OnGUI()
    {
        if (string.IsNullOrEmpty(r_app_version))
        {
            GetVersion();
            r_app_version = PlayerPrefs.GetString("r_app_version");
            r_res_version = PlayerPrefs.GetString("r_res_version");
            l_app_version = PlayerPrefs.GetString("l_app_version");
            l_res_version = PlayerPrefs.GetString("l_res_version");
            //Debug.Log("读取PlayerPrefs");
        }

        GUILayout.Space(10);
        GUILayout.Box($"目标平台：{ConstValue.PLATFORM_NAME}");
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("打包资源", GUILayout.Width(100), GUILayout.Height(40)))
        {
            BuildTarget type = (BuildTarget)System.Enum.Parse(typeof(BuildTarget), ConstValue.PLATFORM_NAME);
            BundleTools.Build_Target(type);
            GetVersion();
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
                    System.Diagnostics.Process.Start("explorer", path);
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("打包应用", GUILayout.Width(100)))
        {
            BuildApp();
            return;
        }
        GUILayout.Space(10);
        GUILayout.Label("输出:", GUILayout.Width(50));
        GUILayout.TextField(ConstValue.BuildDir, GUILayout.Width(380));
        GUILayout.EndHorizontal();
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10); //左10
        if (GUILayout.Button("线上版本", GUILayout.Width(100)))
        {
            GetVersion();
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
        if (GUILayout.Button("打包版本", GUILayout.Width(100)))
        {
            GetVersion();
        }
        GUILayout.Space(10);
        GUILayout.Label("主包:", GUILayout.Width(50));
        l_app_version = GUILayout.TextField(l_app_version);
        if (GUILayout.Button("设置", GUILayout.Width(100)))
        {
            SetAppVersion();
        }
        GUILayout.Space(10);
        GUILayout.Label("资源:", GUILayout.Width(50));
        l_res_version = GUILayout.TextField(l_res_version);
        if (GUILayout.Button("设置", GUILayout.Width(100)))
        {
            SetResVersion();
        }
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("压缩", GUILayout.Width(100)))
        {
            PackZIP();
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


        GUILayout.Space(25);
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.TextArea(readme, GUILayout.Width(620), GUILayout.Height(90));
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
    }

    public static void BuildApp()
    {
        BuildTarget buildTarget = (BuildTarget)System.Enum.Parse(typeof(BuildTarget), ConstValue.PLATFORM_NAME);
        if (Directory.Exists(ConstValue.BuildDir))
            Directory.Delete(ConstValue.BuildDir, true);
        Directory.CreateDirectory(ConstValue.BuildDir);

        BuildPlayerOptions opt = new BuildPlayerOptions();
        opt.scenes = new string[] { "Assets/Scenes/Init.unity" };
        opt.locationPathName = ConstValue.LocationPath;
        opt.target = buildTarget;
        opt.options = BuildOptions.None;

        BuildPipeline.BuildPlayer(opt);

        Debug.Log("打包成功!");
    }

    static async void PackZIP()
    {
        //string app_path = Path.Combine(Environment.CurrentDirectory, "Build").Replace("/", "\\");
        string app_path = ConstValue.LocationPath;
        string app_zip = Path.Combine(Environment.CurrentDirectory, $"{Application.productName}.app.zip").Replace("/", "\\");
        Debug.Log($"压缩1：{app_path} --->\n{app_zip}");
        string res_path = Path.Combine(Application.persistentDataPath, ConstValue.PLATFORM_NAME).Replace("/", "\\");
        string res_zip = Path.Combine(Environment.CurrentDirectory, $"{Application.productName}.res.zip").Replace("/", "\\");
        Debug.Log($"压缩2：{res_path} --->\n{res_zip}");

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 0);
        await Task.Delay(100);
        File.Delete(app_zip);

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 1 / 4f);
        await Task.Delay(100);
        ZipEditor.PackFiles(app_zip, app_path);

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 2 / 4f);
        await Task.Delay(100);
        File.Delete(res_zip);

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 3 / 4f);
        await Task.Delay(100);
        ZipEditor.PackFiles(res_zip, res_path);

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 1f);
        await Task.Delay(100);
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("下一步", "处于安全原因，上传资源请通过FTP等其他方式手动实现！", "我知道了");

        // 服务器删除原目录，解压
        // /www/app/{product_name}/app/
        // /www/app/{product_name}/res/
    }

    // 查询数据库，当前游戏版本
    static async Task<string> GetVersionAsync()
    {
        string responseBody = await HttpHelper.TryGetAsync(ConstValue.GAME_DATA);
        var obj = JsonMapper.ToObject<ServerResponse>(responseBody);
        var model = JsonMapper.ToObject<DBApp>(obj.data);
        r_app_version = model.app_version;
        r_res_version = model.res_version;

        l_app_version = PlayerSettings.bundleVersion;
        string ver_path = $"{Application.dataPath}/res_version.txt";
        l_res_version = File.ReadAllText(ver_path);

        PlayerPrefs.SetString("r_app_version", r_app_version);
        PlayerPrefs.SetString("r_res_version", r_res_version);
        PlayerPrefs.SetString("l_app_version", l_app_version);
        PlayerPrefs.SetString("l_res_version", l_res_version);
        //Debug.Log("保存PlayerPrefs");
        return null;
    }
    static async void GetVersion()
    {
        await GetVersionAsync();
    }
    // 部署新版应用、资源
    static async Task<string> PostDeployRes()
    {
        if (l_app_version.Equals(r_app_version) && (l_res_version.Equals(r_res_version)))
        {
            Debug.LogError("没有任何变化，不需要提交");
            return null;
        }
        C2S_Deploy data = new C2S_Deploy { app_version = l_app_version, res_version = l_res_version };
        string postJson = JsonMapper.ToJson(data);
        string responseBody = await HttpHelper.TryPostAsync(ConstValue.PRESENT_DEPLOY, postJson);
        Debug.Log(responseBody);
        var obj = JsonMapper.ToObject<ServerResponse>(responseBody);
        Debug.Log($"部署完成: {obj.msg}");
        return obj.msg;
    }
    static async void DeployRes()
    {
        await PostDeployRes();
    }

    static void SetAppVersion()
    {
        if (l_app_version.Equals(string.Empty))
        {
            Debug.LogError($"不能设置为空");
            return;
        }
        if (l_app_version.Equals(r_app_version))
        {
            Debug.LogError($"没有变化: {l_app_version}");
        }
        PlayerSettings.bundleVersion = l_app_version;
        Debug.Log($"应用设置成功: {PlayerSettings.bundleVersion}");
    }
    static void SetResVersion()
    {
        if (l_res_version.Equals(string.Empty))
        {
            Debug.LogError($"不能设置为空");
            return;
        }
        if (l_res_version.Equals(r_res_version))
        {
            Debug.LogError($"没有变化: {l_res_version}");
        }
        string res_version_txt = $"{Application.dataPath}/res_version.txt";
        File.WriteAllText(res_version_txt, l_res_version);
        Debug.Log($"资源设置成功: {l_res_version}");
    }
}