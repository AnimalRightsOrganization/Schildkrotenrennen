using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
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
        "\n打包执行了资源打包+部署到模拟的本地和服务器，需要进一步更新服务器配置文件。" +
        "\n部署时通过POST通知PHP执行更新SQL、在服务器目录下生成新的present.json。同时生成一条部署记录。" +
        "\n即json文件和SQL同时有记录版本作用。";

    void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Box($"目标平台：{ConstValue.PLATFORM_NAME}");


        GUILayout.Space(10); //上10
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
        //GUILayout.Box("开发版本", GUILayout.Width(100));
        if (GUILayout.Button("开发版本", GUILayout.Width(100)))
        {
            l_app_version = Application.version;
            string ver_path = $"{Application.dataPath}/res_version.txt";
            l_res_version = File.ReadAllText(ver_path);
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
        if (GUILayout.Button("打包", GUILayout.Width(100), GUILayout.Height(40)))
        {
            BuildTarget type = (BuildTarget)System.Enum.Parse(typeof(BuildTarget), ConstValue.PLATFORM_NAME);
            //Debug.Log($"type={type}");
            BundleTools.Build_Target(type);
        }
        GUILayout.BeginVertical(GUILayout.Height(50));
        {
            GUILayout.BeginHorizontal("远程");
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
            GUILayout.BeginHorizontal("本地");
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
        GUILayout.Space(10);
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("生成版本文件", GUILayout.Width(100)))
        {
            BundleTools.CreatePresent();
        }
        GUILayout.Space(10);
        GUILayout.EndHorizontal();


        GUILayout.Space(35);
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.TextArea(readme, GUILayout.Width(620), GUILayout.Height(100));
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

    // 查询数据库，当前游戏版本
    static async Task<string> GetAppInfo()
    {
        string api = $"http://restapi.moegijinka.cn/api/v1/GameCenter/game_data?name={Application.productName}";
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(api);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        //return responseBody;
        var obj = JsonMapper.ToObject<ServerResponse>(responseBody);
        //return obj.data;
        var model = JsonMapper.ToObject<DBApp>(obj.data);
        //return model;
        r_app_version = model.app_version;
        r_res_version = model.res_version;
        return $"主包:{model.app_version}, 资源:{model.res_version}";
    }
    static async void AppInfo()
    {
        await GetAppInfo();
    }
}