using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Collections;
using eeGames.Actor;

[CustomEditor(typeof(WidgetTween), true), CanEditMultipleObjects]
public class WidgetTween_Editor : Editor 
{
    private WidgetTween Target { get { return (WidgetTween)target; } }
    private AnimBool networkingFoldout;
    private AnimBool easyControls;
    private List<EditorDisplayButton> _editorButtons = new List<EditorDisplayButton>();
    private GUIStyle _editorStyle;


   

    private void Init() 
    {

        networkingFoldout = new AnimBool(true);
        easyControls = new AnimBool(true);
        easyControls.target = true;

        _editorStyle = new GUIStyle();
        _editorStyle.normal.textColor = Color.white;
        _editorStyle.fontStyle = FontStyle.Bold;
        _editorStyle.alignment = TextAnchor.UpperCenter;

        Texture2D btnUp = Resources.Load<Texture2D>("HoverIdle");

        var dummy = Target;
        if (dummy.TweenData == null)
        {
            dummy.TweenData = new Tweens();
          //  Debug.Log("Hey Target is null");
        }
        _editorButtons = new List<EditorDisplayButton>();

        CustomButtonStyle buttonStyle = new CustomButtonStyle("Position Tween", btnUp, btnUp, null, 175);

        PositionTweenButton positionButton = new PositionTweenButton(buttonStyle);
        positionButton.Initialize(ActingType.Position, Target, Target.TweenData.IsPosition, Target.TweenData.sPosition.Time, "",
            (val) =>
            {
                Target.TweenData.IsPosition = val;
            },
            (val) =>
            {
                Target.TweenData.sPosition.Time = val;

            });


        _editorButtons.Add(positionButton);



        CustomButtonStyle sbuttonStyle = new CustomButtonStyle("Scale Tween", btnUp, btnUp, null, 175);

        PositionTweenButton scaleButton = new PositionTweenButton(sbuttonStyle);
        scaleButton.Initialize(ActingType.Scale, Target, Target.TweenData.IsScale, Target.TweenData.sScale.Time, string.Empty,
            (val) =>
            {
                Target.TweenData.IsScale = val;
            },
            (val) =>
            {
                Target.TweenData.sScale.Time = val;

            });


        _editorButtons.Add(scaleButton);



        CustomButtonStyle rbuttonStyle = new CustomButtonStyle("Rotation Tween", btnUp, btnUp, null, 175);

        PositionTweenButton rotationButton = new PositionTweenButton(rbuttonStyle);
        rotationButton.Initialize(ActingType.Rotation, Target, Target.TweenData.IsRotation, Target.TweenData.sRotation.Time, string.Empty,
            (val) =>
            {
                Target.TweenData.IsRotation = val;
            },
            (val) =>
            {
                Target.TweenData.sRotation.Time = val;

            });


        _editorButtons.Add(rotationButton);

    }
    private void OnEnable()
    {
        Init();
    }
    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Separator();
        EditorGUILayout.Space();
        GUI.color = Color.cyan;
       
        EditorGUILayout.HelpBox("Tween Type Works Best with [Ease Out Bounce] For Rotation Tween [Ease Out Elastic] gives good Result and some Tween Types gives false behaviour , play around and find tween type which suites you", MessageType.Info, true);
       
        GUI.color = Color.white;

        EditorGUILayout.Space();
        EditorGUILayout.Separator();
        EditorGUILayout.Space();

        Target.TweenData.TweenType = (LeanTweenType)EditorGUILayout.EnumPopup("Tween Type", Target.TweenData.TweenType, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
        EditorGUILayout.Separator();
        EditorGUILayout.Space();

        if (EditorGUILayout.BeginFadeGroup(networkingFoldout.faded))
        {
            foreach (EditorDisplayButton button in _editorButtons)
            {
                button.Update(Event.current, _editorStyle, this, Screen.width, Screen.height);
                GUILayout.Space(10);
            }

        }


        EditorGUILayout.Space();
        EditorGUILayout.Separator();
        EditorGUILayout.Space();


        EditorUtility.SetDirty(Target);
        
        Repaint();
    }
}
