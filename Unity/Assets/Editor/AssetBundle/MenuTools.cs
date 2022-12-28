using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

public partial class MenuTools : Editor
{
    // 执行批处理文件
    protected static void ExecuteBatch(string fileName, string args)
    {
        Process proc = new Process();
        //proc.StartInfo.WorkingDirectory = projRoot; //在文件所在位置执行
        proc.StartInfo.FileName = fileName; //初始化可执行文件名
        proc.StartInfo.Arguments = args; //初始化可执行文件名
        proc.Start();
    }

    #region 开发
    const string hotfixShared = @"HotFix\Lobby\Shared";
    const string serverShared = @"Unity\Assets\Scenes\ServerOnly\Lobby\Shared";
    const string hotfixCore = @"HotFix\Core";
    const string serverCore = @"Unity\Assets\Scenes\ServerOnly\Core";
    const int SLEEP_TIME = 1;
    [MenuItem("Tools/同步代码/HotFix → Server", true, 1)]
    static async void Sync_H2S()
    {
        string[] srcPaths = new string[] { hotfixCore, hotfixShared };
        string[] outPaths = new string[] { serverCore, serverShared };
        await Sync_SharedCode(srcPaths, outPaths);
    }
    [MenuItem("Tools/同步代码/HotFix ← Server", true, 1)]
    static async void Sync_S2H()
    {
        string[] srcPaths = new string[] { serverCore, serverShared };
        string[] outPaths = new string[] { hotfixCore, hotfixShared };
        await Sync_SharedCode(srcPaths, outPaths);
    }
    static async Task Sync_SharedCode(string[] sourceDirs, string[] destDirs)
    {
        string root_unity = System.Environment.CurrentDirectory;
        string root = Directory.GetParent(root_unity).ToString();

        int loopIndex = 0;
        int loopCount = 0;
        for (int i = 0; i < sourceDirs.Length; i++)
        {
            string sourceDir = Path.Combine(root, sourceDirs[i]);
            string[] csList = Directory.GetFileSystemEntries(sourceDir, "*.cs", SearchOption.AllDirectories); //包含子目录
            loopCount += csList.Length; //计算总文件数
        }
        Debug.Log($"文件夹数={sourceDirs.Length}，总文件数={loopCount}");

        for (int i = 0; i < sourceDirs.Length; i++)
        {
            string sourceDir = Path.Combine(root, sourceDirs[i]);
            string destDir = Path.Combine(root, destDirs[i]);

            try
            {
                string[] csList = Directory.GetFileSystemEntries(sourceDir, "*.cs", SearchOption.AllDirectories);

                // 拷贝cs文件
                foreach (string f in csList)
                {
                    // 无路径的文件名
                    string fName = f.Substring(sourceDir.Length + 1);
                    Debug.Log($"fName={fName}");

                    // 会覆盖目标文件夹的文件
                    File.Copy(Path.Combine(sourceDir, fName), Path.Combine(destDir, fName), true);

                    loopIndex++;
                    float progress = (float)loopIndex / loopCount;
                    string info = $"{loopIndex}/{loopCount}，进度({(progress * 100).ToString("F2")}%)";
                    EditorUtility.DisplayProgressBar("同步中", info, progress);

                    await Task.Delay(SLEEP_TIME);
                }
            }
            catch (DirectoryNotFoundException dirNotFound)
            {
                Debug.Log(dirNotFound.Message);
            }
        }

        await Task.Delay(SLEEP_TIME);
        EditorUtility.ClearProgressBar();
    }
    #endregion

