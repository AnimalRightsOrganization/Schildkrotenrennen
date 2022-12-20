using System.IO;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using Debug = UnityEngine.Debug;

public class EnvWindow : EditorWindow
{
    static EnvWindow window;

    public static void ShowWindow()
    {
        Rect rect = new Rect(0, 0, 300, 200);
        window = (EnvWindow)GetWindowWithRect(typeof(EnvWindow), rect, true, "开发环境");
        window.Show();
    }

    const string notepad_exe = @"C:\Program Files\Notepad++\notepad++.exe";
    const string hosts_path = @"C:\Windows\System32\drivers\etc\hosts";
    static bool is_www = true; //默认
    static bool is_editing = false; //是否正在编辑

    const string yes_use_ab = "USE_ASSETBUNDLE";
    const string not_use_ab = "";
    static NamedBuildTarget target = NamedBuildTarget.Standalone;
    static bool use_ab = false; //默认

    void OnEnable()
    {
        CheckNetwork();

        CheckUseAB();
    }

    void OnGUI()
    {
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        {
            GUILayout.Space(10);
            GUILayout.Label("当前网络", GUILayout.Width(80));
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
            GUILayout.Label("是否用热更新", GUILayout.Width(80));
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

    void OnFocus()
    {
        if (is_editing == true)
        {
            Debug.Log("Focus: 检查网络");
            is_editing = false;
            CheckNetwork();
        }
    }

    void OnLostFocus()
    {

    }

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