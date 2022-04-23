using System.IO;
using UnityEngine;
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

    [MenuItem("Tools/测试/HotFix >> Server", false, 11)]
    static void Sync_H2S()
    {
        Sync_SharedCode(hotfixShared, serverShared);
    }
    [MenuItem("Tools/测试/HotFix << Server", false, 12)]
    static void Sync_S2H()
    {
        Sync_SharedCode(serverShared, hotfixShared);
    }
    static void Sync_SharedCode(string srcPath, string outPath)
    {
        string root_unity = System.Environment.CurrentDirectory;
        string root = Directory.GetParent(root_unity).ToString();

        string hotfixDir = Path.Combine(root, hotfixShared);
        string serverDir = Path.Combine(root, serverShared);

        try
        {
            string[] csList = Directory.GetFiles(srcPath, "*.cs");

            // 拷贝cs文件
            foreach (string f in csList)
            {
                // Remove path from the file name.
                string fName = f.Substring(srcPath.Length + 1);
                Debug.Log($"fName={fName}");

                // 会覆盖目标文件夹的文件
                File.Copy(Path.Combine(srcPath, fName), Path.Combine(outPath, fName), true);
            }

            // 删除源文件
            //foreach (string f in csList)
            //{
            //    File.Delete(f);
            //}
        }
        catch (DirectoryNotFoundException dirNotFound)
        {
            Debug.Log(dirNotFound.Message);
        }
    }
    [MenuItem("Tools/测试/CMD", false, 13)]
    static void TestCMD()
    {
        ExecuteCommand(@"ipconfig /flushdns");
        //ExecuteCommand(@"ping www.baidu.com");
    }
    [MenuItem("Tools/测试/清理临时文件夹", false, 14)]
    static void ClearTmpFolders()
    {
        // 两个需要清理的目录
        // ./Assets/StreamingAssets/Bundles
        // ./StandaloneWindows64
        string[] pathArray = new string[]
        {
            Path.Combine(Application.streamingAssetsPath, "Bundles"),
            Path.Combine(GetUnityDir(), "StandaloneWindows64"),
        };
        for (int i = 0; i < pathArray.Length; i++)
        {
            var path = pathArray[i];
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
        Debug.Log("清理完成");
    }
    [MenuItem("Tools/测试/取消读条", false, 15)]
    static void CancelableProgressBar()
    {
        EditorUtility.ClearProgressBar();
    }

    #endregion

    #region 热更新

    [MenuItem("Tools/热更新/生成Proto", false, 21)]
    static void ConvertProto()
    {
        //RunBatch("convert_proto.bat");
        InnerProto2CS.Proto2CS();
    }
    [MenuItem("Tools/热更新/编译热更工程", false, 22)]
    static void CompileHotFix()
    {
        ExecuteBatch("compile_hotfix.bat");
    }
    [MenuItem("Tools/热更新/MoveDLL", false, 23)]
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

    [MenuItem("Tools/Shader/重置IncludedShaders", false, 31)]
    static void ResetIncludedShaders() { }
    [MenuItem("Tools/Shader/设置IncludedShaders", false, 31)]
    static void SetIncludedShaders() { }

    [MenuItem("Assets/Open Server Project", false)]
    static void OpenServerProject()
    {
        string currDir = Directory.GetCurrentDirectory();
        DirectoryInfo currDirInfo = new DirectoryInfo(currDir);
        string projPath = $@"{currDirInfo.Parent}\NetCoreServer\NetCoreApp.sln";

        Process proc = new Process();
        proc.StartInfo.FileName = projPath;
        proc.StartInfo.CreateNoWindow = true;
        proc.Start();
    }
    [MenuItem("Tools/服务器/服务器AB资源存放目录", false, 41)]
    static void OpenServerAB()
    {
        Process.Start("explorer.exe", GetServerDir());
    }
    [MenuItem("Tools/服务器/运行时AB资源下载目录", false, 42)]
    static void OpenAppAB()
    {
        string path = Application.persistentDataPath.Replace("/", @"\"); //向左的[/]无法打开，要转成[\]
        Process.Start("explorer.exe", path);
    }
    [MenuItem("Tools/服务器/启动服务器", false, 43)]
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
}
#endif