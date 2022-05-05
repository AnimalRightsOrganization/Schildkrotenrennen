using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class JsonAssets //就是ABInfo
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
        string assetsPath = $"{Application.persistentDataPath}/{ConstValue.PLATFORM_NAME}/assets.bytes"; //解析文件
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
        Debug.Log($"LoadPrefab: filePath={filePath}");
        Debug.Log($"ab exist: {File.Exists(filePath)}");

        // 先读取依赖
        List<AssetBundle> dependBundles = new List<AssetBundle>();
        string[] depends = GetDepends($"{fileName}.unity3d");
        for (int i = 0; i < depends.Length; i++)
        {
            string dependPath = GetFilePath(depends[i]);
            Debug.Log($"读取依赖：{i}---{dependPath}");
            AssetBundle dependAsset = AssetBundle.LoadFromFile(dependPath.ToLower());
            dependBundles.Add(dependAsset);
        }

        //Debug.Log($"111.filePath: {filePath}");
        //Debug.Log($"222.filePath.ToLower(): {filePath.ToLower()}");
        var asset = AssetBundle.LoadFromFile(filePath); //这里不能小写，会导致iOS读不出。s
        //Debug.Log($"333.资源数={asset.GetAllAssetNames().Length}---[0]{asset.GetAllAssetNames()[0]}");
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
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{BUNDLES_FOLDER}/{fileName}.mp3");
#else
        string filePath0 = $"{BUNDLES_FOLDER}/{fileName}.unity3d";
        Debug.Log($"filePath0={filePath0}"); //~~Assets/Bundles/Audios/round1.unity3d

        string filePath = GetFilePath($"{fileName}.unity3d");
        Debug.Log($"filePath={filePath}");

        AssetBundle asset = AssetBundle.LoadFromFile(filePath);
        object config = asset.LoadAllAssets()[0];
        asset.Unload(false);
        var clip = (AudioClip)config;
#endif
        return clip;
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

    public static byte[] LoadDLL()
    {
#if UNITY_EDITOR && !USE_ASSETBUNDLE
        string filePath = $"Assets/Bundles/Configs/Hotfix.dll.bytes";
        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
#else
        string fileName = "configs/hotfix";
        string filePath = GetFilePath($"{fileName}.unity3d"); //文件名是加密子串
        AssetBundle asset = AssetBundle.LoadFromFile(filePath);
        TextAsset textAsset = asset.LoadAllAssets()[0] as TextAsset;
        asset.Unload(false);
#endif
        return textAsset.bytes;
    }

    public static Dictionary<string, Sprite> LoadSprite(string configName)
    {
        Dictionary<string, Sprite> sp;
#if UNITY_EDITOR && !USE_ASSETBUNDLE
        string filePath = $"Assets/Bundles/{configName}.png";
        var assets = AssetDatabase.LoadAllAssetsAtPath(filePath);
#else
        string filePath = GetFilePath($"{configName}.unity3d");
        AssetBundle asset = AssetBundle.LoadFromFile(filePath);
        var assets = asset.LoadAllAssets();
        asset.Unload(false);
#endif
        int count = assets.Count();
        //Debug.Log($"子物体：{count}个");

        sp = new Dictionary<string, Sprite>();
        for (int i = 0; i < count; i++)
        {
            var subAsset = assets[i] as Sprite;
            if (subAsset != null)
            {
                //Debug.Log($"{i}---{subAsset?.name}");
                sp.Add(subAsset.name, subAsset);
            }
        }
        //Debug.Log($"字典：{sp.Count}个");
        return sp;
    }

    static string GetFilePath(string assetName)
    {
        Debug.Log($"GetFilePath: {assetName.ToLower()}");

        JsonAssets obj = assetsObj.Where(x => x.filePath == assetName.ToLower()).FirstOrDefault();
        //JsonAssets obj = assetsObj.Where(x => x.filePath == assetName).FirstOrDefault(); //iOS区分大小写？
        Debug.Log($"obj: {obj.md5}");

        string result = $"{Application.persistentDataPath}/{ConstValue.PLATFORM_NAME}/{obj.md5}.unity3d";
        Debug.Log($"result={result}");

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