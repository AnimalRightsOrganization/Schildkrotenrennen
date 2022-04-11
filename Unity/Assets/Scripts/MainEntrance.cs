using UnityEngine;

public class MainEntrance : MonoBehaviour
{
    void Awake()
    {
        //Screen.SetResolution(540, 960, true); //设置分辨率
        Screen.SetResolution(360, 640, true); //设置分辨率
        Screen.fullScreen = false; //设置成全屏
    }

    void Start()
    {
        //if (Code.Client.Client.GetInstance().m_PlayerManager.LocalPlayer == null)
        //{
        //    //Debug.Log("跳转到Client场景就执行一次");
        //    UIManager.Get().Push<UI_Main>();
        //}
        
        UIManager.Get().Push<UI_Main>();
    }
}