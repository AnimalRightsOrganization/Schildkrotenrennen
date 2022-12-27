using System;
using UnityEngine;

public class AndroidHook : IDisposable
{
    private const string activityName = "com.moegijinka.noactivity.OverrideExample"; //OverrideExample extends UnityPlayerActivity
    private const string className = "com.moegijinka.noactivity.TestClass"; //TestClass.java
    private const string jumpActivityName = "com.moegijinka.gamecenter";
    private const string jumpClassName = "com.moegijinka.gamecenter.NormalActivity";

    private AndroidJavaClass jc = null;
    private AndroidJavaObject jo = null;
    public AndroidHook(GameObject go)
    {
        try
        {
            jo = new AndroidJavaObject(activityName);
            Debug.Log($"jo exist: {jo != null}");
            jo.CallStatic("GetInstance", go.name);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void Dispose()
    {
        jc?.Dispose();
        jc = null;
        jo?.Dispose();
        jo = null;
    }

    #region 方法
    public bool CheckInstall()
    {
        bool result = false;
        try
        {
            result = jo.Call<bool>("checkInstall", jumpActivityName);
            Debug.Log($"checkInstall: {result}");
        }
        catch (Exception e)
        {
            Debug.LogError($"error checkInstall: {e}");
        }
        return result;
    }
    public void JumpActivity()
    {
        try
        {
            jo.Call("onJumpActivity", jumpActivityName, jumpClassName, "");
            Debug.Log("call onJumpActivity");
        }
        catch (Exception e)
        {
            Debug.LogError($"error onJumpActivity: {e}");
        }
    }
    #endregion
}