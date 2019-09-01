using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using eeGames.Widget;

public class wPopUp : Widget
{
    [SerializeField]
    private Button m_closeButton;

    #region UNity Methods

    protected override void Awake() 
    {
        base.Awake();
        m_closeButton.onClick.AddListener(OnCloseButtonClick); 
    }
    void OnDestroy() { Reset(); }
    #endregion

    #region Helper Method
    void OnCloseButtonClick()
    {
        // 
        WidgetManager.Instance.Pop(false);
    }
    #endregion

    #region Widget Implementation 
   

    public  void Reset()
    {
        m_closeButton.onClick.RemoveListener(OnCloseButtonClick);
    }

   
    public override void UpdateWidget()
    {

    }
    #endregion

    
}

