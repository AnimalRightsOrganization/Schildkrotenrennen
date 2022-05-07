using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameObject Map;
    public GameObject Rock;
    public GameObject Turtle;

    void Start()
    {
        var map_asset = ResManager.LoadPrefab("Prefabs/Map");
        Map = Instantiate(map_asset);
        Map.name = "Map";
    }

    void Update()
    {
        
    }
}