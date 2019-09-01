using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using eeGames.Widget;

/// <summary>
/// Stores Windget Data Edited by Widget Editor Window
/// </summary>
public class WidgetDatabase : ScriptableObject
{
    [SerializeField]
    private List<WidgetData> database;

    void OnEnable()
    {
        if (database == null)
            database = new List<WidgetData>();
    }

    public void Add(WidgetData npc)
    {
        database.Add(npc);
    }

    public void Remove(WidgetData npc)
    {
        database.Remove(npc);
    }

    public void RemoveAt(int index)
    {
        database.RemoveAt(index);
    }

    public int COUNT
    {
        get { return database.Count; }
    }


    public WidgetData GameUI(int index)
    {
        return database.ElementAt(index);
    }

    public void SortAlphabeticallyAtoZ()
    {
        database.Sort((x, y) => string.Compare(x.Id.ToString(), y.Id.ToString()));
    }

    public List<WidgetData> GetDatabase()
    {
        return database;
    }
}


[System.Serializable]
public class WidgetData
{
    public WidgetName Id;
    public string perfabPath;
    public bool PoolOnLoad;
}
