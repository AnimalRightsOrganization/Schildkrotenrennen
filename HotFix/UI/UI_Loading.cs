using UnityEngine.UI;

namespace HotFix
{
    public class UI_Loading : UIBase
    {
        public Text m_ProgressText;
        public Slider m_ProgressSlider;
        private int Total;

        void Awake()
        {
            m_ProgressText = transform.Find("Slider").Find("Text").GetComponent<Text>();
            m_ProgressSlider = transform.Find("Slider").GetComponent<Slider>();
            m_ProgressSlider.minValue = 0;
        }

        public void OnStart(int total)
        {
            this.Total = total;
        }
        public void OnUpdate(int progress)
        {
            float percent = (float)progress / (float)this.Total;
            m_ProgressText.text = $"{(percent * 100).ToString("F0")}%";

            m_ProgressSlider.maxValue = this.Total;
            m_ProgressSlider.value = progress;
        }
    }
}