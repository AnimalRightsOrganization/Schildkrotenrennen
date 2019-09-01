using UnityEngine;
using System.Collections;
using UnityEditor;
using eeGames.Actor;
using UnityEditor.AnimatedValues;
using System.Collections.Generic;

[CustomEditor(typeof(Actor), true), CanEditMultipleObjects]
public class Actor_Editor : Editor
{
    private Actor Target { get { return (Actor)target; } }
//    private AnimBool networkingFoldout;
    private bool m_tweens;
    private GUIStyle m_editorStyle;
    private ActorVec3Editor m_actor;

    private List<CommonActorElements> m_actorElements = new List<CommonActorElements>();
    private void OnEnable()
    {
        m_actorElements.Clear();
//        networkingFoldout = new AnimBool(true);

        m_editorStyle = new GUIStyle();
        m_editorStyle.normal.textColor = Color.white;
        m_editorStyle.fontStyle = FontStyle.Bold;
        m_editorStyle.alignment = TextAnchor.UpperCenter;


        var dummy = Target;
        if (dummy.ActorData == null)
        {
           // dummy.ActorData = new ActorData();
            Debug.Log("Hey Target is null");
        }


        m_actor = new ActorVec3Editor();
        var sActorData = Target.ActorData;
        m_actor.Init(Target.Type.ToString() + " Actor", Target.Type, sActorData.Time, sActorData.DelayTime, sActorData.IsActive,
            sActorData.IsAutoPlay, sActorData.IsLoop, sActorData.IsOnce, sActorData.TweenType, sActorData.LoopType, sActorData.TweenCount,
            sActorData.To, sActorData.From,
            (val) =>
            {
                sActorData.Time = val;
            },
            (val) =>
            {
                sActorData.DelayTime = val;
            },
            (val) =>
            {
                sActorData.IsActive = val;
            },
            (val) =>
            {
                sActorData.IsAutoPlay = val;
            },
            (val) =>
            {
                sActorData.IsLoop = val;
            },
            (val) =>
            {
                sActorData.TweenType = val;
            },
            (val) =>
            {
                sActorData.LoopType = val;
            },
            (val) =>
            {
                sActorData.TweenCount = val;
            },
            (val) =>
            {
                sActorData.To = val;
            },
            (val) =>
            {
                sActorData.From = val;
            },
            (val) => 
            {
                Target.Type = val;
            },
            (val) =>
            {
                Target.ActorData.IsOnce = val;
            });


        m_actorElements.Add(m_actor);

        //      ActorColor colorActor = new ActorColor();
        // m_actorElements.Add(colorActor);
    }

//    private static bool _positionFold = false;
    public override void OnInspectorGUI()
    {

//        DrawDefaultInspector();
        foreach (var item in m_actorElements)
        {
            item.Update();
            if (item.Enable)
            {
                SerializedProperty onStart = serializedObject.FindProperty("OnStart");
                EditorGUIUtility.LookLikeControls();
                EditorGUILayout.PropertyField(onStart);

                SerializedProperty onStop = serializedObject.FindProperty("OnStop");
                EditorGUIUtility.LookLikeControls();
                EditorGUILayout.PropertyField(onStop);
            }
        }


        

        //       // Needed because the enum's keep getting reset
        EditorUtility.SetDirty(Target);

        Repaint();
    }
}
