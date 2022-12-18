using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using Debug = UnityEngine.Debug;
using static DG.DemiEditor.DeEditorUtils;

public partial class BundleTools : Editor
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

    #region 代码
    const string hotfixShared = @"HotFix\Lobby\Shared";
    const string serverShared = @"Unity\Assets\Scenes\ServerOnly\Lobby\Shared";
    const string hotfixCore = @"HotFix\Core";
    const string serverCore = @"Unity\Assets\Scenes\ServerOnly\Core";
    const int SLEEP_TIME = 1;
    [MenuItem("Tools/覆盖/HotFix >> Server", false, 1)]
    static async void Sync_H2S()
    {
        string[] srcPaths = new string[] { hotfixCore, hotfixShared };
        string[] outPaths = new string[] { serverCore, serverShared };
        await Sync_SharedCode(srcPaths, outPaths);
    }
    [MenuItem("Tools/覆盖/HotFix << Server", false, 1)]
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
    [MenuItem("Assets/Open Server Project", false)]
    static void OpenServerProject()
    {
        string currDir = Directory.GetCurrentDirectory();
        DirectoryInfo currDirInfo = new DirectoryInfo(currDir);
        //string projPath = $@"{currDirInfo.Parent}\NetCoreServer\NetCoreApp.sln";
        string projPath = $@"{currDirInfo.Parent}\NetCoreServer";
#if UNITY_STANDALONE_OSX
        //MacOS只认[/]。但不能编译winform。
        projPath = projPath.Replace(@"\", "/");
#endif
        Process proc = new Process();
        proc.StartInfo.WorkingDirectory = projPath;
        proc.StartInfo.FileName = "NetCoreApp.sln";
        proc.Start();
    }
    #endregion

    #region 运行
    //% (ctrl on Windows and Linux, cmd on macOS),
    //^ (ctrl on Windows, Linux, and macOS),
    //# (shift),
    //& (alt)
    [MenuItem("Tools/运行/命令面板 %_F10", false)]
    static void RunEditor()
    {
        TestWindow.ShowWindow();
    }
    [MenuItem("Tools/运行/客户端 %_F11", false, 11)]
    static void RunClient()
    {
        string currDir = Directory.GetCurrentDirectory();
        DirectoryInfo currDirInfo = new DirectoryInfo(currDir);
        string exePath = $@"{currDirInfo.Parent}\Unity\Build\";

        Process proc = new Process();
        proc.StartInfo.WorkingDirectory = exePath; //在文件所在位置执行
        proc.StartInfo.FileName = "turtle.exe"; //初始化可执行文件名
        proc.Start();
    }
    [MenuItem("Tools/运行/服务器 %_F12", false, 11)]
    static void RunServer()
    {
        string currDir = Directory.GetCurrentDirectory();
        DirectoryInfo currDirInfo = new DirectoryInfo(currDir);
        string exePath = $@"{currDirInfo.Parent}\NetCoreServer\NetCoreApp\bin\Debug\netcoreapp3.1\";

        Process proc = new Process();
        proc.StartInfo.WorkingDirectory = exePath; //在文件所在位置执行
        proc.StartInfo.FileName = "NetCoreServer.exe"; //初始化可执行文件名
        proc.Start();
    }
    #endregion

    #region 打包
    [MenuItem("Tools/打包/生成 Proto.cs", false, 1)]
    static void ConvertProto()
    {
        ProtoTools.Proto2CS();
    }
    [MenuItem("Tools/打包/编译 HotFix.sln %_F7", false, 1)]
    static void CompileHotFix()
    {
        string currDir = Directory.GetCurrentDirectory();
        DirectoryInfo currDirInfo = new DirectoryInfo(currDir);
        string projRoot = currDirInfo.Parent.ToString();
        //Debug.Log($"WorkingDirectory: {projRoot}");
        string fileName = "C:\\Program Files\\Microsoft Visual Studio\\2022\\Enterprise\\Common7\\IDE\\devenv.exe";
        string args = "devenv D:\\Documents\\GitHub\\TurtleRace\\HotFix\\HotFix_Project.csproj /rebuild";
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
            //ExecuteBatch(fileName, args);

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

    [MenuItem("Tools/打包/移动 HotFix.dll %_F8", false, 1)]
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
    [MenuItem("Tools/打包/AssetBundle", false, 1)]
    static void BuildRes()
    {
        BuildTarget target = (BuildTarget)System.Enum.Parse(typeof(BuildTarget), ConstValue.PLATFORM_NAME);
        Debug.Log($"打包{target}平台资源");
        BundleTools.Build_Target(target);
    }
    [MenuItem("Tools/打包/服务器", false, 1)]
    static void BuildServer_Win64()
    {
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
    [MenuItem("Tools/打包/客户端", false, 1)]
    static void BuildClient_Win64()
    {
        SetIcon();

        if (Directory.Exists(ConstValue.BuildDir) == false)
            Directory.CreateDirectory(ConstValue.BuildDir);

        string build_client = $"{ConstValue.BuildDir}/Client";
        if (Directory.Exists(build_client) == false)
            Directory.CreateDirectory(build_client);

        BuildPlayerOptions opt = new BuildPlayerOptions
        {
            scenes = new string[] { "Assets/Scenes/Client.unity" },
            locationPathName = Path.Combine(build_client, "Client.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development,
        };

        BuildReport report = BuildPipeline.BuildPlayer(opt);

        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
            Debug.Log($"打包成功: {opt.locationPathName}");
        if (summary.result == BuildResult.Failed)
            Debug.LogError("打包失败");
    }
    [MenuItem("Tools/打包/安卓", false, 1)]
    static void BuildClient_Android()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Android, BuildTarget.Android);
    }
    static void BuildClient_iOS()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.iOS, BuildTarget.iOS);

        string defines = "USE_ASSETBUNDLE;CHANNEL_11011";
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defines);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.moegijinka.turtlerace"); //不同渠道包名不一样
        //PlayerSettings.bundleVersion = string.Format("{0}.{1}.{2}", GameConfig.clientVersions[0],
        //    GameConfig.clientVersions[1] * 100 + GameConfig.clientVersions[2], GameConfig.clientVersions[3]);

        int code;
        if (int.TryParse(PlayerSettings.iOS.buildNumber, out code) == false)
        {
            code = 0;
        }
        PlayerSettings.iOS.buildNumber = (code + 1).ToString();
    }
    [MenuItem("Tools/图标/SetIcon", true)]
    static void SetIcon()
    {
        string filePath = $"Assets/Arts/Textures/Icon.png";
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
}