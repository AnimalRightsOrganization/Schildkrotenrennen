using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using eeGames.Widget;

public class wSetting : Widget
{
    [SerializeField] private Button m_audioButton;
    [SerializeField] private Button m_gameButton;
    [SerializeField] private Button m_saveButton;
    [SerializeField] private Button m_closeButton;

    #region UNity Methods

    protected override void Awake()
    {
        m_audioButton.onClick.AddListener(OnAudioButtonClick);
        m_closeButton.onClick.AddListener(OnCloseButtonClick);
       
    }

    void OnDestroy()
    {
        m_audioButton.onClick.RemoveListener(OnAudioButtonClick);
        m_closeButton.onClick.RemoveListener(OnCloseButtonClick);
        base.DestroyWidget();
    }

    #endregion

    #region Helper Method

    void OnAudioButtonClick()
    {
        WidgetManager.Instance.Push(WidgetName.AudioSetting, false, true);
    }

    private void OnCloseButtonClick()
    {
        WidgetManager.Instance.Pop(false);
    }

    #endregion
}
