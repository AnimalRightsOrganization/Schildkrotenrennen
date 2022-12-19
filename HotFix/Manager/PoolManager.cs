using System.Collections.Generic;
using UnityEngine;

namespace HotFix
{
    public class PoolManager : MonoBehaviour
    {
        static PoolManager _instance;
        public static PoolManager Get
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<PoolManager>();
                return _instance;
            }
        }

        private Dictionary<string, List<GameObject>> dic_inactive;
        private Dictionary<string, List<GameObject>> dic_active;

        void Awake()
        {
            dic_inactive = new Dictionary<string, List<GameObject>>();
            dic_active = new Dictionary<string, List<GameObject>>();

            //int count = ConfigManager.Get().m_CharacterList.Length;
            //int count = Enum.GetValues(typeof(CharacterName)).Length;
            int count = 0;
            //Debug.Log($"角色总数={count}"); //从0开始
            for (int i = 0; i < count; i++)
            {
                //string charaName = ((CharacterName)i).ToString();
                string charaName = "";
                //Debug.Log($"{i}---{charaName}");

                var prefab = ResManager.LoadPrefab($"Prefabs/{charaName}");
                var lst = new List<GameObject>();

                var asset1 = Instantiate(prefab, transform);
                asset1.name = charaName;
                lst.Add(asset1);
                asset1.SetActive(false);

                var asset2 = Instantiate(prefab, transform);
                asset2.name = charaName;
                lst.Add(asset2);
                asset2.SetActive(false);

                dic_inactive.Add(charaName, lst);
            }
        }

        public GameObject Spawn(string value)
        {
            GameObject obj = null;
            if (dic_inactive.ContainsKey(value))
            {
                var lst = dic_inactive[value];
                if (lst.Count > 0)
                {
                    obj = lst[0];
                    obj.SetActive(true);
                    lst.RemoveAt(0);

                }
                else
                {
                    var prefab = ResManager.LoadPrefab($"Prefabs/{value}");
                    obj = Instantiate(prefab, transform);
                    obj.name = value;
                    obj.SetActive(true);
                    lst.Add(obj);
                }

                if (dic_active.ContainsKey(value))
                {
                    var active_list = dic_active[value];
                    active_list.Add(obj);
                }
                else
                {
                    var active_list = new List<GameObject>();
                    active_list.Add(obj);
                    dic_active.Add(value, active_list);
                }
            }
            return obj;
        }

        public void Despawn(GameObject value)
        {
            if (value == null) return;
            string charaName = value.gameObject.name;
            dic_inactive[charaName].Add(value);
            value.transform.position = Vector3.zero;
            value.transform.localScale = Vector3.one;
            value.transform.SetParent(transform);
            value.SetActive(false);

            //dic_active[charaName].RemoveAt(0);
            dic_active[charaName].Remove(value);
            //Debug.Log($"Despawn: {charaName}");
        }

        public void DespawnAll()
        {
            foreach (var item in dic_active)
            {
                //string charaName = item.Key;
                var active_list = item.Value;
                for (int i = active_list.Count - 1; i >= 0; i--)
                {
                    var obj = active_list[i];
                    //Debug.Log($"{i}: {obj.gameObject.name}");

                    //active_list.Remove(obj);
                    //dic_inactive[charaName].Add(obj);
                    Despawn(obj);
                }
            }
        }
    }
}