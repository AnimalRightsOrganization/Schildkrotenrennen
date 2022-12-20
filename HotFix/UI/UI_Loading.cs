using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix
{
    public class UI_Loading : UIBase
    {
        public Slider m_Progress;

        void Awake()
        {
            m_Progress = transform.Find("Slider").GetComponent<Slider>();
        }

        public void OnUpdate(Task task)
        {

        }
    }
}