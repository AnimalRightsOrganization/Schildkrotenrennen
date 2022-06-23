using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using LitJson;
#if UNITY_EDITOR
using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
public partial class BundleTools : Editor
{
    // 执行命令提示符
    protected static void ExecuteCommand(string command)
    {
        /* cmd /c dir 是执行完dir命令后关闭命令窗口。
         * cmd /k dir 是执行完dir命令后不关闭命令窗口。
         * cmd /c start dir 会打开一个新窗口后执行dir指令，原窗口会关闭。
         * cmd /k start dir 会打开一个新窗口后执行dir指令，原窗口不会关闭。*/

        Process p = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "cmd.exe";
        startInfo.WorkingDirectory = @"C:\";
        //startInfo.Arguments = @"/c " + command; // cmd.exe spesific implementation
        startInfo.Arguments = @"/k " + command;
        p.StartInfo = startInfo;
        p.Start();
        //p.WaitForExit(); //等待一定时间（ms）退出
    }
    // 执行批处理文件
    protected static void ExecuteBatch(string batFileName)
    {
        string currDir = Directory.GetCurrentDirectory();
        DirectoryInfo currDirInfo = new DirectoryInfo(currDir);
        string projRoot = currDirInfo.Parent.ToString();
        Debug.Log(projRoot);

        string filePath = $@"{projRoot}\{batFileName}";
        Debug.Log(filePath);
        Process proc = new Process();
        proc.StartInfo.WorkingDirectory = projRoot; //在文件所在位置执行
        proc.StartInfo.FileName = filePath; //初始化可执行文件名
        proc.Start();
    }

    #region 测试

    const string hotfixShared = @"HotFix\Lobby\Shared";
    const string serverShared = @"NetCoreServer\NetCoreApp\Lobby\Shared";
    const string hotfixCore = @"HotFix\Core";
    const string serverCore = @"NetCoreServer\NetCoreApp\Core";
    const int SLEEP_TIME = 1;

    [MenuItem("Tools/测试/HotFix >> Server", false, 0)]
    static async void Sync_H2S()
    {
        string[] srcPaths = new string[] { hotfixCore, hotfixShared };
        string[] outPaths = new string[] { serverCore, serverShared };
        await Sync_SharedCode(srcPaths, outPaths);
    }
    [MenuItem("Tools/测试/HotFix << Server", false, 0)]
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
    [MenuItem("Tools/测试/CMD", true, 11)]
    static void TestCMD()
    {
        ExecuteCommand(@"ipconfig /flushdns");
        //ExecuteCommand(@"ping www.baidu.com");
    }
    [MenuItem("Tools/测试/清理临时文件夹", true, 12)]
    static void ClearTmpFolders()
    {
        // 两个需要清理的目录
        // ./Assets/StreamingAssets/Bundles
        // ./StandaloneWindows64/Android/iOS
        string[] pathArray = new string[]
        {
            Path.Combine(Application.streamingAssetsPath, "Bundles"),
            Path.Combine(ConstValue.GetUnityDir, ConstValue.PLATFORM_NAME),
        };
        for (int i = 0; i < pathArray.Length; i++)
        {
            var path = pathArray[i];
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
        Debug.Log("清理完成");
    }
    [MenuItem("Tools/测试/取消读条", true, 13)]
    static void CancelableProgressBar()
    {
        EditorUtility.ClearProgressBar();
    }
    [MenuItem("Tools/测试/启动客户端 %_F11", false, 100)]
    static void StartTestApp()
    {
        string currDir = Directory.GetCurrentDirectory();
        DirectoryInfo currDirInfo = new DirectoryInfo(currDir);
        string exePath = $@"{currDirInfo.Parent}\Unity\Build\";

        Process proc = new Process();
        proc.StartInfo.WorkingDirectory = exePath; //在文件所在位置执行
        proc.StartInfo.FileName = "turtle.exe"; //初始化可执行文件名
        proc.Start();
    }
    [MenuItem("Tools/测试/启动服务器 %_F12", false, 100)]
    static void StartServer()
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

    #region 热更新

    [MenuItem("Tools/热更新/生成Proto", false, 21)]
    static void ConvertProto()
    {
        InnerProto2CS.Proto2CS();
    }
    [MenuItem("Tools/热更新/编译热更工程", false, 22)]
    static void CompileHotFix()
    {
        ExecuteBatch("compile_hotfix.bat");
    }
    //% (ctrl on Windows and Linux, cmd on macOS),
    //^ (ctrl on Windows, Linux, and macOS),
    //# (shift),
    //& (alt)
    [MenuItem("Tools/热更新/MoveDLL %_F8", false, 23)]
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

    [MenuItem("Tools/Shader/重置IncludedShaders", true, 31)]
    static void ResetIncludedShaders() { }
    [MenuItem("Tools/Shader/设置IncludedShaders", true, 31)]
    static void SetIncludedShaders() { }

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
}
#endif