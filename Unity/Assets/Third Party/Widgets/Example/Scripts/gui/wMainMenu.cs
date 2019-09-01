using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using eeGames.Widget;

public class wMainMenu : eeGames.Widget.Widget
{
    [SerializeField] private Button m_settingButton;
    [SerializeField] private Button m_miniGameButton;

    #region UNity Methods
    protected override void Awake()
    {
        m_settingButton.onClick.AddListener(OnSettingButtonClick);
        m_miniGameButton.onClick.AddListener(OnMiniGameButtonClick);
    }

    void OnDestroy()
    {
        m_settingButton.onClick.RemoveListener(OnSettingButtonClick);
        m_miniGameButton.onClick.RemoveListener(OnMiniGameButtonClick);
        base.DestroyWidget();
    }

    #endregion

    #region Helper Method

    void OnSettingButtonClick()
    {
        WidgetManager.Instance.Push(WidgetName.Setting, false, true);
    }

    void OnMiniGameButtonClick()
    {
        WidgetManager.Instance.Push(WidgetName.MainGame);
    }

    #endregion

    #region Widget Implementation

    #endregion

    #region Utitity Methods

    /// <summary>
    /// this method is used by MiniGame
    /// </summary>
    /// <returns> randomly generated number between 1 - 99 </returns>
    public int GetRandomGeneratedNumber()
    {
        return UnityEngine.Random.Range(1, 99);
    }

    #endregion
}
