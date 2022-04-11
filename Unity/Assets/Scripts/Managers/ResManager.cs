﻿using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class JsonAssets
{
    public string filePath;
    public string md5;
    public string[] depend;
}

public class ResManager
{
    static JsonAssets[] _assetsObj;
    static JsonAssets[] assetsObj
    {
        get
        {
            if (_assetsObj == null)
                Reload();
            return _assetsObj;
        }
    }
    static void Reload()
    {
        string assetsPath = $@"{Application.persistentDataPath}\StandaloneWindows64\assets.bytes"; //解析文件
        string assetsJson = File.ReadAllText(assetsPath);
        _assetsObj = JsonMapper.ToObject<JsonAssets[]>(assetsJson);
    }

    public const string BUNDLES_FOLDER = "Assets/Bundles";
    public const string PREFAB_FOLDER = "Assets/Bundles/Prefabs";

    public static GameObject LoadPrefab(string fileName)
    {
#if UNITY_EDITOR && !USE_ASSETBUNDLE
        string filePath = $"Assets/Bundles/{fileName}.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
#else
        string filePath = GetFilePath($"{fileName}.unity3d");

        // 先读取依赖
        List<AssetBundle> dependBundles = new List<AssetBundle>();
        string[] depends = GetDepends($"{fileName}.unity3d");
        for (int i = 0; i < depends.Length; i++)
        {
            string dependPath = GetFilePath(depends[i]);
            AssetBundle dependAsset = AssetBundle.LoadFromFile(dependPath.ToLower());
            dependBundles.Add(dependAsset);
        }

        var asset = AssetBundle.LoadFromFile(filePath.ToLower());
        GameObject prefab = asset.LoadAllAssets()[0] as GameObject;
        asset.Unload(false);

        // 再卸载依赖
        for (int i = dependBundles.Count - 1; i >= 0; i--)
        {
            dependBundles[i].Unload(false);
            dependBundles.RemoveAt(i);
        }
#endif
        return prefab;
    }
    
    public static AudioClip LoadAudioClip(string fileName)
    {
#if UNITY_EDITOR && !USE_ASSETBUNDLE
        var obj = AssetDatabase.LoadAssetAtPath<AudioClip>($"{BUNDLES_FOLDER}/{fileName}.mp3");
#else
        string filePath0 = $"{BUNDLES_FOLDER}/{fileName}.unity3d";
        Debug.Log($"filePath0={filePath0}"); //~~Assets/Bundles/Audios/round1.unity3d

        string filePath = GetFilePath($"{fileName}.unity3d");
        Debug.Log($"filePath={filePath}");

        AssetBundle asset = AssetBundle.LoadFromFile(filePath);
        object config = asset.LoadAllAssets()[0];
        asset.Unload(false);
        var obj = (AudioClip)config;
#endif
        return obj;
    }

    public static object LoadConfig(string configName)
    {
#if UNITY_EDITOR && !USE_ASSETBUNDLE
        string filePath = $"Assets/Bundles/{configName}.asset";
        object config = AssetDatabase.LoadAssetAtPath<Object>(filePath);
#else
        string filePath = GetFilePath($"{configName}.unity3d");
        AssetBundle asset = AssetBundle.LoadFromFile(filePath);
        object config = asset.LoadAllAssets()[0];
        asset.Unload(false);
#endif
        return config;
    }

    static string GetFilePath(string assetName)
    {
        JsonAssets obj = assetsObj.Where(x => x.filePath == assetName.ToLower()).FirstOrDefault();
        //Debug.Log($"assetName={assetName}");
        string result = $"{Application.persistentDataPath}/StandaloneWindows64/{obj.md5}.unity3d";
        //Debug.Log($"<color=green>result={result}</color>");
        return result;
    }

    static string[] GetDepends(string assetName)
    {
        JsonAssets obj = assetsObj.Where(x => x.filePath == assetName.ToLower()).FirstOrDefault();
        return obj.depend;
    }

    private static Dictionary<string, GameObject> GameObjectPool = new Dictionary<string, GameObject>();
    public static GameObject GetGameObject(string key)
    {
        GameObject obj = null;
        if (GameObjectPool.TryGetValue(key, out obj))
        {
            //Debug.Log($"从对象池获取：{key}");
            return obj;
        }
        else
        {
            //Debug.Log($"新建：{key}");
            obj = LoadPrefab(key);
            GameObjectPool.Add(key, obj);
            return obj;
        }
    }
}