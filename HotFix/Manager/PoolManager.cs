using System.Collections.Generic;
using UnityEngine;

namespace HotFix
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Get;

        public Dictionary<string, List<GameObject>> dic_active;
        public Dictionary<string, List<GameObject>> dic_inactive;

        void Awake()
        {
            Get = this;

            dic_active = new Dictionary<string, List<GameObject>>();
            dic_inactive = new Dictionary<string, List<GameObject>>();
        }

        // 预创建，关闭显示
        public GameObject Prespawn(string value)
        {
            var obj = Spawn(value);
            obj.SetActive(false);
            return obj;
        }
        //inactive→active
        public GameObject Spawn(string value)
        {
            GameObject obj = null;
            List<GameObject> list_inactive = null;
            List<GameObject> list_active = null;

            //①没有缓存，新建key-value，新建obj
            Debug.Log("①①①");
            if (dic_inactive.TryGetValue(value, out list_inactive) == false)
            {
                //Debug.Log($"list_inactive: {list_inactive != null}"); //False, 就是null
                list_inactive = new List<GameObject>();
                dic_inactive.Add(value, list_inactive);

                var prefab = ResManager.LoadPrefab($"Prefabs/{value}");
                obj = Instantiate(prefab, transform);
                obj.name = value;
                list_inactive.Add(obj);

                //Debug.Log($"inactive.第一次创建: key={value}, value count={list_inactive.Count}");
            }

            //②有缓存，用完了
            Debug.Log("②②②");
            if (list_inactive.Count == 0)
            {
                var prefab = ResManager.LoadPrefab($"Prefabs/{value}");
                obj = Instantiate(prefab, transform);
                obj.name = value;
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
            Debug.Log("③③③");
            if (dic_active.TryGetValue(value, out list_active) == false)
            {
                list_active = new List<GameObject>();
                dic_active.Add(value, list_active);

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
            //Debug.Log($"Despawn: key={charaName}, count={dic_inactive.ContainsKey(charaName)}");
            //List<GameObject> lst = new List<GameObject>();
            //dic_inactive.TryGetValue(charaName, out lst);

            dic_inactive[charaName].Add(obj);
            obj.transform.position = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.SetParent(transform);
            obj.SetActive(false);

            dic_active[charaName].Remove(obj);
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
    }
}