    #region 打包
    [MenuItem("Tools/打包/导表", false, 0)]
    static void GenerateJson()
    {
        int indexOfFormat = 0; //输出格式索引
        int indexOfEncoding = 0; //编码索引
        bool keepSource = true; //是否保留原始文件
        string configs_dir = $"{Application.dataPath}/Bundles/Configs";

        string excel_root = $"{Directory.GetParent(System.Environment.CurrentDirectory)}/Excel";
        string[] array = Directory.GetFiles(excel_root);
        List<string> excelList = new List<string>(array); //Excel文件列表

        foreach (string excelPath in excelList)
        {
            string srcName = (new FileInfo(excelPath)).Name;
            string output = $"{configs_dir}/{srcName.Replace(".xlsx", ".bytes")}";

            //构造Excel工具类
            ExcelUtility excel = new ExcelUtility(excelPath);

            //判断编码类型
            Encoding encoding = null;
            if (indexOfEncoding == 0 || indexOfEncoding == 3)
            {
                encoding = Encoding.GetEncoding("utf-8");
            }
            else if (indexOfEncoding == 1)
            {
                encoding = Encoding.GetEncoding("gb2312");
            }

            //判断输出类型
            if (indexOfFormat == 0)
            {
                excel.ConvertToJson(output, encoding);
            }
            else if (indexOfFormat == 1)
            {
                excel.ConvertToCSV(output, encoding);
            }
            else if (indexOfFormat == 2)
            {
                excel.ConvertToXml(output);
            }
            else if (indexOfFormat == 3)
            {
                excel.ConvertToLua(output, encoding);
            }

            //判断是否保留源文件
            if (!keepSource)
            {
                FileUtil.DeleteFileOrDirectory(excelPath);
            }

            //刷新本地资源
            AssetDatabase.Refresh();
        }
        Debug.Log($"导出{array.Length}个配置到: {configs_dir}");
    }
    [MenuItem("Tools/打包/生成 Proto.cs", false, 0)]
    static void ConvertProto()
    {
        ProtoTools.Proto2CS();
    }
    [MenuItem("Tools/打包/编译 HotFix.sln %_F7", false, 0)]
    static void CompileHotFix()
    {
        string currDir = Directory.GetCurrentDirectory();
        DirectoryInfo currDirInfo = new DirectoryInfo(currDir);
        string projRoot = currDirInfo.Parent.ToString();
        //Debug.Log($"WorkingDirectory: {projRoot}");
        string fileName = "C:\\Program Files\\Microsoft Visual Studio\\2022\\Enterprise\\Common7\\IDE\\devenv.exe";
        string args = $"devenv D:\\Documents\\GitHub\\{ConstValue.APP_NAME}\\HotFix\\HotFix_Project.csproj /rebuild";
        //Debug.Log($"WorkingDirectory: {fileName}");

        bool finish = false;
        float progress = 0;
        EditorApplication.update += () =>
        {
            if (finish == false)
            {
                progress += 0.0002f;
                EditorUtility.DisplayProgressBar("Compiler", args, progress);
                //Debug.Log($"Now: {DateTime.Now.ToString("HH:mm:ss")}"); //每秒执行260-530次 //0.003s
            }
            else
            {
                progress += 0.002f;
                EditorUtility.DisplayProgressBar("Compiler", args, 1);
                if (progress >= 1)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    AssetDatabase.Refresh();
                }
            }
        };

