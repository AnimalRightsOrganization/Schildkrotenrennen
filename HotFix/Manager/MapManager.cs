using UnityEngine;

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
            new Vector3(-2.0f, 0.25f, 1.6f),
            new Vector3(-0.5f, 0.25f, 1.6f),
            new Vector3(+1.0f, 0.25f, 1.6f),
            new Vector3(+2.5f, 0.25f, 1.6f),
            new Vector3(+4.0f, 0.25f, 1.6f),
        };
        public static Vector3 orig_p = new Vector3(0, 18, -2);
        public static Vector3 orig_r = new Vector3(60, 0, 0);

        void Awake()
        {
            Get = this;
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