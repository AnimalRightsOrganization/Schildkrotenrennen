using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace HotFix
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Get;

        public Transform Parent;

        // UI存储栈
        public Dictionary<string, UIBase> stack;
        public Dictionary<string, UIBase> recyclePool;

        void Awake()
        {
            Get = this;

            Parent = GameObject.Find("Canvas").transform;
            stack = new Dictionary<string, UIBase>();
            recyclePool = new Dictionary<string, UIBase>();
        }

        public UIBase GetActiveUI()
        {
            var child = Parent.GetChild(Parent.childCount - 1);
            //Debug.Log($"GetActive: {child.name}");
            string scriptName = child.name;

            UIBase ui = null;
            if (stack.TryGetValue(scriptName, out ui) == false)
            {
                //Debug.LogError($"还没有创建：{scriptName}");
                return null;
            }
            return ui.GetComponent<UIBase>();
        }

        public T GetUI<T>() where T : UIBase
        {
            string scriptName = typeof(T).ToString().Replace("HotFix.", "");
            //Debug.Log($"GetUI: {scriptName}");
            UIBase ui = null;
            if (stack.TryGetValue(scriptName, out ui) == false)
            {
                //Debug.LogError($"还没有创建：{scriptName}");
                return null;
            }
            return ui.GetComponent<T>();
        }

        public T Push<T>() where T : UIBase
        {
            string scriptName = typeof(T).ToString().Split('.')[1];
            //Debug.Log($"Push<{scriptName}>");
            UIBase ui = null;
            if (stack.TryGetValue(scriptName, out ui))
            {
                return ui.GetComponent<T>();
            }
            if (recyclePool.TryGetValue(scriptName, out ui))
            {
                recyclePool.Remove(scriptName);
                stack.Add(scriptName, ui);
                //Debug.Log($"<color=yellow>ReUse{stack.Count}/{recyclePool.Count}</color>");
                ui.gameObject.SetActive(true);
                ui.transform.SetAsLastSibling();
                return ui.GetComponent<T>();
            }
            else
            {
                GameObject prefab = ResManager.LoadPrefab($"UI/{scriptName}"); //iOS区分大小写？
                GameObject obj = Instantiate(prefab, Parent);
                obj.transform.localPosition = Vector3.zero;
                obj.name = scriptName;

                if (obj.GetComponent<T>() == false)
                    obj.AddComponent<T>();
                var script = obj.GetComponent<T>();
                stack.Add(scriptName, script);
                //Debug.Log($"<color=yellow>New{stack.Count}/{recyclePool.Count}</color>");
                return script;
            }
        }

        public void Pop(UIBase ui, bool destroy = false)
        {
            string scriptName = ui.name;
            if (ui == null)
            {
                Debug.LogError("没有需要销毁的UI");
                return;
            }
            stack.Remove(scriptName);
            if (destroy == false)
            {
                recyclePool.Add(scriptName, ui);
                ui.gameObject.SetActive(false);
            }
            else
            {
                Destroy(ui.gameObject);
            }
        }
        public void PopAll(bool destroy = false)
        {
            foreach (var item in stack)
            {
                //Debug.Log($"{item.Key}---{item.Value.gameObject}");
                if (destroy == false)
                {
                    recyclePool.Add(item.Key, item.Value);
                    item.Value.gameObject.SetActive(false);
                }
                else
                {
                    Destroy(item.Value.gameObject);
                }
            }
            stack.Clear();
        }

        #region 测试

        public static void Test1()
        {
            Debug.Log("UIManager中的Test1");
        }
        public void Test2()
        {
            Debug.Log("Test2");
        }
        public void Test3(string value)
        {
            Debug.Log("Test3: " + value);
        }

        #endregion
    }
}