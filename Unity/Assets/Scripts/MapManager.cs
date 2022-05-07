using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DG.Tweening;

[CustomEditor(typeof(MapManager))]
public class DemoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); //��ʾĬ�����в���

        MapManager demo = (MapManager)target;

        if (GUILayout.Button("+1"))
        {
            demo.Move1(demo.selected, 1);
        }
        if (GUILayout.Button("+2"))
        {
            demo.Move2(demo.selected);
        }
        if (GUILayout.Button("-1"))
        {
            demo.Move1(demo.selected, -1);
        }
    }
}
public enum TurtleColor
{
    NONE = -1, //δָ����
    RED = 0,
    YELLOW = 1,
    GREEN = 2,
    BLUE = 3,
    PURPLE = 4,
    COUNT = 5,
}
public class MapManager : MonoBehaviour
{
    public const float TURTLE_HEIGHT = 0.25f;

    public bool IsLock = false;

    public GameObject Map;
    public Transform[] Rock;
    public Transform[] Turtle;

    // ��¼ÿ�������е��ڹ꣬��˳�򣨴��µ��ϣ�
    //key:����ID  value:�ڹ꣬��˳��
    public List<int>[] GridContent;

    // ��ѯĳ�ڹ굱ǰ���ڸ���
    //key:�ڹ���ɫ  value:����ID
    public int[] TurtleInGrid;

    void Start()
    {
        //var map_asset = ResManager.LoadPrefab("Prefabs/Map");
        //Map = Instantiate(map_asset);
        //Map.name = "Map";

        Init();
    }

    void Init()
    {
        // �����5ֻ��
        GridContent = new List<int>[10];
        for (int i = 0; i < 10; i++)
        {
            GridContent[i] = new List<int>();
        }
        GridContent[0] = new List<int> { 0, 1, 2, 3, 4 };

        // 5ֻ�������
        TurtleInGrid = new int[5];
        TurtleInGrid[0] = 0;
        TurtleInGrid[1] = 0;
        TurtleInGrid[2] = 0;
        TurtleInGrid[3] = 0;
        TurtleInGrid[4] = 0;
    }

    public Tweener Move1(TurtleColor turtle, int step)
    {
        Tweener tw = null;
        if (IsLock)
        {
            Debug.LogError("�ƶ���...�Ժ�����");
            return tw;
        }

        // ���ݲ����
        int turtle_id = (int)turtle;
        int src_id = TurtleInGrid[turtle_id];
        if (src_id >= 9)
        {
            Debug.LogError("�Ѿ������յ�");
            return tw;
        }
        int dest_id = src_id + step;
        if (dest_id < 0)
        {
            Debug.LogError("�Ѿ������");
            return tw;
        }
        Vector3 dest_pos = Rock[dest_id].position;

        // ���ݲ��޸�
        TurtleInGrid[turtle_id] = dest_id;
        GridContent[src_id].Remove(turtle_id);
        GridContent[dest_id].Add(turtle_id);


        // ���Ŀ����ӵ�ǰ�Ƿ����ڹ꣬���Ա��ֲ���Ӱ��
        int dest_count = GridContent[dest_id].Count;
        dest_pos.y = TURTLE_HEIGHT * dest_count;

        // ���ֲ��޸�
        tw = Turtle[turtle_id].DOMove(dest_pos, 0.5f);
        tw.OnPlay(() =>
        {
            IsLock = true;
            Debug.Log("tw.OnPlay.111");
        });
        tw.OnComplete(() =>
        {
            IsLock = false;
            Debug.Log("tw.OnComplete.111");
        });
        return tw;
    }

    public void Move2(TurtleColor turtle)
    {
        //TODO:
        //~~��Inspector����ʾ�ֵ䡣~~
        //~~�������������~~
        //~~�ۺ��ˡ�~~
        //~~���ƶ�����~~
        //�ݵ������ߡ���ȡ���䡣

        var tw = Move1(turtle, 1);
        //�����ٴ�ע��ί�У��൱�ڰ�Move1�����ί�и�����
        tw.OnPlay(() =>
        {
            IsLock = true;
        });
        tw.OnComplete(() =>
        {
            IsLock = false;

            Move1(turtle, 1);
        });
    }

    public TurtleColor selected;
}