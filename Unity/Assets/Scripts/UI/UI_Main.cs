using UnityEngine;
using UnityEngine.UI;

public class UI_Main : UIBase
{
    [SerializeField] Button m_JoinButton;
    [SerializeField] Button m_CreateButton;
    [SerializeField] Button m_SettingsButton;

    void Awake()
    {
        m_JoinButton = transform.Find("MatchBtn").GetComponent<Button>();
        m_CreateButton = transform.Find("SingleBtn").GetComponent<Button>();
        m_SettingsButton = transform.Find("SettingsBtn").GetComponent<Button>();

        m_JoinButton.onClick.AddListener(OnJoinBtnClick);
        m_CreateButton.onClick.AddListener(OnCreateBtnClick);
        m_SettingsButton.onClick.AddListener(OnSettingsBtnClick);
    }

    void OnJoinBtnClick()
    {
        //TODO: 弹出大厅列表
    }

    void OnCreateBtnClick()
    {
        //TODO: 弹出创建游戏选项，人数、人机、是否公开
        var obj = new GameObject("TEMP_Local");
        var script = obj.AddComponent<TEMP_Local>();
        UIManager.Get().Push<UI_Game>();
    }

    void OnSettingsBtnClick()
    {

    }
}