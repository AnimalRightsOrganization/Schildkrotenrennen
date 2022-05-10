using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    List<int> DataSource = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
    public int PageIndex = 1;
    public int EachCount = 10;
    public List<int> listRange;

    void Awake()
    {
        /*
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        // 100�򼶣�7~9ms
        // һ�ڼ���400~550ms
        DataSource = new List<int>();
        for (int i = 0; i < 100000000; i++)
        {
            DataSource.Add(i);
        }
        watch.Stop();
        var useTime = watch.ElapsedMilliseconds;
        Debug.Log($"�����ʱ��{useTime}ms");*/
    }

    void Start()
    {
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        listRange = GetPage();
        //listRange = GetPage1();
        watch.Stop();
        var useTime = watch.ElapsedMilliseconds;
        Debug.Log($"��ѯ��ʱ��{useTime}ms");
    }

    // 100�򼶣�0ms
    // һ�ڼ���0~1ms
    private List<int> GetPage()
    {
        //Skip()����������
        //Take()��ѯ������
        return this.DataSource.Skip((PageIndex - 1) * EachCount).Take(EachCount).ToList();
    }

    // 100�򼶣�0ms
    // һ�ڼ���ms
    private List<int> GetPage1()
    {
        List<int> tmp = new List<int>();
        int startId = (PageIndex - 1) * EachCount;
        /* 60~70ms
        for (int i = 0; i < DataSource.Count; i++)
        {
            if (i < startId)
                continue;

            if (i >= startId + EachCount)
                continue;

            var item = DataSource[i];
            tmp.Add(item);
        }*/
        for (int i = startId; i < DataSource.Count; i++)
        {
            if (i >= startId + EachCount)
                break;

            var item = DataSource[i];
            tmp.Add(item);
        }
        return tmp;
    }
}