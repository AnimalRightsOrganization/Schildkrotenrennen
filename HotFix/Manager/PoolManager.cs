using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace HotFix
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Get;

        public Dictionary<string, List<GameObject>> dic_active;
        public Dictionary<string, List<GameObject>> dic_inactive;
        public Dictionary<string, Sprite> dic_sprite;

        void Awake()
        {
            Get = this;

            dic_active = new Dictionary<string, List<GameObject>>();
            dic_inactive = new Dictionary<string, List<GameObject>>();
        }

        // 一次性创建多个，优化Bundle载入次数
        public async Task<List<GameObject>> SpawnNum<T>(
            string assetName, int num, Action action = null) where T : MonoBehaviour
        {
            List<GameObject> list_inactive = null;
            List<GameObject> list_active = null;
            var prefab = ResManager.LoadPrefab($"Prefabs/{assetName}");

            for (int i = 0; i < num; i++)
            {
                GameObject obj = null;

                //①没有缓存，新建key-value，新建obj
                Debug.Log($"①①①:{assetName}");
                if (dic_inactive.TryGetValue(assetName, out list_inactive) == false)
                {
                    list_inactive = new List<GameObject>();
                    dic_inactive.Add(assetName, list_inactive);

                    //var prefab = ResManager.LoadPrefab($"Prefabs/{assetName}");
                    obj = Instantiate(prefab, transform);
                    obj.name = assetName;
                    list_inactive.Add(obj);
                }

                //②有缓存，用完了
                if (list_inactive.Count == 0)
                {
                    //var prefab = ResManager.LoadPrefab($"Prefabs/{value}");
                    obj = Instantiate(prefab, transform);
                    obj.name = assetName;
                    list_inactive.Add(obj);
                }
                else
                {
                    obj = list_inactive[0];
                }
                obj.SetActive(true);
                list_inactive.RemoveAt(0);

                //③记录加入active
                if (dic_active.TryGetValue(assetName, out list_active) == false)
                {
                    list_active = new List<GameObject>();
                    dic_active.Add(assetName, list_active);
                }
                list_active.Add(obj);

                if (obj.GetComponent<T>() == false)
                    obj.AddComponent<T>();
                await Task.Delay(1);

                action?.Invoke();
            }

            return (list_active);
        }
        //inactive→active
        public GameObject Spawn(string assetName)
        {
            GameObject obj = null;
            List<GameObject> list_inactive = null;
            List<GameObject> list_active = null;

            //①没有缓存，新建key-value，新建obj
            Debug.Log($"①①①:{assetName}, {dic_inactive.ContainsKey(assetName)}");
            if (dic_inactive.TryGetValue(assetName, out list_inactive) == false)
            {
                //Debug.Log($"list_inactive: {list_inactive != null}"); //False, 就是null
                list_inactive = new List<GameObject>();
                dic_inactive.Add(assetName, list_inactive);

                var prefab = ResManager.LoadPrefab($"Prefabs/{assetName}");
                obj = Instantiate(prefab, transform);
                obj.name = assetName;
                list_inactive.Add(obj);

                //Debug.Log($"inactive.第一次创建: key={value}, value count={list_inactive.Count}");
            }

            //②有缓存，用完了
            Debug.Log($"②②②:{list_inactive.Count}");
            if (list_inactive.Count == 0)
            {
                var prefab = ResManager.LoadPrefab($"Prefabs/{assetName}");
                obj = Instantiate(prefab, transform);
                obj.name = assetName;
                list_inactive.Add(obj);

                //Debug.Log($"inactive.扩建: key={value}, value count={list_inactive.Count}");
            }
            else
            {
                obj = list_inactive[0];

                //Debug.Log($"inactive.使用缓存: key={value}, value count={list_inactive.Count}");
            }
            obj.SetActive(true);
            list_inactive.RemoveAt(0);


            //③记录加入active
            Debug.Log($"③③③:{dic_active.ContainsKey(assetName)}");
            if (dic_active.TryGetValue(assetName, out list_active) == false)
            {
                list_active = new List<GameObject>();
                dic_active.Add(assetName, list_active);

                //Debug.Log($"active.第一次创建: key={value}, value count={list_active.Count}");
            }
            list_active.Add(obj);

            //Debug.Log($"{value} / active:{dic_active.Count} / inactive:{dic_inactive.Count}");

            return obj;
        }

        //active→inactive
        public void Despawn(GameObject obj)
        {
            if (obj == null) return;
            string charaName = obj.name;

            dic_inactive[charaName].Add(obj);
            obj.transform.position = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.SetParent(transform);
            obj.SetActive(false);

            dic_active[charaName].Remove(obj);
        }
        public void DespawnType(string key)
        {
            List<GameObject> list = new List<GameObject>();
            if (dic_active.TryGetValue(key, out list))
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var obj = list[i];
                    Despawn(obj);
                }
            }
        }
        public void DespawnAll()
        {
            foreach (var item in dic_active)
            {
                var active_list = item.Value;
                for (int i = active_list.Count - 1; i >= 0; i--)
                {
                    var obj = active_list[i];
                    Despawn(obj);
                }
            }
        }

        public void Print()
        {
            Debug.Log($"<color=green>dic_active: {dic_active.Count}</color>");
            foreach (var item in dic_active)
            {
                Debug.Log($"---item_active: {item.Key}:{item.Value.Count}");
            }
            Debug.Log($"<color=red>dic_inactive: {dic_inactive.Count}</color>");
            foreach (var item in dic_inactive)
            {
                Debug.Log($"---item_inactive: {item.Key}:{item.Value.Count}");
            }
        }
        public static void StaticPrint()
        {
            Debug.Log("StaticPrint.2");
        }
    }
}