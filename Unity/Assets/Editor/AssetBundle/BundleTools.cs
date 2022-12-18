using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

public partial class BundleTools : Editor
{
    #region 标记
    private static void SetLabels()
    {
        // 移除所有没有使用的标记
        AssetDatabase.RemoveUnusedAssetBundleNames();

        // 1. 找到资源所在的文件夹
        DirectoryInfo directoryInfo = new DirectoryInfo(ConstValue.srcPath);
        DirectoryInfo[] typeDirectories = directoryInfo.GetDirectories(); //子文件夹

        // 2. 遍历里面每个子文件夹
        foreach (DirectoryInfo childDirectory in typeDirectories)
        {
            string typeDirectory = ConstValue.srcPath + "/" + childDirectory.Name;
            DirectoryInfo sceneDirectoryInfo = new DirectoryInfo(typeDirectory); //一级目录
            //Debug.Log("<color=red>" + typeDirectory + "</color>");

            // 错误检测
            if (sceneDirectoryInfo == null)
            {
                Debug.LogError(typeDirectory + "不存在");
                return;
            }
            else
            {
                Dictionary<string, string> namePathDict = new Dictionary<string, string>();

                // 3. 遍历子文件夹里的所有文件系统
                string typeName = Path.GetFileName(typeDirectory);
                onSceneFileSystemInfo(sceneDirectoryInfo, typeName, namePathDict);
            }
        }
        AssetDatabase.Refresh();
        Debug.LogWarning("设置成功");
    }
    private static void CleanLabels()
    {
        // 获取所有的AssetBundle名称
        string[] abNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < abNames.Length; i++)
        {
            // 强制删除所有AssetBundle名称
            AssetDatabase.RemoveAssetBundleName(abNames[i], true);
        }
    }

    private static string getBundleName(FileInfo fileInfo, string typeName)
    {
        string windowPath = fileInfo.FullName;
        string unityPath = windowPath.Replace(@"\", "/"); //转斜杠 C:/Users/Administrator/Documents/GitHub/AssetBundleExample/Assets/Sources/Textures/trash_2.jpg

        int Index = unityPath.IndexOf(typeName) + typeName.Length;
        string bundlePath = unityPath.Substring(Index + 1);

        var array = bundlePath.Split('.');
        string bundlePathWithoutExt = array[0];
        string result = Path.Combine(typeName, bundlePathWithoutExt);
        //Debug.Log(result);
        return result;
    }
    private static void setLable(FileInfo fileInfo, string typeName, Dictionary<string, string> namePathDict)
    {
        // 忽视unity自身生成的meta文件
        if (fileInfo.Extension == ".meta") return;
        //Debug.Log(fileInfo); // ..\v2\Lightmap\home\Lightmap-0_comp_light.exr  => Lightmap\Lightmap-0_comp_light.exr

        string bundleName = getBundleName(fileInfo, typeName); //sofa_1.mat
        //Debug.Log(bundleName); // 最终结果

        int index = fileInfo.FullName.IndexOf("Assets");
        string assetPath = fileInfo.FullName.Substring(index); // Assets/Sources/Materials/sofa_1.mat
        //Debug.Log(assetPath);

        // 6. 修改名称和后缀
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
        assetImporter.assetBundleName = bundleName;
        if (fileInfo.Extension == ".unity")
        {
            assetImporter.assetBundleVariant = "u3d"; //场景文件
        }
        else
        {
            assetImporter.assetBundleVariant = "unity3d"; //资源文件
        }

        // 添加到字典
        string folderName = "";
        if (bundleName.Contains("/"))
        {
            folderName = bundleName.Split('/')[1];
        }
        else
        {
            folderName = bundleName;
        }

        string bundlePath = assetImporter.assetBundleName + "." + assetImporter.assetBundleVariant;
        if (!namePathDict.ContainsKey(folderName))
            namePathDict.Add(folderName, bundlePath);
    }
    private static void onSceneFileSystemInfo(FileSystemInfo fileSystemInfo, string typeName, Dictionary<string, string> namePathDict)
    {
        if (!fileSystemInfo.Exists)
        {
            Debug.LogError(fileSystemInfo.FullName + ":不存在");
            return;
        }
        DirectoryInfo directoryInfo = fileSystemInfo as DirectoryInfo;
        FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
        foreach (var tempfileInfo in fileSystemInfos)
        {
            FileInfo fileInfo = tempfileInfo as FileInfo;
            if (fileInfo == null)
            {
                // 4. 如果找到的是文件夹, 递归直到没有文件夹
                DirectoryInfo dirInfo = tempfileInfo as DirectoryInfo; //二级目录
                //Debug.Log("强转失败，是文件夹:" + dirInfo);
                onSceneFileSystemInfo(tempfileInfo, typeName, namePathDict);
            }
            else
            {
                // 5. 找到文件, 修改他的 AssetLabels
                //Debug.Log("是文件");
                setLable(fileInfo, typeName, namePathDict);
            }
        }
    }
    #endregion

    #region 打包
    private static void BuildAssetBundles(BuildTarget target)
    {
        //设置标签
        SetLabels();

        //删除/创建目录
        if (Directory.Exists(ConstValue.outputPath1st))
            Directory.Delete(ConstValue.outputPath1st, true);
        Directory.CreateDirectory(ConstValue.outputPath1st);

        //打包
        //BuildPipeline.BuildAssetBundles(ConstValue.outputPath1st, 0, BuildTarget.Android);
        //BuildPipeline.BuildAssetBundles(ConstValue.outputPath1st, 0, EditorUserBuildSettings.activeBuildTarget);
        BuildPipeline.BuildAssetBundles(ConstValue.outputPath1st, 0, target);
        Debug.Log($"第一次打包到: {ConstValue.outputPath1st}");

        //清理标签
        CleanLabels();

        //生成配置文件assets.bytes
        MakeVersion();

        //转移到新目录
        MakeMD5();

        //删除第一次打包目录
        Directory.Delete(ConstValue.outputPath1st, true);
    }

    //manifest生成配置，资源版本
    private static void MakeVersion()
    {
        string assetBundlePath = Path.Combine(ConstValue.outputPath1st, ConstValue.PATCH_NAME);
        if (!File.Exists(assetBundlePath))
        {
            Debug.LogError($"assetBundle不存在: {assetBundlePath}");
            return;
        }
        string manifestPath = Path.Combine(ConstValue.outputPath1st, $"{ConstValue.PATCH_NAME}.manifest");
        if (!File.Exists(manifestPath))
        {
            Debug.LogError("manifest不存在");
            return;
        }

        //解析*.manifest
        var bundle = AssetBundle.LoadFromFile(assetBundlePath);
        AssetBundleManifest manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        string[] bundles = manifest.GetAllAssetBundles();

        string res_version_txt = $"{Application.dataPath}/res_version.txt";
        if (File.Exists(res_version_txt) == false)
        {
            File.WriteAllText(res_version_txt, "1");
        }
        int res_version = int.Parse(File.ReadAllText(res_version_txt));
        List<ABInfo> ABInfoList = new List<ABInfo>();
        for (int i = 0; i < bundles.Length; i++)
        {
            //计算文件md5，写入json
            string filePath = Path.Combine(ConstValue.outputPath1st, bundles[i]);
            string md5 = Md5Utils.GetMD5HashFromFile(filePath);
            string[] depends = manifest.GetAllDependencies(bundles[i]);
            ABInfo fs = new ABInfo(bundles[i], md5, depends);
            ABInfoList.Add(fs);
        }
        AssetsBytes data = new AssetsBytes(res_version, ABInfoList);
        //string jsonStr = JsonMapper.ToJson(data);
        string jsonStr = JsonConvert.SerializeObject(data);
        //Debug.Log(jsonStr);

        // 压缩包释放掉
        bundle.Unload(false);
        bundle = null;

        string assetsPath = Path.Combine(ConstValue.outputPath1st, "assets.bytes");
        File.WriteAllText(assetsPath, jsonStr);
        File.WriteAllText(res_version_txt, res_version.ToString());
    }

    //MD5重命名导出
    private static void MakeMD5()
    {
        DirectoryInfo dirInfo = new DirectoryInfo(ConstValue.outputPath1st);
        if (!Directory.Exists(ConstValue.outputPath1st))
        {
            Debug.LogError("路径不存在");
            return;
        }
        FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);
        Debug.Log($"文件总数={files.Length}，预计完成后文件数={(files.Length - 1) / 2 + 1}");

        if (Directory.Exists(ConstValue.outputPath2nd))
            Directory.Delete(ConstValue.outputPath2nd, true);
        Directory.CreateDirectory(ConstValue.outputPath2nd);

        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Name.EndsWith(".meta") || 
                files[i].Name.EndsWith(".manifest")) continue;
            //Debug.Log(i + "---" + files[i].FullName + "\n" + GetMD5HashFromFile(files[i].FullName));

            string srcFilePath = files[i].FullName;
            string dstFilePath = Path.Combine(ConstValue.outputPath2nd, files[i].Name);
            if (files[i].Name.EndsWith(".unity3d"))
            {
                dstFilePath = Path.Combine(ConstValue.outputPath2nd, Md5Utils.GetMD5HashFromFile(files[i].FullName) + ".unity3d");
            }
            File.Copy(srcFilePath, dstFilePath);
        }
    }
    #endregion

    #region 目标平台
    public static void Build_Target(BuildTarget target)
    {
        Debug.Log($"打包{target}平台资源");
        if (!EditorUserBuildSettings.activeBuildTarget.Equals(target))
        {
            Debug.LogError("请先切换平台");
            EditorUtility.DisplayDialog("目标平台与当前平台不一致，请先进行平台转换", "当前平台：" + EditorUserBuildSettings.activeBuildTarget + $"\n目标平台：{target}", "OK");
            return;
        }

        BuildAssetBundles(target);

        Deploy(target);

        Debug.Log("打包完成");
    }
    private static void Deploy(BuildTarget target)
    {
        if (!Directory.Exists(ConstValue.outputPath2nd))
        {
            Debug.LogError($"目录不存在：{ConstValue.outputPath2nd}");
            return;
        }

        //本地部署
        string appPath = $@"{Application.persistentDataPath}\{target}";
        if (Directory.Exists(appPath))
            Directory.Delete(appPath, true);
        CopyFolder(ConstValue.outputPath2nd, appPath);
        Debug.Log($"本地部署完成\n{ConstValue.outputPath2nd}--->\n{appPath}");

        //远程部署
        string wwwPath = $@"{ConstValue.GetDeployRes}\{target}";
        if (Directory.Exists(wwwPath))
            Directory.Delete(wwwPath, true);
        CopyFolder(ConstValue.outputPath2nd, wwwPath);
        Debug.Log($"远程部署完成\n{ConstValue.outputPath2nd}--->\n{wwwPath}");

        //删除输出目录
        Directory.Delete(ConstValue.outputPath2nd, true);
    }
    private static void CopyFolder(string strFromPath, string strToPath)
    {
        //如果源文件夹不存在，则创建
        if (!Directory.Exists(strFromPath))
        {
            Directory.CreateDirectory(strFromPath);
        }
        if (!Directory.Exists(strToPath))
        {
            Directory.CreateDirectory(strToPath);
        }
        //创建数组保存源文件夹下的文件名
        string[] strFiles = Directory.GetFiles(strFromPath);
        //循环拷贝文件
        for (int i = 0; i < strFiles.Length; i++)
        {
            //取得拷贝的文件名，只取文件名，地址截掉。
            string strFileName = strFiles[i].Substring(strFiles[i].LastIndexOf("\\") + 1, strFiles[i].Length - strFiles[i].LastIndexOf("\\") - 1);
            //开始拷贝文件,true表示覆盖同名文件
            File.Copy(strFiles[i], strToPath + "\\" + strFileName, true);
        }
        //创建DirectoryInfo实例
        DirectoryInfo dirInfo = new DirectoryInfo(strFromPath);
        //取得源文件夹下的所有子文件夹名称
        DirectoryInfo[] ZiPath = dirInfo.GetDirectories();
        for (int j = 0; j < ZiPath.Length; j++)
        {
            //获取所有子文件夹名
            string strZiPath = strFromPath + "\\" + ZiPath[j].ToString();
            //把得到的子文件夹当成新的源文件夹，从头开始新一轮的拷贝
            CopyFolder(strZiPath, strToPath);
        }
    }
    #endregion
}