        Task.Run(() =>
        {
            Process proc = new Process();
            proc.EnableRaisingEvents = true;
            proc.StartInfo.FileName = fileName; //初始化可执行文件名
            proc.StartInfo.Arguments = args; //初始化可执行文件名
            proc.Exited += (object sender, EventArgs e) =>
            {
                finish = true;
                Debug.Log(
                    $"Exit time    : {proc.ExitTime}\n" +
                    $"Exit code    : {proc.ExitCode}\n" +
                    $"Elapsed time : {Math.Round((proc.ExitTime - proc.StartTime).TotalMilliseconds)}");
            };
            proc.Start();
            proc.WaitForExit();
            proc.Close();
        });
    }
    [MenuItem("Tools/打包/移动 HotFix.dll %_F8", false, 0)]
    static void MoveDLL()
    {
        string dllPath = Path.Combine(Application.streamingAssetsPath, "HotFix.dll");
        if (!File.Exists(dllPath))
        {
            Debug.LogError($"文件不存在：{dllPath}");
            return;
        }

        string bytesPath = Path.Combine(Application.dataPath, "Bundles/Configs/HotFix.dll.bytes");
        if (File.Exists(bytesPath))
        {
            File.Delete(bytesPath);
        }
        File.Move(dllPath, bytesPath);

        Directory.Delete(Application.streamingAssetsPath, true);

        AssetDatabase.Refresh();
        Debug.Log("移动完成");
    }

    [MenuItem("Tools/打包/资源/Windows", false, 1)]
    static void BuildRes_Win64()
    {
        BundleTools.BuildRes(BuildTarget.StandaloneWindows64);
    }
    [MenuItem("Tools/打包/资源/Android", false, 1)]
    static void BuildRes_Android()
    {
        BundleTools.BuildRes(BuildTarget.Android);
    }
    [MenuItem("Tools/打包/资源/iOS", false, 1)]
    static void BuildRes_iOS()
    {
        BundleTools.BuildRes(BuildTarget.iOS);
    }

    [MenuItem("Tools/打包/服务器/Windows", false, 2)]
    static void BuildServer_Win64()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Server, BuildTarget.StandaloneWindows64);

        RemoveIcon();

        if (Directory.Exists(ConstValue.BuildDir) == false)
            Directory.CreateDirectory(ConstValue.BuildDir);

        string build_server = $"{ConstValue.BuildDir}/Server";
        if (Directory.Exists(build_server) == false)
            Directory.CreateDirectory(build_server);

        BuildPlayerOptions opt = new BuildPlayerOptions
        {
            scenes = new string[] { "Assets/Scenes/Server.unity" },
            locationPathName = $"{build_server}/GameServer.exe",
            target = BuildTarget.StandaloneWindows64,
#if UNITY_2021_1_OR_NEWER
            options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development | BuildOptions.EnableDeepProfilingSupport,
            subtarget = (int)StandaloneBuildSubtarget.Server,
#else
            options = BuildOptions.EnableHeadlessMode | BuildOptions.ShowBuiltPlayer | BuildOptions.Development
#endif
        };

        BuildReport report = BuildPipeline.BuildPlayer(opt);

        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
            Debug.Log($"打包成功: {opt.locationPathName}");
        if (summary.result == BuildResult.Failed)
            Debug.LogError("打包失败");
    }
    [MenuItem("Tools/打包/服务器/Linux", true, 2)]
    static void BuildServer_Linux() { }

    [MenuItem("Tools/打包/客户端/Windows", false, 3)]
    static void BuildClient_Win64()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Standalone, BuildTarget.StandaloneWindows64);

        BuildClient(BuildTarget.StandaloneWindows64, 101);
    }
    [MenuItem("Tools/打包/客户端/Android", false, 3)]
    static void BuildClient_Android()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Android, BuildTarget.Android);

        BuildClient(BuildTarget.Android, 102);
    }
    [MenuItem("Tools/打包/客户端/iOS", false, 3)]
    static void BuildClient_iOS()
    {
        BuildClient(BuildTarget.iOS, 103);
    }
    public static void BuildClient(BuildTarget target, int channel = 101)
    {
        SetIcon();

        if (Directory.Exists(ConstValue.BuildDir) == false)
            Directory.CreateDirectory(ConstValue.BuildDir);

        string fileName = $"GameClient_{channel}";
        string build_dir = $"{ConstValue.BuildDir}/{fileName}";
        if (Directory.Exists(build_dir) == false)
            Directory.CreateDirectory(build_dir);

        string ext = string.Empty;
        switch (target)
        {
            case BuildTarget.StandaloneWindows64:
                ext = "exe";
                break;
            case BuildTarget.Android:
                ext = "apk";
                break;
            case BuildTarget.iOS:
                ext = "ipa";
                break;
        }

        BuildPlayerOptions opt = new BuildPlayerOptions
        {
            scenes = new string[] { "Assets/Scenes/Client.unity" },
            locationPathName = Path.Combine(build_dir, $"{fileName}.{ext}"),
            target = target,
            extraScriptingDefines = new string[] { $"Channel_{channel}" },
            options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development,
        };

        BuildReport report = BuildPipeline.BuildPlayer(opt);

        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
            Debug.Log($"打包成功: {opt.locationPathName}");
        if (summary.result == BuildResult.Failed)
            Debug.LogError("打包失败");
    }
    [MenuItem("Tools/打包/部署 %_F9", false, 3)]
    static void ModifyHost()
    {
        DeployWindow.ShowWindow();
    }

    [MenuItem("Tools/图标/SetIcon", true)]
    static void SetIcon()
    {
        string filePath = $"Assets/Arts/Icon/{ConstValue.APP_NAME}.png";
        Texture2D t2d = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);

        Texture2D[] array_1 = new Texture2D[] { t2d };
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, array_1); //default

        Texture2D[] array_8 = new Texture2D[] { t2d, t2d, t2d, t2d, t2d, t2d, t2d, t2d };
        //PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone, array_8); //override, 会覆盖

        AssetDatabase.Refresh();
    }
    [MenuItem("Tools/图标/RemoveIcon", true)]
    static void RemoveIcon()
    {
        Texture2D[] array_1 = new Texture2D[] { null };
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, null); //default

        Texture2D[] array_8 = new Texture2D[] { null, null, null, null, null, null, null, null };
        //PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone, array_8); //override, 会覆盖

        AssetDatabase.Refresh();
    }
    #endregion

    #region 运行
    //% (ctrl on Windows and Linux, cmd on macOS),
    //^ (ctrl on Windows, Linux, and macOS),
    //# (shift),
    //& (alt)
    [MenuItem("Tools/运行/客户端 %_F11", false, 11)]
    static void RunClient()
    {
        string filePath = Path.Combine(ConstValue.UnityDir, "Build/GameClient_101/GameClient_101.exe");
        Process.Start(filePath);
        Debug.Log(filePath);
    }
    [MenuItem("Tools/运行/服务器 %_F12", false, 11)]
    static void RunServer()
    {
        string filePath = Path.Combine(ConstValue.UnityDir, "Build/Server/GameServer.exe");
        Process.Start(filePath);
        Debug.Log(filePath);
    }
    #endregion
}