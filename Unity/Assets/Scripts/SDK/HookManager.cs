using UnityEngine;
using Newtonsoft.Json;

public class HookManager : MonoBehaviour
{
    public static HookManager Get;

    private AndroidHook hook;

    void Awake()
    {
        Get = this;
        DontDestroyOnLoad(this.gameObject);

#if UNITY_ANDROID
        hook = new AndroidHook(gameObject);
#elif UNITY_IOS
        //hook = new iOSHook(gameObject);
#else
        hook = null;
#endif
    }

    void OnDestroy()
    {
        hook?.Dispose();
    }

    public bool CheckInstall()
    {
        return hook.CheckInstall();
    }

    public void JumpActivity()
    {
        hook.JumpActivity();
    }

    // 字符串消息返回
    public void JavaToUnity(string token)
    {
        Debug.Log($"token recv: {token}");
        Client.GameManager.Token = token;
    }
    // json消息返回
    public void JsonToUnity(string json)
    {
        var obj = JsonConvert.DeserializeObject<MoeCallback>(json);
        Debug.Log($"JsonToUnity: {obj.code}, {obj.data}");
        switch (obj.code)
        {
            case 0: //0:大厅主动发
                Client.GameManager.Token = obj.data;
                break;
            case 1: //1:游戏请求后发
                // 如果是主动请求的
                Client.GameManager.Token = obj.data;
                Client.ILGlobal.Instance.callHotFix("HotFix.Main", "LoginToken", null, Client.GameManager.Token);
                break;
        }
    }
}