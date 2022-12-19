using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Debug = UnityEngine.Debug;
using Client;

public class TestWindow : EditorWindow
{
    static TestWindow window;

    public static void ShowWindow()
    {
        window = (TestWindow)GetWindow(typeof(TestWindow));
        window.titleContent = new GUIContent("调试窗口");
        window.Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("test5"))
        {
            var ui_login = GameObject.Find("UI_Login");
            var usrInput = ui_login.transform.Find("LoginPanel/UserInput").GetComponent<InputField>();
            usrInput.text = "test5";
            var pwdInput = ui_login.transform.Find("LoginPanel/PwdInput").GetComponent<InputField>();
            pwdInput.text = "123456";
        }
        if (GUILayout.Button("test6"))
        {
            var ui_login = GameObject.Find("UI_Login");
            var usrInput = ui_login.transform.Find("LoginPanel/UserInput").GetComponent<InputField>();
            usrInput.text = "test6";
            var pwdInput = ui_login.transform.Find("LoginPanel/PwdInput").GetComponent<InputField>();
            pwdInput.text = "123456";
        }
        if (GUILayout.Button("Push<UI>"))
        {
            //ILGlobal.Instance.appdomain.Invoke("HotFix.Main", "PushUI", ILGlobal.Instance.gameObject, null);
            ILGlobal.Instance.appdomain.Invoke("HotFix.Main", "PushUI", null, null); //没有对象，静态
        }
        if (GUILayout.Button("SnapShot"))
        {
            string fileName = $"{Application.streamingAssetsPath}/Actor_{DateTime.Now.ToString("yyyyMMddhhmmss")}.png";
            ScreenCapture.CaptureScreenshot(fileName);
            Debug.Log(fileName);
            AssetDatabase.Refresh();
        }
    }
}