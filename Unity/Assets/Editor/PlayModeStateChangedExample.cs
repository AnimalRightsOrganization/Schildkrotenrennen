using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
/*
[InitializeOnLoadAttribute]
public static class PlayModeStateChangedExample
{
    static PlayModeStateChange m_State;

    [DidReloadScripts]
    public static void Listen()
    {
        Debug.Log($"监听到脚本编译了，是否在运行：{Application.isPlaying}");

        //if ((int)m_State >= 2)
        //if (Application.isPlaying)
        //IPCManager.Instance.Close(); //避免卡死
    }

    static PlayModeStateChangedExample()
    {
        EditorApplication.playModeStateChanged += LogPlayModeState;
    }

    private static void LogPlayModeState(PlayModeStateChange state)
    {
        m_State = state;
        //Debug.Log($"--------------------------------状态：{state}");
    }
}
*/