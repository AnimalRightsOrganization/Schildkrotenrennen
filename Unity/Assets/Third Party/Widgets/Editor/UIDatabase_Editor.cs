

using UnityEngine;
using System.Collections;
using UnityEditor;
using eeGames.Widget;

public class UIDatabase_Editor : EditorWindow
{
    private enum State
    {
        BLANK,
        EDIT,
        ADD
    }

    private State m_state;

//    private int m_selectedUI;
    private string m_newUIName;
    private WidgetData m_newUI;
    private const string DATABASE_PATH = @"Assets/Widgets/Source Code/database/Resources/WidgetDatabase.asset";

    private WidgetDatabase m_gameWidgets;
    private Vector2 m_scrollPos;



    [MenuItem("Tools/Widget/Tutorial")]
    public static void Tutorial()
    {
        string url = "https://eeprogrammer.wordpress.com/2016/03/06/unity3d-widgetui-manager/";
        Application.OpenURL(url);
        Debug.Log(url);
    }

    [MenuItem("Tools/Widget/Widget Editor %#w")]
    public static void Init()
    {
        UIDatabase_Editor window = EditorWindow.GetWindow<UIDatabase_Editor>();
        window.minSize = new Vector2(800, 400);

      //  window.titleContent = new GUIContent("Widget Editor", Resources.Load<Texture2D>("icon")); // not avaliable in unity 5.0
        window.title = ("Widget Editor");
        window.Show();
    }

    void OnEnable()
    {
        if (m_gameWidgets == null)
            LoadDatabase();

        m_state = State.BLANK;
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        DisplayListArea();
        DisplayMainArea();
        EditorGUILayout.EndHorizontal();
    }

    void LoadDatabase()
    {
        m_gameWidgets = (WidgetDatabase)AssetDatabase.LoadAssetAtPath(DATABASE_PATH, typeof(WidgetDatabase));

        if (m_gameWidgets == null)
            CreateDatabase();
    }

    void CreateDatabase()
    {
        m_gameWidgets = ScriptableObject.CreateInstance<WidgetDatabase>();
        AssetDatabase.CreateAsset(m_gameWidgets, DATABASE_PATH);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    void DisplayListArea()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(250));
        EditorGUILayout.Space();

        m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, "box", GUILayout.ExpandHeight(true));

        for (int cnt = 0; cnt < m_gameWidgets.COUNT; cnt++)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                m_gameWidgets.RemoveAt(cnt);
                m_gameWidgets.SortAlphabeticallyAtoZ();
                EditorUtility.SetDirty(m_gameWidgets);
                m_state = State.BLANK;
                return;
            }
            GUI.color = Color.white;

            GUI.color = Color.green;
            if (GUILayout.Button(m_gameWidgets.GameUI(cnt).Id.ToString(), "box", GUILayout.ExpandWidth(true)))
            {
//                m_selectedUI = cnt;
                m_state = State.EDIT;
                m_newUI = m_gameWidgets.GameUI(cnt);
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Widgets: " + m_gameWidgets.COUNT, GUILayout.Width(100));

        if (GUILayout.Button("New Widget"))
        {
            m_newUI = new WidgetData();
            m_state = State.ADD;
        }


        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
    }

    void DisplayMainArea()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        switch (m_state)
        {
            case State.ADD:
                DisplayAddMainArea();
                break;
            case State.EDIT:
                DisplayEditMainArea();
                break;
            default:
                DisplayBlankMainArea();
                break;
        }

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
    }
    void DisplayBlankMainArea()
    {
        EditorGUILayout.LabelField(
            "There are 3 things that can be displayed here.\n" +
            "1) Widget Id\n" +
            "2) Pool on Load\n" +
            "3) Widget Prefab Path\n"
            ,
            GUILayout.ExpandHeight(true));
    }

    // private UIData uiData = null;
    void DisplayEditMainArea()
    {
        DisplayUIInfo();

        EditorGUILayout.Space();

        if (GUILayout.Button("Done Edit", GUILayout.Width(100)))
        {
            //if (IsExist())
            //{
            //    ShowNotification(new GUIContent(m_newUI.Id.ToString() + " Already Exists"));
            //}
            //else
            //{
                m_gameWidgets.SortAlphabeticallyAtoZ();
                EditorUtility.SetDirty(m_gameWidgets);
                m_state = State.BLANK;
            //}
        }


    }
    bool IsExist()
    {
        bool exist = false;
        m_gameWidgets.GetDatabase().ForEach(ui =>
        {
            if (ui.Id == m_newUI.Id)
            {
                exist = true;
            }

        });
        return exist;
    }
    private void DisplayUIInfo()
    {
        m_newUI.Id = (WidgetName)EditorGUI.EnumPopup(GUILayoutUtility.GetRect(0.0f, 10.0f, GUILayout.ExpandWidth(true)), "Widget ID:", m_newUI.Id);
        EditorGUILayout.Space();
        m_newUI.PoolOnLoad = EditorGUILayout.Toggle("Pool on Load", m_newUI.PoolOnLoad);
        m_newUI.perfabPath = EditorGUILayout.TextField(new GUIContent("Widget(prefab) Path : "), m_newUI.perfabPath);
        DropPrefabArea(m_newUI);
    }
    void DisplayAddMainArea()
    {
        DisplayUIInfo();

        if (GUILayout.Button("Done", GUILayout.Width(100)))
        {

            if (IsExist())
            {
                ShowNotification(new GUIContent(m_newUI.Id.ToString() + " Already Exists"));
            }
            else
            {
                m_gameWidgets.Add(m_newUI);
                m_gameWidgets.SortAlphabeticallyAtoZ();
                EditorUtility.SetDirty(m_gameWidgets);
                m_state = State.BLANK;
            }
        }
    }

    public void DropPrefabArea(WidgetData uiData)
    {
        Event evt = Event.current;
        GUI.color = Color.green;
        Rect drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(drop_area, "Drag Widget Here");
        GUI.color = Color.white;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!drop_area.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (string dragged_object in DragAndDrop.paths)
                    {
                        int offset = dragged_object.IndexOf("Resources/");
                        string processedString = (dragged_object.Split('.')[0]).Remove(0, offset + "Resources/".Length);
                        uiData.perfabPath = processedString;
                    }
                }
                break;
        }
    }

    [MenuItem("Tools/Widget/Create Widget Manager")]
    public static void CreateWidgetManager()
    {
        var obj = GameObject.FindObjectOfType<WidgetManager>();
        if(obj == null)
        {
            GameObject wManager = new GameObject("Widget_Manager", typeof(WidgetManager));
            Debug.Log("Widget Manager Created...! " + wManager.name);
        }
        else
        {
            Debug.Log("Widget Manager Already Exists in Scene");
        }
        
    }

    public static class uGUITools
    {
        [MenuItem("Tools/Widget/Anchors to Corners %#A")]
        static void AnchorsToCorners()
        {
            foreach (Transform transform in Selection.transforms)
            {
                RectTransform t = transform as RectTransform;
                RectTransform pt = Selection.activeTransform.parent as RectTransform;

                if (t == null || pt == null) return;

                Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
                                                    t.anchorMin.y + t.offsetMin.y / pt.rect.height);
                Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
                                                    t.anchorMax.y + t.offsetMax.y / pt.rect.height);

                t.anchorMin = newAnchorsMin;
                t.anchorMax = newAnchorsMax;
                t.offsetMin = t.offsetMax = new Vector2(0, 0);
            }
        }
    }
}