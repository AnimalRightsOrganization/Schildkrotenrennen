using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using kcp2k.Examples;

namespace HotFix
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager Get;

        private Transform Map;
        public Transform[] Rock;
        public Item_Turtle[] Turtle;
        //public const float TURTLE_HEIGHT = 0.25f;
        public static Vector3[] START_POS = new Vector3[]
        {
            new Vector3(1, 0.25f, 1.6f),
            new Vector3(2, 0.25f, 1.6f),
            new Vector3(3, 0.25f, 1.6f),
            new Vector3(4, 0.25f, 1.6f),
            new Vector3(5, 0.25f, 1.6f),
        };
        public static Vector3 orig_p = new Vector3(0, 18, -2);
        public static Vector3 orig_r = new Vector3(60, 0, 0);

        void Awake()
        {
            Get = this;
        }

        void Update()
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                // Check if finger is over a UI element
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    Debug.Log("Touched the UI");
                    return;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    //Debug.Log($"click: {hit.transform.name}");
                    string _name = hit.transform.name.Split('_')[1];
                    int index = int.Parse(_name);

                    var data = KcpChatClient.m_ClientRoom.GridData[index];

                    if (index > 0 && data.Count > 1)
                    {
                        Debug.Log($"格子_{index}: {data.Count}个");

                        // 显示堆叠详情
                        var tw0 = Camera.main.transform.DOMove(hit.transform.position + new Vector3(0, 6, -6), 0.3f);
                        var tw1 = Camera.main.transform.DORotate(new Vector3(30, 0, 0), 0.3f);
                        tw0.OnComplete(() =>
                        {
                            var tw2 = Camera.main.transform.DOMove(orig_p, 0.3f);
                            var tw3 = Camera.main.transform.DORotate(orig_r, 0.3f);
                            tw2.SetDelay(1); //秒
                            tw3.SetDelay(1);
                            tw2.Play();
                            tw3.Play();
                        });
                    }
                }
            }
        }

        public void InitAssets()
        {
            Debug.Log($"InitAssets---{gameObject.name}");

            Map = transform.Find("Map");

            Rock = new Transform[10];
            for (int i = 0; i < Map.childCount; i++)
            {
                var item = Map.GetChild(i);
                Rock[i] = item;
            }

            Turtle = new Item_Turtle[5];
            var turtles = transform.Find("Turtles");
            for (int i = 0; i < turtles.childCount; i++)
            {
                var item = turtles.GetChild(i);
                if (item.gameObject.GetComponent<Item_Turtle>() == false)
                    item.gameObject.AddComponent<Item_Turtle>();
                Turtle[i] = item.gameObject.GetComponent<Item_Turtle>();
                Turtle[i].InitData(i);
            }
        }
    }
}