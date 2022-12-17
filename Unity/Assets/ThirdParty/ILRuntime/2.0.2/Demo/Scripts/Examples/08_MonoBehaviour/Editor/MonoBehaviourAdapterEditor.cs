using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Runtime.Enviorment;

#if UNITY_EDITOR
[CustomEditor(typeof(MonoBehaviourAdapter.Adaptor), true)]
public class MonoBehaviourAdapterEditor : UnityEditor.UI.GraphicEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        MonoBehaviourAdapter.Adaptor clr = target as MonoBehaviourAdapter.Adaptor;
        var instance = clr.ILInstance;
        if (instance != null)
        {
            EditorGUILayout.LabelField("Script", clr.ILInstance.Type.FullName);

            int index = 0;
            foreach (var i in instance.Type.FieldMapping)
            {
                //这里是取的所有字段，没有处理不是public的
                var name = i.Key;
                var type = instance.Type.FieldTypes[index];//在这里不能用i.Value，因为Unity有HideInInspector方法，隐藏序列化的值，但是还是会被计数
                index++;

                var cType = type.TypeForCLR;
                if (cType.IsPublic == false)
                {
                    instance[i.Value] = "Private Value";
                }
                if (cType.IsPrimitive)//如果是基础类型（boolean、char、byte、short、int、long、float、double）
                {
                    if (cType == typeof(float))
                    {
                        instance[i.Value] = EditorGUILayout.FloatField(name, (float)instance[i.Value]);
                    }
                    else if (cType == typeof(int))
                    {
                        instance[i.Value] = EditorGUILayout.IntField(name, (int)instance[i.Value]);
                    }
                    else if (cType == typeof(bool))
                    {
                        instance[i.Value] = EditorGUILayout.Toggle(name, i.Value == 1);
                    }
                    else
                    {
                        //throw new System.NotImplementedException();//剩下的大家自己补吧
                        instance[i.Value] = cType.ToString();
                    }
                }
                else
                {
                    object obj = instance[i.Value];
                    if (cType == typeof(Vector2))
                    {
                        instance[i.Value] = EditorGUILayout.Vector2Field(name, (Vector2)instance[i.Value]);
                    }
                    else if (cType == typeof(Vector3))
                    {
                        instance[i.Value] = EditorGUILayout.Vector3Field(name, (Vector3)instance[i.Value]);
                    }
                    else if (cType == typeof(Vector4))
                    {
                        instance[i.Value] = EditorGUILayout.Vector4Field(name, (Vector4)instance[i.Value]);
                    }
                    else if (cType == typeof(Color))
                    {
                        instance[i.Value] = EditorGUILayout.ColorField(name, (Color)instance[i.Value]);
                    }
                    else if (cType == typeof(Bounds))
                    {
                        instance[i.Value] = EditorGUILayout.BoundsField(name, (Bounds)instance[i.Value]);
                    }
                    else if (cType == typeof(AnimationCurve))
                    {
                        instance[i.Value] = EditorGUILayout.CurveField(name, (AnimationCurve)instance[i.Value]);
                    }
                    else if (typeof(UnityEngine.Object).IsAssignableFrom(cType))
                    {
                        //处理Unity类型
                        var res = EditorGUILayout.ObjectField(name, obj as UnityEngine.Object, cType, true);
                        instance[i.Value] = res;
                    }
                    else
                    {
                        //其他类型现在没法处理
                        if (obj != null)
                            EditorGUILayout.LabelField(name, obj.ToString());
                        else
                            EditorGUILayout.LabelField(name, "(null)");
                    }
                }
            }
        }
    }

    protected override void OnEnable()
    {

    }

    protected override void OnDisable()
    {
        //SceneView.duringSceneGui = null; 不要再SceneView中绘制，否则报错
    }
}
#endif