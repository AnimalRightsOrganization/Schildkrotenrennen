using UnityEngine;

namespace HotFix
{
    public class UI_Connect : UIBase
    {
        public Transform m_Rotate;

        void Awake()
        {
            m_Rotate = transform.Find("Mask/Rotate");
        }

        void Update()
        {
            m_Rotate.Rotate(Vector3.back, 10);
        }
    }
}