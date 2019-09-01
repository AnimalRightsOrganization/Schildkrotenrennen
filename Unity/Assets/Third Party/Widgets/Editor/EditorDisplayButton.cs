using eeGames.Actor;
using UnityEditor;
using UnityEngine;


//public enum ActingType
//{
//    Position,
//    Scale,
//    Rotation
//}

public class SubmitButtonStyle
{
    public System.Action SubmitSuccess;
    public string SubmitTitle;
    public int SubmitButtonWidth;
    public int SubmitButtonHeight;

    public SubmitButtonStyle(System.Action submitSuccess = null, string title = "", int width = 0, int height = 0)
    {
        SubmitSuccess = submitSuccess;
        SubmitTitle = title;
        SubmitButtonWidth = width;
        SubmitButtonHeight = height;
    }
}

public class CustomButtonStyle
{
    public string ButtonTitle;
    public Texture2D ButtonIcon;
    public Texture2D ButtonUp;
    public Texture2D ButtonDown;
    public System.Action ButtonSubmitAction;
    public int ButtonHeight;
    public GUIStyle ButtonStyle;

    public CustomButtonStyle(string title, Texture2D up, Texture2D down, System.Action submitAction = null, int height = 50, Texture2D icon = null, GUIStyle style = null)
    {
        ButtonTitle = title;
        ButtonIcon = icon;
        ButtonUp = up;
        ButtonDown = down;
        ButtonSubmitAction = submitAction;
        ButtonHeight = height;
        ButtonStyle = style;
    }
}

public class EditorDisplayButton
{

    protected System.Action _submitCallback;
    protected Texture2D _buttonUp;
    protected Texture2D _buttonDown;
    protected Texture2D _buttonIcon;
    protected Texture2D _activeTex;
    protected Texture2D _prevTex;
    protected GUIContent _buttonContent;
    protected GUIStyle _buttonStyle;
    protected Rect _buttonRect;

    //protected int _buttonWidth;
    protected int _buttonHeight;

    protected string _buttonTitle;
    protected string _buttonText;

    //Submit button
    protected bool _submitButtonVisibile = false;
    protected string _submitButtonTitle;
    protected System.Action _submitButtonAction;
    protected int _submitButtonWidth;
    protected int _submitButtonHeight;

    public EditorDisplayButton(CustomButtonStyle buttonStyle, SubmitButtonStyle submitStyle = null)
    {
        _buttonUp = buttonStyle.ButtonUp;
        _buttonDown = buttonStyle.ButtonDown;
        _buttonIcon = buttonStyle.ButtonIcon;
        _submitCallback = buttonStyle.ButtonSubmitAction;
        _buttonUp.wrapMode = TextureWrapMode.Repeat;
        _buttonContent = new GUIContent(_buttonUp);
        _activeTex = _buttonUp;

        //_buttonTextField = string.Empty;
        _buttonTitle = buttonStyle.ButtonTitle;

        if (submitStyle != null)
        {
            _submitButtonVisibile = true;
            _submitButtonTitle = submitStyle.SubmitTitle;
            _submitButtonAction = submitStyle.SubmitSuccess;
            _submitButtonWidth = submitStyle.SubmitButtonWidth;
            _submitButtonHeight = submitStyle.SubmitButtonHeight;
        }

        _buttonHeight = buttonStyle.ButtonHeight;

        if (buttonStyle.ButtonStyle != null)
            _buttonStyle = buttonStyle.ButtonStyle;
        else
        {
            _buttonStyle = new GUIStyle();

        }
    }

    public virtual void Update(Event windowEvent, GUIStyle fontStyle, Editor window, int width, int height)
    { }

    public virtual void UpdateBits(ushort position, ushort scale, ushort rotation)
    { }
}

public class PositionTweenButton : EditorDisplayButton
{
    #region Position Tween
    private bool _interpolationPrev = false;
    private bool _interpolation = false;

    private const int ADJUSTABLE_HEIGHT_MIN = -10;
    private const int ADJUSTABLE_HEIGHT_MAX = -5;

    public bool PositionTween
    {
        get { return _interpolation; }
        set
        {
            if (value != _interpolationPrev)
            {
                _interpolation = value;
                _interpolationPrev = value;

                if (_interpolationChanged != null)
                    _interpolationChanged(value);
            }
        }
    }
    private System.Action<bool> _interpolationChanged;
    #endregion

    #region Tween Time
    private float m_tweenTimePrev = 0;
    private float m_tweenTime = 0;
    public float TweenTime
    {
        get { return m_tweenTime; }
        set
        {
            if (value != m_tweenTimePrev)
            {
                m_tweenTime = value;
                m_tweenTimePrev = value;

                if (m_tweenTimeChanged != null)
                    m_tweenTimeChanged(value);
            }
        }
    }
    private System.Action<float> m_tweenTimeChanged;
    private WidgetTween m_widgetTween;
    private ActingType m_type;
    #endregion



    private string _toolTiptext;
    private bool _interpolationButtonVisible;

