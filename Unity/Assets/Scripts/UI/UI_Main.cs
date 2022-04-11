using UnityEngine;
using UnityEngine.UI;

public class UI_Main : UIBase
{
    [SerializeField] Button m_MatchButton;
    [SerializeField] Button m_SingleButton;
    [SerializeField] Button m_SettingsButton;

    void Awake()
    {
        m_MatchButton = transform.Find("MatchBtn").GetComponent<Button>();
        m_SingleButton = transform.Find("SingleBtn").GetComponent<Button>();
        m_SettingsButton = transform.Find("SettingsBtn").GetComponent<Button>();

        m_MatchButton.onClick.AddListener(OnMatchBtnClick);
        m_SingleButton.onClick.AddListener(OnSingleBtnClick);
        m_SettingsButton.onClick.AddListener(OnSettingsBtnClick);
    }

    void OnMatchBtnClick()
    {

    }

    void OnSingleBtnClick()
    {
        UIManager.Get().Push<UI_Game>();
    }

    void OnSettingsBtnClick()
    {

    }
}