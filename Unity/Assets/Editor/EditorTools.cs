using System;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

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
        if (GUILayout.Button("SnapShot"))
        {
            string fileName = $"{Application.streamingAssetsPath}/Actor_{DateTime.Now.ToString("yyyyMMddhhmmss")}.png";
            ScreenCapture.CaptureScreenshot(fileName);
            Debug.Log(fileName);
            AssetDatabase.Refresh();
        }
    }
}