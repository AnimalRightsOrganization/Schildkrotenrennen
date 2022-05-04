using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix
{
    public class UI_Connect : UIBase
    {
        public Transform m_Rotate;

        void Awake()
        {
            m_Rotate = transform.Find("Background/Rotate");
        }

        void Update()
        {
            m_Rotate.Rotate(Vector3.back, 10);
        }
    }
}