    public PositionTweenButton(CustomButtonStyle buttonStyle, SubmitButtonStyle submitStyle = null)
        : base(buttonStyle, submitStyle)
    {
    }

    public void Initialize(ActingType type, WidgetTween wTween, bool wTweens, float tweenTime, string tooltipText, System.Action<bool> positionChanged, System.Action<float> tweenTimeChanged)
    {
        PositionTween = wTweens;

        _toolTiptext = tooltipText;

        _interpolationChanged = positionChanged;
        m_tweenTimeChanged = tweenTimeChanged;


        if (!_submitButtonVisibile)
        {
            _submitButtonVisibile = true;
            _submitButtonTitle = PositionTween ? "Disable" : "Enable";
        }

        m_widgetTween = wTween;
        TweenTime = tweenTime;
        m_type = type;
    }

    public override void Update(Event windowEvent, GUIStyle fontStyle, Editor window, int width, int height)
    {
        if (_activeTex == null)
            return;

        Rect original = GUILayoutUtility.GetRect(_buttonContent, _buttonStyle, GUILayout.Height(_buttonHeight));
        _buttonRect = original;


        Rect textField = new Rect(original);
        textField.xMin += 10;
        textField.xMax -= width * 0.32f;
        textField.yMin += 10;
        textField.yMax -= 20;

        Rect iconField = new Rect(original);
        iconField.xMin += 10;
        iconField.xMax -= (width - 60);
        iconField.yMin += 5;
        iconField.yMax -= 35;

        Rect lerpSpeedLabelField = new Rect(original);
        lerpSpeedLabelField.xMin += 10;
        lerpSpeedLabelField.xMax -= (width - 100);
        lerpSpeedLabelField.yMin += 65;
        lerpSpeedLabelField.yMax -= 92;



        Rect lerpPositionField = new Rect(original);
        lerpPositionField.xMin += 10;
        //lerpPositionField.xMin += 65;
        lerpPositionField.xMax -= (width - (width * 0.35f));
        lerpPositionField.yMin += 105 + ADJUSTABLE_HEIGHT_MIN;
        lerpPositionField.yMax -= 65 + ADJUSTABLE_HEIGHT_MAX;

        Rect lerpPositionXField = new Rect(original);
        lerpPositionXField.xMin += (width - (width * 0.68f));
        lerpPositionXField.xMax -= (width - (width * 0.55f));
        lerpPositionXField.yMin += 105 + ADJUSTABLE_HEIGHT_MIN;
        lerpPositionXField.yMax -= 65 + ADJUSTABLE_HEIGHT_MAX;

        Rect lerpPositionYField = new Rect(original);
        lerpPositionYField.xMin += (width - (width * 0.48f));
        lerpPositionYField.xMax -= (width - (width * 0.75f));
        lerpPositionYField.yMin += 105 + ADJUSTABLE_HEIGHT_MIN;
        lerpPositionYField.yMax -= 65 + ADJUSTABLE_HEIGHT_MAX;

        Rect lerpPositionZField = new Rect(original);
        lerpPositionZField.xMin += (width - (width * 0.275f));
        lerpPositionZField.xMax -= (width - (width * 0.96f));
        lerpPositionZField.yMin += 105 + ADJUSTABLE_HEIGHT_MIN;
        lerpPositionZField.yMax -= 65 + ADJUSTABLE_HEIGHT_MAX;

        Rect lerpRotationField = new Rect(original);
        lerpRotationField.xMin += 10;
        //lerpRotationField.xMin += 65;
        lerpRotationField.xMax -= (width - (width * 0.35f));
        lerpRotationField.yMin += 140 + ADJUSTABLE_HEIGHT_MIN - 10;
        lerpRotationField.yMax -= 30 + ADJUSTABLE_HEIGHT_MAX + 10;

        Rect lerpRotationXField = new Rect(original);
        lerpRotationXField.xMin += (width - (width * 0.68f));
        lerpRotationXField.xMax -= (width - (width * 0.55f));
        lerpRotationXField.yMin += 140 + ADJUSTABLE_HEIGHT_MIN - 10;
        lerpRotationXField.yMax -= 30 + ADJUSTABLE_HEIGHT_MAX + 10;

        Rect lerpRotationYField = new Rect(original);
        lerpRotationYField.xMin += (width - (width * 0.48f));
        lerpRotationYField.xMax -= (width - (width * 0.75f));
        lerpRotationYField.yMin += 140 + ADJUSTABLE_HEIGHT_MIN - 10;
        lerpRotationYField.yMax -= 30 + ADJUSTABLE_HEIGHT_MAX + 10;

        Rect lerpRotationZField = new Rect(original);
        lerpRotationZField.xMin += (width - (width * 0.275f));
        lerpRotationZField.xMax -= (width - (width * 0.96f));
        lerpRotationZField.yMin += 140 + ADJUSTABLE_HEIGHT_MIN - 10;
        lerpRotationZField.yMax -= 30 + ADJUSTABLE_HEIGHT_MAX + 10;

        Rect lerpScaleField = new Rect(original);
        lerpScaleField.xMin += 10;
        //lerpScaleField.xMin += 65;
        lerpScaleField.xMax -= (width - (width * 0.35f));
        lerpScaleField.yMin += 175 + ADJUSTABLE_HEIGHT_MIN - 20;
        lerpScaleField.yMax -= -5 + ADJUSTABLE_HEIGHT_MAX + 20;

        Rect lerpScaleXField = new Rect(original);
        lerpScaleXField.xMin += (width - (width * 0.68f));
        lerpScaleXField.xMax -= (width - (width * 0.55f));
        lerpScaleXField.yMin += 175 + ADJUSTABLE_HEIGHT_MIN - 20;
        lerpScaleXField.yMax -= -5 + ADJUSTABLE_HEIGHT_MAX + 20;

        Rect lerpScaleYField = new Rect(original);
        lerpScaleYField.xMin += (width - (width * 0.48f));
        lerpScaleYField.xMax -= (width - (width * 0.75f));
        lerpScaleYField.yMin += 175 + ADJUSTABLE_HEIGHT_MIN - 20;
        lerpScaleYField.yMax -= -5 + ADJUSTABLE_HEIGHT_MAX + 20;

        Rect lerpScaleZField = new Rect(original);
        lerpScaleZField.xMin += (width - (width * 0.275f));
        lerpScaleZField.xMax -= (width - (width * 0.96f));
        lerpScaleZField.yMin += 175 + ADJUSTABLE_HEIGHT_MIN - 20;
        lerpScaleZField.yMax -= -5 + ADJUSTABLE_HEIGHT_MAX + 20;


        if (windowEvent.isMouse && _buttonRect.Contains(windowEvent.mousePosition))
        {
            if (windowEvent.type == EventType.MouseDown)
            {
                if (_buttonDown != null)
                {
                    _activeTex = _buttonDown;
                    _buttonContent.image = _buttonUp;
                }
            }
            else if (windowEvent.type == EventType.MouseUp)
            {
                if (_buttonUp != null)
                {
                    _activeTex = _buttonUp;
                    _buttonContent.image = _buttonUp;
                }

                if (_submitCallback != null)
                    _submitCallback();
            }
        }
        else if (windowEvent.isMouse)
        {
            if (_buttonUp != null)
            {
                _activeTex = _buttonUp;
                _buttonContent.image = _buttonUp;
            }
        }

        GUI.DrawTexture(_buttonRect, _activeTex, ScaleMode.StretchToFill);

        if (_buttonIcon != null)
            GUI.DrawTexture(iconField, _buttonIcon, ScaleMode.StretchToFill);

        if (fontStyle != null)
            GUI.Label(_buttonRect, _buttonTitle, fontStyle);

        if (!string.IsNullOrEmpty(_toolTiptext))
        {
            GUI.color = Color.yellow;
            GUI.TextArea(textField, _toolTiptext);
            GUI.color = Color.white;
        }

        if (_submitButtonVisibile)
        {
            bool fullScreenButton = string.IsNullOrEmpty(_toolTiptext);

            Rect submitField = new Rect(original);
            submitField.xMin += !fullScreenButton ? (width * 0.68f) + _submitButtonWidth : 10 + _submitButtonWidth;
            submitField.xMax -= 10 - _submitButtonWidth;
            submitField.yMin += 20 + _submitButtonHeight;
            submitField.yMax -= 125 - _submitButtonHeight;

            Rect lerpSpeedLabel = new Rect(original);
            lerpSpeedLabel.xMin += 10;
            lerpSpeedLabel.xMax -= 10;
            lerpSpeedLabel.yMin += 52;
            lerpSpeedLabel.yMax -= 75;

            Rect lerpSpeedField = new Rect(original);
            lerpSpeedField.xMin += (width * 0.25f) + _submitButtonWidth;
            lerpSpeedField.xMax -= 10 - _submitButtonWidth;
            lerpSpeedField.yMin += 65 + _submitButtonHeight;
            lerpSpeedField.yMax -= 85;



            _submitButtonTitle = PositionTween ? "Disable" : "Enable";

            GUI.color = PositionTween ? Color.green : Color.gray;

            if (GUI.Button(submitField, _submitButtonTitle))
            {
                PositionTween = !PositionTween;
                _submitButtonTitle = PositionTween ? "Disable" : "Enable";

                if (_submitButtonAction != null)
                    _submitButtonAction();
            }
            GUI.color = Color.white;

            GUI.Label(lerpSpeedLabel, "Tween Time", fontStyle);



            GUI.enabled = PositionTween;

            TweenTime = GUI.HorizontalSlider(lerpSpeedField, TweenTime, 0f, 10f);  // TODO : Replace magic numbers with consts

            float tempLerpSpeed = TweenTime;
            if (float.TryParse(GUI.TextField(lerpSpeedLabelField, TweenTime.ToString()), out tempLerpSpeed))
            {
                TweenTime = tempLerpSpeed;
            }


            #region Lerp Position GUI


            if (GUI.Button(lerpPositionField, "From"))
            {
                switch (m_type)
                {
                    case ActingType.Position:
                        var newPos = m_widgetTween.gameObject.transform.position; //.GetComponent<RectTransform>()
                        Debug.Log("Pos : " + newPos);

                        string[] dimension = UnityEditor.UnityStats.screenRes.Split('x');
                        newPos.x /= System.Int32.Parse(dimension[0]);
                        newPos.y /= System.Int32.Parse(dimension[1]);

                        m_widgetTween.TweenData.sPosition.From = newPos;
                        break;
                    case ActingType.Scale:
                        m_widgetTween.TweenData.sScale.From = m_widgetTween.gameObject.transform.localScale;
                        break;
                    case ActingType.Rotation:
                        m_widgetTween.TweenData.sRotation.From = m_widgetTween.gameObject.transform.rotation.eulerAngles;
                        break;

                }

            }


            switch (m_type)
            {
                case ActingType.Position:
                    GUI.Button(lerpPositionXField, "X:" + (System.Math.Round(m_widgetTween.TweenData.sPosition.From.x, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpPositionYField, "Y:" + (System.Math.Round(m_widgetTween.TweenData.sPosition.From.y, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpPositionZField, "Z:" + (System.Math.Round(m_widgetTween.TweenData.sPosition.From.z, 2).ToString()));
                    GUI.color = Color.white;

                    break;
                case ActingType.Scale:
                    GUI.Button(lerpPositionXField, "X:" + (System.Math.Round(m_widgetTween.TweenData.sScale.From.x, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpPositionYField, "Y:" + (System.Math.Round(m_widgetTween.TweenData.sScale.From.y, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpPositionZField, "Z:" + (System.Math.Round(m_widgetTween.TweenData.sScale.From.z, 2).ToString()));
                    GUI.color = Color.white;
                    break;
                case ActingType.Rotation:

                    GUI.Button(lerpPositionXField, "X:" + (System.Math.Round(m_widgetTween.TweenData.sRotation.From.x, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpPositionYField, "Y:" + (System.Math.Round(m_widgetTween.TweenData.sRotation.From.y, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpPositionZField, "Z:" + (System.Math.Round(m_widgetTween.TweenData.sRotation.From.z, 2).ToString()));
                    GUI.color = Color.white;

                    break;
            }



            #endregion



            #region Lerp Rotation GUI

            if (GUI.Button(lerpRotationField, "To"))
            {
                switch (m_type)
                {
                    case ActingType.Position:
                        var newPos = m_widgetTween.gameObject.transform.position;//.GetComponent<RectTransform>()
                        string[] dimension = UnityEditor.UnityStats.screenRes.Split('x');
                        newPos.x /= System.Int32.Parse(dimension[0]);
                        newPos.y /= System.Int32.Parse(dimension[1]);

                        m_widgetTween.TweenData.sPosition.To = newPos;
                        break;
                    case ActingType.Scale:
                        m_widgetTween.TweenData.sScale.To = m_widgetTween.gameObject.transform.localScale;
                        break;
                    case ActingType.Rotation:
                        m_widgetTween.TweenData.sRotation.To = m_widgetTween.gameObject.transform.rotation.eulerAngles;
                        break;

                }
            }


            switch (m_type)
            {
                case ActingType.Position:
                    GUI.Button(lerpRotationXField, "X:" + (System.Math.Round(m_widgetTween.TweenData.sPosition.To.x, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpRotationYField, "Y:" + (System.Math.Round(m_widgetTween.TweenData.sPosition.To.y, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpRotationZField, "Z:" + (System.Math.Round(m_widgetTween.TweenData.sPosition.To.z, 2).ToString()));
                    GUI.color = Color.white;

                    break;
                case ActingType.Scale:
                    GUI.Button(lerpRotationXField, "X:" + (System.Math.Round(m_widgetTween.TweenData.sScale.To.x, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpRotationYField, "Y:" + (System.Math.Round(m_widgetTween.TweenData.sScale.To.y, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpRotationZField, "Z:" + (System.Math.Round(m_widgetTween.TweenData.sScale.To.z, 2).ToString()));
                    GUI.color = Color.white;
                    break;
                case ActingType.Rotation:

                    GUI.Button(lerpRotationXField, "X:" + (System.Math.Round(m_widgetTween.TweenData.sRotation.To.x, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpRotationYField, "Y:" + (System.Math.Round(m_widgetTween.TweenData.sRotation.To.y, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpRotationZField, "Z:" + (System.Math.Round(m_widgetTween.TweenData.sRotation.To.z, 2).ToString()));
                    GUI.color = Color.white;

                    break;
            }



            #endregion

            #region Lerp Scale GUI
            //  GUI.color = LerpScale ? Color.white : Color.gray;




            if (GUI.Button(lerpScaleField, "Hide"))
            {

                switch (m_type)
                {
                    case ActingType.Position:
                        var newPos = m_widgetTween.gameObject.transform.position; //.GetComponent<RectTransform>()
                        string[] dimension = UnityEditor.UnityStats.screenRes.Split('x');
                        newPos.x /= System.Int32.Parse(dimension[0]);
                        newPos.y /= System.Int32.Parse(dimension[1]);

                        m_widgetTween.TweenData.ePosition = newPos;
                        break;
                    case ActingType.Scale:
                        m_widgetTween.TweenData.eScale = m_widgetTween.gameObject.transform.localScale;
                        break;
                    case ActingType.Rotation:
                        m_widgetTween.TweenData.eRotation = m_widgetTween.gameObject.transform.rotation.eulerAngles;
                        break;

                }
            }


            switch (m_type)
            {
                case ActingType.Position:
                    GUI.Button(lerpScaleXField, "X:" + (System.Math.Round(m_widgetTween.TweenData.ePosition.x, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpScaleYField, "Y:" + (System.Math.Round(m_widgetTween.TweenData.ePosition.y, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpScaleZField, "Z:" + (System.Math.Round(m_widgetTween.TweenData.ePosition.z, 2).ToString()));
                    GUI.color = Color.white;

                    break;
                case ActingType.Scale:
                    GUI.Button(lerpScaleXField, "X:" + (System.Math.Round(m_widgetTween.TweenData.eScale.x, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpScaleYField, "Y:" + (System.Math.Round(m_widgetTween.TweenData.eScale.y, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpScaleZField, "Z:" + (System.Math.Round(m_widgetTween.TweenData.eScale.z, 2).ToString()));
                    GUI.color = Color.white;
                    break;
                case ActingType.Rotation:

                    GUI.Button(lerpScaleXField, "X:" + (System.Math.Round(m_widgetTween.TweenData.eRotation.x, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpScaleYField, "Y:" + (System.Math.Round(m_widgetTween.TweenData.eRotation.y, 2).ToString()));
                    GUI.color = Color.white;

                    GUI.Button(lerpScaleZField, "Z:" + (System.Math.Round(m_widgetTween.TweenData.eRotation.z, 2).ToString()));
                    GUI.color = Color.white;

                    break;
            }



            #endregion

            GUI.enabled = true;
        }

        if (_prevTex != _activeTex)
        {
            _prevTex = _activeTex;
            window.Repaint();
        }
    }
}



//public enum LoopType
//{
//    PingPong,
//    StartOver
//}
public abstract class CommonActorElements
{

    #region Enable
    private bool m_enablePrevious = false;
    private bool m_enablePlay = false;

    public bool Enable
    {
        get { return m_enablePlay; }
        set
        {
            if (value != m_enablePrevious)
            {
                m_enablePlay = value;
                m_enablePrevious = value;

                if (m_enableChanged != null)
                    m_enableChanged(value);
            }
        }
    }
    protected System.Action<bool> m_enableChanged;
    #endregion


    #region Tween Time
    private float m_tweenTimePrev = 0;
    private float m_tweenTime = 0;
    public float TweenTime
    {
        get { return m_tweenTime; }
        set
        {
            if (value != m_tweenTimePrev)
            {
                m_tweenTime = value;
                m_tweenTimePrev = value;

                if (m_tweenTimeChanged != null)
                    m_tweenTimeChanged(value);
            }
        }
    }
    protected System.Action<float> m_tweenTimeChanged;

    #endregion



    #region Tween delay Time
    private float m_tweenDelayTimePrev = 0;
    private float m_tweenDelayTime = 0;
    public float TweenDelayTime
    {
        get { return m_tweenDelayTime; }
        set
        {
            if (value != m_tweenDelayTimePrev)
            {
                m_tweenDelayTime = value;
                m_tweenDelayTimePrev = value;

                if (m_tweenDelayTimeChanged != null)
                    m_tweenDelayTimeChanged(value);
            }
        }
    }
    protected System.Action<float> m_tweenDelayTimeChanged;

    #endregion


    #region Auto PLay
    private bool m_autoPlayPrevious = false;
    private bool m_autoPlay = false;

    public bool AutoPlay
    {
        get { return m_autoPlay; }
        set
        {
            if (value != m_autoPlayPrevious)
            {
                m_autoPlay = value;
                m_autoPlayPrevious = value;

                if (m_autoPlayChanged != null)
                    m_autoPlayChanged(value);
            }
        }
    }
    protected System.Action<bool> m_autoPlayChanged;
    #endregion



    #region Tween Type
    private LeanTweenType m_tweenTypePrevious;
    private LeanTweenType m_tweenType;

    public LeanTweenType CTweenType
    {
        get { return m_tweenType; }
        set
        {
            if (value != m_tweenTypePrevious)
            {
                m_tweenType = value;
                m_tweenTypePrevious = value;

                if (m_tweenTypeChanged != null)
                    m_tweenTypeChanged(value);
            }
        }
    }
    protected System.Action<LeanTweenType> m_tweenTypeChanged;
    #endregion



    #region Acting Type
    private ActingType m_actingTypePrevious;
    private ActingType m_actingType;

    public ActingType ActingType
    {
        get { return m_actingType; }
        set
        {
            if (value != m_actingTypePrevious)
            {
                m_actingType = value;
                m_actingTypePrevious = value;

                if(m_actingType == eeGames.Actor.ActingType.Color)
                {
                    var obj = Selection.activeTransform.gameObject;
                    if( obj.GetComponent<CanvasGroup>() == null) obj.AddComponent<CanvasGroup>();
                }
                Title = m_actingType.ToString() + " Actor";
                if (m_actingTypeChanged != null)
                    m_actingTypeChanged(value);
            }
        }
    }
    protected System.Action<ActingType> m_actingTypeChanged;
    #endregion


    #region Loop Type
    private LoopType m_loopTypePrevious;
    private LoopType m_loopType;

    public LoopType LoopType
    {
        get { return m_loopType; }
        set
        {
            if (value != m_loopTypePrevious)
            {
                m_loopType = value;
                m_loopTypePrevious = value;

                if (m_loopTypeChanged != null)
                    m_loopTypeChanged(value);
            }
        }
    }
    protected System.Action<LoopType> m_loopTypeChanged;
    #endregion



    #region Loop
    private bool m_loopPrevious = false;
    private bool m_loop = false;

    public bool Loop
    {
        get { return m_loop; }
        set
        {
            if (value != m_loopPrevious)
            {
                m_loop = value;
                m_loopPrevious = value;

                if (m_loopChanged != null)
                    m_loopChanged(value);
            }
        }
    }
    protected System.Action<bool> m_loopChanged;
    #endregion




    #region PingPong
    private bool m_pingPongPrevious = false;
    private bool m_pingPong = false;

    public bool PingPong
    {
        get { return m_pingPong; }
        set
        {
            if (value != m_pingPongPrevious)
            {
                m_pingPong = value;
                m_pingPongPrevious = value;

                if (m_pingPongChanged != null)
                    m_pingPongChanged(value);
            }
        }
    }
    protected System.Action<bool> m_pingPongChanged;
    #endregion

    #region Tween Count
    private int m_tweenCountPrev = 0;
    private int m_tweenCount = 0;
    public int TweenCount
    {
        get { return m_tweenCount; }
        set
        {
            if (value != m_tweenCountPrev)
            {
                m_tweenCount = value;
                m_tweenCountPrev = value;

                if (m_tweenCountChanged != null)
                    m_tweenCountChanged(value);
            }
        }
    }
    protected System.Action<int> m_tweenCountChanged;

    #endregion




    #region title
    private string m_titlePrev;
    private string m_title;
    public string Title
    {
        get { return m_title; }
        set
        {
            if (value != m_titlePrev)
            {
                m_title = value;
                m_titlePrev = value;

                if (m_titleChanged != null)
                    m_titleChanged(value);
            }
        }
    }
    protected System.Action<string> m_titleChanged;

    #endregion


    public virtual void Update()
    {
        // disbale  button

        // tween time
        // start delay
        // auto play
        // tween type
        // loop [loop type] | count
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));


        GUI.color = Enable ? Color.cyan : Color.grey;
        if (GUILayout.Button(Title, GUILayout.ExpandWidth(true)))
        {
            Enable = !Enable;
        }

        if (!Enable) return;
        GUI.color = Color.white;
//        if(ActingType != eeGames.Actor.ActingType.Color) GUI.backgroundColor = Color.cyan;

        ActingType = (ActingType)EditorGUILayout.EnumPopup("Acting Type", ActingType);
        EditorGUILayout.BeginHorizontal();
        TweenTime = EditorGUILayout.FloatField("Acting Time: ", TweenTime, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
//        GUILayout.Label("Start Delay : ", GUILayout.Width(130));

        TweenDelayTime = EditorGUILayout.FloatField("Start Delay : ", TweenDelayTime, GUILayout.ExpandWidth(true));
        //TweenDelayTime = float.Parse(GUILayout.TextField(TweenDelayTime.ToString(),GUILayout.ExpandWidth(true)));
      //  TweenDelayTime = GUILayout.HorizontalSlider(TweenDelayTime, 0, 60, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        AutoPlay = GUILayout.Toggle(AutoPlay, "Auto Play");
        CTweenType = (LeanTweenType)EditorGUILayout.EnumPopup("Tween Type", CTweenType);

        Loop = GUILayout.Toggle(Loop, "Loop");


        if (Loop) LoopType = (LoopType)EditorGUILayout.EnumPopup("Loop Type", LoopType);
        else
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
//            GUILayout.Label("Count", GUILayout.Width(130));
//            TweenCount = int.Parse(EditorGUILayout.TextField(TweenCount.ToString()));

            
            PingPong = GUILayout.Toggle(PingPong, "Once", GUILayout.Width(150));
            if(!PingPong) TweenCount = EditorGUILayout.IntField("Count: ", TweenCount, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }
}


public class ActorVec3Editor : CommonActorElements
{

    #region To
    private Vector4 m_toPrev;
    private Vector4 m_to;
    public Vector4 To
    {
        get { return m_to; }
        set
        {
            if (value != m_toPrev)
            {
                m_to = value;
                m_toPrev = value;

                if (m_toChanged != null)
                    m_toChanged(value);
            }
        }
    }
    private System.Action<Vector4> m_toChanged;

    #endregion


    #region From
    private Vector4 m_fromPrev;
    private Vector4 m_from;
    public Vector4 From
    {
        get { return m_from; }
        set
        {
            if (value != m_fromPrev)
            {
                m_from = value;
                m_fromPrev = value;

                if (m_fromChanged != null)
                    m_fromChanged(value);
            }
        }
    }
    private System.Action<Vector4> m_fromChanged;

    #endregion


   
    public void Init(string title, ActingType tType, float tweenTime, float delayTweenTime,
        bool isEnable, bool isAutoPlay, bool isLoop, bool isPingPong, LeanTweenType tweenType, LoopType loopType,
        int tweenCount, Vector4 toVec, Vector4 fromVec, System.Action<float> tweenTimeChanged,
        System.Action<float> delayTimeChanged, System.Action<bool> IsEnableChanged, System.Action<bool> autoPlayChanged,
        System.Action<bool> loopChanged, System.Action<LeanTweenType> tweenTypeChanged,
        System.Action<LoopType> loopTypeChanged, System.Action<int> countChanged, System.Action<Vector4> toChanged,
        System.Action<Vector4> fromChanged, System.Action<ActingType> actingTypeChanged, System.Action<bool> IsPingPongChanged)
    {
        Title = title;
        TweenTime = tweenTime;
        TweenDelayTime = delayTweenTime;
        Enable = isEnable;
        AutoPlay = isAutoPlay;
        Loop = isLoop;
        CTweenType = tweenType;
        LoopType = loopType;
        TweenCount = tweenCount;
        To = toVec;
        From = fromVec;
        PingPong = isPingPong;

        m_tweenTimeChanged = tweenTimeChanged;
        m_tweenDelayTimeChanged = delayTimeChanged;
        m_autoPlayChanged = autoPlayChanged;
        m_loopChanged = loopChanged;
        m_tweenTypeChanged = tweenTypeChanged;
        m_loopTypeChanged = loopTypeChanged;
        m_tweenCountChanged = countChanged;
        m_toChanged = toChanged;
        m_fromChanged = fromChanged;
        m_actingTypeChanged = actingTypeChanged;
        m_pingPongChanged = IsPingPongChanged;

        m_enableChanged = IsEnableChanged;
        ActingType = tType;

    }

    private void DrawColorActor() 
    {
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
       
  //      GUI.color = Color.cyan;
        if (GUILayout.Button("From", GUILayout.Width(75)))
        {
            // no need for switch
           
        }
        GUI.color = Color.white;
        From = EditorGUILayout.ColorField("", From, GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
    //    GUI.color = Color.cyan;
        if (GUILayout.Button("To", GUILayout.Width(75)))
        {
           
        }
        GUI.color = Color.white;
        To = EditorGUILayout.ColorField("", To, GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
       
    //    GUI.color = Color.cyan;
        EditorGUILayout.EndHorizontal();
    }
    private void DrawVec3Actor() 
    {
        GUI.color = Color.yellow;
        if (LoopType == eeGames.Actor.LoopType.StartOver && ActingType == eeGames.Actor.ActingType.Rotation)
            EditorGUILayout.HelpBox("Now this tween only works for Z-axis rotation, for clockwise rotation set 1 in last box(Z) of from button, for anti-clockwise rotation set -1 in last box(Z) of from button", MessageType.Info, true);
        GUI.color = Color.white;

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        
        if (GUILayout.Button("From", GUILayout.Width(75)))
        {
            switch (ActingType)
            {
                case ActingType.Position:
                    var newPos = Selection.activeTransform.position; //.GetComponent<RectTransform>()
                    
                    //  Debug.Log("Pos : " + newPos);

                    string[] dimension = UnityEditor.UnityStats.screenRes.Split('x');
                    newPos.x /= System.Int32.Parse(dimension[0]);
                    newPos.y /= System.Int32.Parse(dimension[1]);

                    From = newPos;
                    break;
                case ActingType.Scale:
                    From = Selection.activeTransform.localScale;
                    break;
                case ActingType.Rotation:
                    From = Selection.activeTransform.rotation.eulerAngles;
                    break;
            }
        }
        From = EditorGUILayout.Vector3Field("", From, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        if (GUILayout.Button("To", GUILayout.Width(75)))
        {
            switch (ActingType)
            {
                case ActingType.Position:
                    var newPos = Selection.activeTransform.position; //.GetComponent<RectTransform>()
                    //  Debug.Log("Pos : " + newPos);

                    string[] dimension = UnityEditor.UnityStats.screenRes.Split('x');
                    newPos.x /= System.Int32.Parse(dimension[0]);
                    newPos.y /= System.Int32.Parse(dimension[1]);

                    To = newPos;
                    break;
                case ActingType.Scale:
                    To = Selection.activeTransform.localScale;
                    break;
                case ActingType.Rotation:
                    To = Selection.activeTransform.rotation.eulerAngles;
                    break;
            }
        }
        To = EditorGUILayout.Vector3Field("", To, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        
        EditorGUILayout.EndHorizontal();
    }

    public override void Update()
    {

        base.Update();
        if (!Enable) return;
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
    

        if(ActingType == eeGames.Actor.ActingType.Color)
        {
            DrawColorActor();
        }
        else
        {
//            GUI.backgroundColor = Color.cyan;
            DrawVec3Actor();
        }

        EditorGUILayout.EndVertical();
    }
}


public class ActorColor : CommonActorElements
{
    #region To
    private Vector4 m_toPrev;
    private Vector4 m_to;
    public Vector4 To
    {
        get { return m_to; }
        set
        {
            if (value != m_toPrev)
            {
                m_to = value;
                m_toPrev = value;

                if (m_toChanged != null)
                    m_toChanged(value);
            }
        }
    }
    private System.Action<Vector4> m_toChanged;

    #endregion


    #region From
    private Vector4 m_fromPrev;
    private Vector4 m_from;
    public Vector4 From
    {
        get { return m_from; }
        set
        {
            if (value != m_fromPrev)
            {
                m_from = value;
                m_fromPrev = value;

                if (m_fromChanged != null)
                    m_fromChanged(value);
            }
        }
    }
    private System.Action<Vector4> m_fromChanged;

    #endregion




    #region Hide
    private Vector4 m_hidePrev;
    private Vector4 m_hide;
    public Vector4 Hide
    {
        get { return m_hide; }
        set
        {
            if (value != m_hidePrev)
            {
                m_hide = value;
                m_hidePrev = value;

                if (m_hideChanged != null)
                    m_hideChanged(value);
            }
        }
    }
    private System.Action<Vector4> m_hideChanged;

    #endregion

    private ActingType m_type;
    public void Init(string title, ActingType tType, float tweenTime, float delayTweenTime,
        bool isEnable, bool isAutoPlay, bool isLoop, LeanTweenType tweenType, LoopType loopType,
        int tweenCount, Vector4 toVec, Vector4 fromVec, Vector4 hideVec, System.Action<float> tweenTimeChanged,
        System.Action<float> delayTimeChanged, System.Action<bool> IsEnableChanged, System.Action<bool> autoPlayChanged,
        System.Action<bool> loopChanged, System.Action<LeanTweenType> tweenTypeChanged,
        System.Action<LoopType> loopTypeChanged, System.Action<int> countChanged, System.Action<Vector4> toChanged,
        System.Action<Vector4> fromChanged, System.Action<Vector4> hideChanged)
    {
        Title = title;
        TweenTime = tweenTime;
        TweenDelayTime = delayTweenTime;
        Enable = isEnable;
        AutoPlay = isAutoPlay;
        Loop = isLoop;
        CTweenType = tweenType;
        LoopType = loopType;
        TweenCount = tweenCount;
        To = toVec;
        From = fromVec;
        Hide = hideVec;
        m_tweenTimeChanged = tweenTimeChanged;
        m_tweenDelayTimeChanged = delayTimeChanged;
        m_autoPlayChanged = autoPlayChanged;
        m_loopChanged = loopChanged;
        m_tweenTypeChanged = tweenTypeChanged;
        m_loopTypeChanged = loopTypeChanged;
        m_tweenCountChanged = countChanged;
        m_toChanged = toChanged;
        m_fromChanged = fromChanged;
        m_hideChanged = hideChanged;
        m_enableChanged = IsEnableChanged;
        m_type = tType;
    }


    public override void Update()
    {

        base.Update();
        if (!Enable) return;
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
        GUI.backgroundColor = Color.white;


        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUI.color = Color.cyan;
        if (GUILayout.Button("From", GUILayout.Width(75)))
        {
            switch (m_type)
            {
                case ActingType.Position:
                    break;
                case ActingType.Scale:
                    // From = Selection.activeTransform.localScale;
                    break;
                case ActingType.Rotation:
                    break;
            }
        }
        GUI.color = Color.white;
        From = EditorGUILayout.ColorField("", From, GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUI.color = Color.cyan;
        if (GUILayout.Button("To", GUILayout.Width(75)))
        {
            switch (m_type)
            {
                case ActingType.Position:
                    break;
                case ActingType.Scale:
                    //   To = Selection.activeTransform.localScale;
                    break;
                case ActingType.Rotation:
                    break;
            }
        }
        GUI.color = Color.white;
        To = EditorGUILayout.ColorField("", To, GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        //GUI.color = Color.cyan;
        //if (GUILayout.Button("Hide", GUILayout.Width(75)))
        //{
        //    switch (m_type)
        //    {
        //        case ActingType.Position:
        //            break;
        //        case ActingType.Scale:
        //            //     Hide = Selection.activeTransform.localScale;
        //            break;
        //        case ActingType.Rotation:
        //            break;
        //    }
        //}
        //GUI.color = Color.white;
        //Hide = EditorGUILayout.ColorField("", Hide, GUILayout.ExpandWidth(true));
        GUI.color = Color.cyan;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
}