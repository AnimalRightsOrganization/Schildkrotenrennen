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
        // 100万级，7~9ms
        // 一亿级，400~550ms
        DataSource = new List<int>();
        for (int i = 0; i < 100000000; i++)
        {
            DataSource.Add(i);
        }
        watch.Stop();
        var useTime = watch.ElapsedMilliseconds;
        Debug.Log($"插入耗时：{useTime}ms");*/
    }

    void Start()
    {
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        listRange = GetPage();
        //listRange = GetPage1();
        watch.Stop();
        var useTime = watch.ElapsedMilliseconds;
        Debug.Log($"查询耗时：{useTime}ms");
    }

    // 100万级，0ms
    // 一亿级，0~1ms
    private List<int> GetPage()
    {
        //Skip()跳过多少条
        //Take()查询多少条
        return this.DataSource.Skip((PageIndex - 1) * EachCount).Take(EachCount).ToList();
    }

    // 100万级，0ms
    // 一亿级，ms
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