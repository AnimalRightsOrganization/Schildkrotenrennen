using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

public class DeployWindow : EditorWindow
{
    [MenuItem("Tools/打包/部署", false)]
    static void AddWindow()
    {
        Rect rect = new Rect(0, 0, 600, 400);
        DeployWindow window = (DeployWindow)GetWindowWithRect(typeof(DeployWindow), rect, true, "部署");
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

    void OnEnable()
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
    }

    int targetPlatform= 0;

    void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Box($"目标平台：{ConstValue.PLATFORM_NAME}");
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Box("应用输出", GUILayout.Width(100));
        GUILayout.Space(10);
        if (GUILayout.Button(ConstValue.BuildDir, GUILayout.Width(450)))
        {
            if (Directory.Exists(ConstValue.BuildDir) == false)
                Directory.CreateDirectory(ConstValue.BuildDir);
            System.Diagnostics.Process.Start("explorer", ConstValue.BuildDir);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Box("远程资源", GUILayout.Width(100));
        GUILayout.Space(10);
        if (GUILayout.Button(ConstValue.GetDeployRoot, GUILayout.Width(450)))
        {
            System.Diagnostics.Process.Start("explorer", ConstValue.GetDeployRes);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Box("本地资源", GUILayout.Width(100));
        GUILayout.Space(10);
        string path = Application.persistentDataPath.Replace("/", "\\");
        if (GUILayout.Button(path, GUILayout.Width(450)))
        {
            System.Diagnostics.Process.Start("explorer", path);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10); //左10
        if (GUILayout.Button("线上版本", GUILayout.Width(100))) { GetVersion(); }
        GUILayout.Space(10);
        GUILayout.Label("应用:", GUILayout.Width(40));
        GUILayout.TextField(r_app_version, GUILayout.Width(175));
        GUILayout.Space(10);
        GUILayout.Label("资源:", GUILayout.Width(40));
        GUILayout.TextField(r_res_version, GUILayout.Width(175));
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("打包版本", GUILayout.Width(100))) { GetVersion(); }
        GUILayout.Space(10);
        GUILayout.Label("应用:", GUILayout.Width(40));
        l_app_version = GUILayout.TextField(l_app_version, GUILayout.Width(80));
        GUILayout.Space(10);
        if (GUILayout.Button("设置", GUILayout.Width(80)))
        {
            SetAppVersion();
        }
        GUILayout.Space(10);
        GUILayout.Label("资源:", GUILayout.Width(40));
        l_res_version = GUILayout.TextField(l_res_version, GUILayout.Width(80));
        GUILayout.Space(10);
        if (GUILayout.Button("设置", GUILayout.Width(80)))
        {
            SetResVersion();
        }
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Box("打包", GUILayout.Width(100));
        targetPlatform = GUILayout.SelectionGrid(targetPlatform, new string[] { "Windows", "Android", "iOS" }, 1); //一行几个
        GUILayout.Space(55);
        if (GUILayout.Button("打包应用", GUILayout.Width(175))) { BuildApp(); return; }
        GUILayout.Space(55);
        if (GUILayout.Button("打包资源", GUILayout.Width(175))) { BuildRes(); }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Box("压缩", GUILayout.Width(100));
        GUILayout.Space(55);
        if (GUILayout.Button("压缩应用", GUILayout.Width(175))) { PackAppZip(); }
        GUILayout.Space(55);
        if (GUILayout.Button("压缩资源", GUILayout.Width(175))) { PackResZip(); }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("部署", GUILayout.Width(100))) { DeployRes(); }
        GUILayout.Space(10);
        GUILayout.EndHorizontal();


        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.TextArea(readme, GUILayout.Width(580), GUILayout.Height(100));
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
    }

    // 配置
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
        AssetDatabase.Refresh();
    }

    // 打包
    static void BuildApp()
    {
        BuildTarget target = (BuildTarget)System.Enum.Parse(typeof(BuildTarget), ConstValue.PLATFORM_NAME);
        if (Directory.Exists(ConstValue.BuildDir))
            Directory.Delete(ConstValue.BuildDir, true);
        Directory.CreateDirectory(ConstValue.BuildDir);

        BuildPlayerOptions opt = new BuildPlayerOptions();
        opt.scenes = new string[] { "Assets/Scenes/Client.unity" };
        opt.locationPathName = ConstValue.LocationPath;
        opt.target = target;
        opt.options = BuildOptions.None;

        BuildPipeline.BuildPlayer(opt);

        Debug.Log("打包成功!");
    }
    static void BuildRes()
    {
        BuildTarget target = (BuildTarget)System.Enum.Parse(typeof(BuildTarget), ConstValue.PLATFORM_NAME);
        BundleTools.Build_Target(target);
    }

    // 远程目录结构：
    // (afk/fight/turtlerace)/(app/res)/(iOS/Android/StandaloneWindows64)

    // 压缩
    static async void PackAppZip()
    {
        string app_path = ConstValue.BuildDir;
        string app_zip = Path.Combine(Environment.CurrentDirectory, $"{ConstValue.PLATFORM_NAME}.app.zip").Replace("/", "\\");
        Debug.Log($"压缩应用：{app_path} --->\n{app_zip}");

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 0f);
        await Task.Delay(100);
        File.Delete(app_zip);

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 0.5f);
        await Task.Delay(100);
        ZipTools.PackFiles(app_zip, app_path);

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 1f);
        await Task.Delay(100);
        EditorUtility.ClearProgressBar();
        Debug.Log("压缩成功");
    }
    static async void PackResZip()
    {
        string res_path = Path.Combine(Application.persistentDataPath, ConstValue.PLATFORM_NAME).Replace("/", "\\");
        string res_zip = Path.Combine(Environment.CurrentDirectory, $"{ConstValue.PLATFORM_NAME}.res.zip").Replace("/", "\\");
        Debug.Log($"压缩资源：{res_path} --->\n{res_zip}");

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 0f);
        await Task.Delay(100);
        File.Delete(res_zip);

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 0.5f);
        await Task.Delay(100);
        ZipTools.PackFiles(res_zip, res_path);

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 1f);
        await Task.Delay(100);
        EditorUtility.ClearProgressBar();
        Debug.Log("压缩成功");
    }

    // 查询版本
    static async Task<string> GetVersionAsync()
    {
        string responseBody = await HttpHelper.TryGetAsync(ConstValue.GAME_DATA);
        if (string.IsNullOrEmpty(responseBody))
        {
            Debug.LogError("获取远程资源版本失败");
            return null;
        }
        //var obj = JsonMapper.ToObject<ServerResponse>(responseBody);
        //var model = JsonMapper.ToObject<DBApp>(obj.data);
        var obj = JsonConvert.DeserializeObject<ServerResponse>(responseBody);
        var model = JsonConvert.DeserializeObject<DBApp>(obj.data);
        Debug.Log(obj.data);
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

    // 部署
    static async Task<string> PostDeployRes()
    {
        if (l_app_version.Equals(r_app_version) && (l_res_version.Equals(r_res_version)))
        {
            Debug.LogError("没有任何变化，不需要提交");
            return null;
        }
        C2S_Deploy data = new C2S_Deploy { app_version = l_app_version, res_version = l_res_version };
        string postJson = JsonConvert.SerializeObject(data);
        string responseBody = await HttpHelper.TryPostAsync(ConstValue.PRESENT_DEPLOY, postJson);
        Debug.Log(responseBody);
        var obj = JsonConvert.DeserializeObject<ServerResponse>(responseBody);
        Debug.Log($"部署完成: {obj.msg}");
        return obj.msg;
    }
    static async void DeployRes()
    {
        string src = @"C:\Users\Administrator\source\repos\QRCode\GameCenter\bin\Debug\net6.0-windows\Temp\StandaloneWindows64.zip";
        string dst = @"C:\Users\Administrator\source\repos\QRCode\GameCenter\bin\Debug\net6.0-windows\Applications\turtlerace\";
        ZipTools.UnpackFiles(src, dst);
        await Task.CompletedTask;
        //return;

        //await PostDeployRes();
    }
}