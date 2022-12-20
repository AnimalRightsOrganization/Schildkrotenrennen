using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using Newtonsoft.Json;
using Debug = UnityEngine.Debug;

public class DeployWindow : EditorWindow
{
    // 远程目录结构：
    // (afk/fight/turtlerace)/(release/res)/(iOS/Android/StandaloneWindows64)

    //const string readme = "说明：" +
    //    "\n[线上版本] 请求服务器查询线上当前运营的应用和资源版本。" +
    //    "\n[打包版本] 指的是执行完打包，准备上传到服务器的安装包和资源包。" +
    //    "\n[打包] 执行了资源打包+部署资源到模拟的本地和服务器，还需要实际上传包体，并更新SQL记录。" +
    //    "\n[压缩] 把安装包和资源包打包称 *.zip 文件上传。" +
    //    "\n[部署] 通过POST通知PHP执行更新SQL，同时生成一条部署记录。";

    static string r_app_version = string.Empty;
    static string r_res_version = string.Empty;
    static string l_app_version = string.Empty;
    static string l_res_version = string.Empty;

    static int targetPlatform = 0;

    public static void ShowWindow()
    {
        Rect rect = new Rect(0, 0, 600, 350);
        DeployWindow window = (DeployWindow)GetWindowWithRect(typeof(DeployWindow), rect, true, "部署");
        window.Show();
    }

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
        CheckNetwork();
        CheckUseAB();
    }
    void OnFocus()
    {
        if (is_editing == true)
        {
            Debug.Log("Focus: 检查网络");
            is_editing = false;
            CheckNetwork();
        }
    }
    void OnLostFocus() { }

    void OnGUI()
    {
        targetPlatform = GUILayout.SelectionGrid(targetPlatform, new string[] { "打包", "环境" }, 2, GUILayout.Height(30)); //一行几个
        GUILayout.Space(10);
        if (targetPlatform == 0)
            Page0();
        else
            Page1();

        //GUILayout.Space(10);
        //GUILayout.Box($"目标平台：{ConstValue.PLATFORM_NAME}");
    }
    // 打包
    static void Page0()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(10); //左10
        if (GUILayout.Button("线上版本", GUILayout.Width(100))) { GetVersion(); }
        GUILayout.Space(10);
        GUILayout.Label("应用:", GUILayout.Width(40));
        GUILayout.Box(r_app_version, GUILayout.Width(175));
        GUILayout.Space(10);
        GUILayout.Label("资源:", GUILayout.Width(40));
        GUILayout.Box(r_res_version, GUILayout.Width(175));
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Box("打包版本", GUILayout.Width(100));
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

        // 表格
        GUILayout.Space(30);
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Box("渠道打包", GUILayout.Width(100));
        GUILayout.Box("PC", GUILayout.Width(100));
        GUILayout.Box("安卓", GUILayout.Width(100));
        GUILayout.Box("苹果", GUILayout.Width(100));
        GUILayout.Box("全部", GUILayout.Width(100));
        GUILayout.EndHorizontal();
        // 行
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Box("内测", GUILayout.Width(100));
        if (GUILayout.Button("101", GUILayout.Width(100)))
        {
            //BuildApp(); //打包应用
            //BuildRes(); //打包资源
            return;
        }
        if (GUILayout.Button("102", GUILayout.Width(100)))
        {
            return;
        }
        if (GUILayout.Button("103", GUILayout.Width(100)))
        {
            return;
        }
        if (GUILayout.Button("全部", GUILayout.Width(100)))
        {
            BuildAll(101);
            return;
        }
        GUILayout.EndHorizontal();
        // 行
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Box("官方", GUILayout.Width(100));
        if (GUILayout.Button("1001", GUILayout.Width(100))) { }
        if (GUILayout.Button("1002", GUILayout.Width(100))) { }
        if (GUILayout.Button("1003", GUILayout.Width(100))) { }
        if (GUILayout.Button("全部", GUILayout.Width(100))) { }
        GUILayout.EndHorizontal();
        // 行
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Box("Steam", GUILayout.Width(100));
        if (GUILayout.Button("1011", GUILayout.Width(100))) { }
        GUILayout.Box("", GUILayout.Width(100));
        GUILayout.Box("", GUILayout.Width(100));
        GUILayout.Box("", GUILayout.Width(100));
        GUILayout.EndHorizontal();
        // 行
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Box("华为", GUILayout.Width(100));
        GUILayout.Box("", GUILayout.Width(100));
        if (GUILayout.Button("1022", GUILayout.Width(100))) { }
        GUILayout.Box("", GUILayout.Width(100));
        GUILayout.Box("", GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("部署", GUILayout.Width(100), GUILayout.Height(30))) { DeployRes(); }
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
    }
    // 环境
    static void Page1()
    {
        //GUILayout.Label("2/2");
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Box("安装包输出", GUILayout.Width(100));
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
        GUILayout.Box("局域网资源", GUILayout.Width(100));
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
        GUILayout.Space(10);
        GUILayout.Box("REST", GUILayout.Width(100));
        GUILayout.Space(10);
        string restapi = "https://restapi.moegijinka.cn";
        GUILayout.Box(restapi, GUILayout.Width(450));
        GUILayout.Space(10);
        GUILayout.EndHorizontal();


        // 说明
        //GUILayout.Space(10);
        //GUILayout.BeginHorizontal();
        //GUILayout.Space(10);
        //GUILayout.TextArea(readme, GUILayout.Width(580), GUILayout.Height(100));
        //GUILayout.Space(10);
        //GUILayout.EndHorizontal();


        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        {
            GUILayout.Space(10);
            GUILayout.Box("当前网络", GUILayout.Width(100));
            GUILayout.Space(10);
            if (is_www)
            {
                if (GUILayout.Button("外网", GUILayout.Width(90)))
                {
                    CheckNetwork();
                    Debug.Log("刷新");
                }
            }
            else
            {
                if (GUILayout.Button("内网", GUILayout.Width(90)))
                {
                    CheckNetwork();
                    Debug.Log("刷新");
                }
            }
            GUILayout.Space(10);
            if (GUILayout.Button("修改", GUILayout.Width(80)))
            {
                ChangeNetwork();
            }
            GUILayout.Space(10);
        }
        GUILayout.EndHorizontal();


        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        {
            GUILayout.Space(10);
            GUILayout.Box("是否用热更新", GUILayout.Width(100));
            GUILayout.Space(10);
            if (use_ab)
            {
                if (GUILayout.Button("使用热更新", GUILayout.Width(90)))
                {
                    CheckUseAB();
                    Debug.Log("刷新");
                }
            }
            else
            {
                if (GUILayout.Button("不用热更新", GUILayout.Width(90)))
                {
                    CheckUseAB();
                    Debug.Log("刷新");
                }
            }
            GUILayout.Space(10);
            if (GUILayout.Button("修改", GUILayout.Width(80)))
            {
                ChangeUseAB();
            }
            GUILayout.Space(10);
        }
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

    // 压缩
    [MenuItem("Tools/PC压缩", true)]
    static async Task PackStandaloneWindows64(int channel)
    {
        string app_path = Path.Combine(ConstValue.BuildDir, $"StandaloneWindows64").Replace("/", "\\");
        string app_zip = Path.Combine(ConstValue.BuildDir, $"GameClient_{channel}.zip").Replace("/", "\\");
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
    [MenuItem("Tools/应用压缩", true)]
    static async Task PackRelease(int channel)
    {
        string zip_path = Path.Combine(ConstValue.BuildDir, $"GameClient_{channel}.zip").Replace("/", "\\");
        string apk_path = Path.Combine(ConstValue.BuildDir, $"Android/GameClient_{channel + 1}.apk").Replace("/", "\\");

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 0f);
        await Task.Delay(100);
        //放到同一个目录下
        string src_dir = Path.Combine(ConstValue.BuildDir, "release").Replace("/", "\\");
        if (Directory.Exists(ConstValue.ZipDeploy) == false)
            Directory.CreateDirectory(ConstValue.ZipDeploy);
        string dst_zip = Path.Combine(ConstValue.ZipDeploy, "release.zip").Replace("/", "\\");
        if (Directory.Exists(src_dir))
            Directory.Delete(src_dir, true);
        Directory.CreateDirectory(src_dir);
        Debug.Log($"压缩应用：{src_dir} --->\n{dst_zip}");

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 0.25f);
        await Task.Delay(100);
        string zip_copy_to = $"{src_dir}\\GameClient_{channel}.zip";
        File.Copy(zip_path, zip_copy_to);
        File.SetAttributes(zip_copy_to, FileAttributes.Normal);

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 0.5f);
        await Task.Delay(100);
        string apk_copy_to = $"{src_dir}\\GameClient_{channel + 1}.apk";
        File.Copy(apk_path, apk_copy_to);
        File.SetAttributes(apk_copy_to, FileAttributes.Normal);

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 0.75f);
        await Task.Delay(1000);
        ZipTools.PackFiles(dst_zip, src_dir); //生成.zip

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 1f);
        await Task.Delay(100);
        EditorUtility.ClearProgressBar();
        Debug.Log("压缩成功");
    }
    [MenuItem("Tools/资源压缩", true)]
    static async Task PackRes()
    {
        // 本地资源目录包含杂项，使用局域网资源目录
        string res_path = Path.Combine(ConstValue.GetDeployRoot, "res").Replace("/", "\\");
        string res_zip = Path.Combine(ConstValue.ZipDeploy, "res.zip").Replace("/", "\\");
        Debug.Log($"压缩资源：{res_path} --->\n{res_zip}");

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 0f);
        await Task.Delay(100);
        if (Directory.Exists(ConstValue.ZipDeploy) == false)
            Directory.CreateDirectory(ConstValue.ZipDeploy);
        if (File.Exists(res_zip))
            File.Delete(res_zip); //若已存在，删除

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 0.5f);
        await Task.Delay(100);
        ZipTools.PackFiles(res_zip, res_path); //生成.zip

        EditorUtility.DisplayProgressBar("压缩", "压缩中...", 1f);
        await Task.Delay(100);
        EditorUtility.ClearProgressBar();
        Debug.Log("压缩成功");
    }
    [MenuItem("Tools/Cancel", false)]
    static void Cancel()
    {
        EditorUtility.ClearProgressBar();
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

    // 某渠道三个安装包、三份资源，压缩
    static async void BuildAll(int channel)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();


        // 所有渠道的资源，都共用一份！
        BundleTools.BuildRes(BuildTarget.StandaloneWindows64);
        BundleTools.BuildRes(BuildTarget.Android);
        await PackRes(); //../Deploy/res/

        BundleTools.BuildClient(BuildTarget.StandaloneWindows64, channel);
        await PackStandaloneWindows64(channel);
        BundleTools.BuildClient(BuildTarget.Android, channel + 1);
        //BundleTools.BuildClient(BuildTarget.iOS, channel + 2);
        await PackRelease(channel); //../Deploy/release/

        //总运行毫秒数，可多次分段取
        stopwatch.Stop();
        double TotalMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
        EditorUtility.DisplayDialog("打包完成", $"用时:{(TotalMilliseconds/1000).ToString("F0")}秒", "查看");
        Process.Start("explorer", ConstValue.ZipDeploy);
    }



    const string notepad_exe = @"C:\Program Files\Notepad++\notepad++.exe";
    const string hosts_path = @"C:\Windows\System32\drivers\etc\hosts";
    static bool is_www = true; //默认
    static bool is_editing = false; //是否正在编辑

    const string yes_use_ab = "USE_ASSETBUNDLE";
    const string not_use_ab = "";
    static NamedBuildTarget target = NamedBuildTarget.Standalone;
    static bool use_ab = false; //默认

    static async void CheckNetwork()
    {
        string[] lines = await File.ReadAllLinesAsync(hosts_path);
        // 默认有四行，从追加的开始读
        is_www = true;
        for (int i = 4; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.StartsWith("##") == false)
            {
                is_www = false;
                break;
            }
        }
    }
    static void ChangeNetwork()
    {
        is_editing = true;
        Process.Start(notepad_exe, hosts_path); //notepad++
    }
    static bool CheckUseAB()
    {
        string cur_use_ab = PlayerSettings.GetScriptingDefineSymbols(target);
        use_ab = cur_use_ab.Contains(yes_use_ab);
        return use_ab;
    }
    static void ChangeUseAB()
    {
        CheckUseAB();

        use_ab = !use_ab;
        string dst_use_ab = use_ab ? yes_use_ab : not_use_ab;

        PlayerSettings.SetScriptingDefineSymbols(target, dst_use_ab);
        AssetDatabase.Refresh();
    }
}