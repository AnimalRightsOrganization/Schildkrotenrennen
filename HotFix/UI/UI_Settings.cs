using UnityEngine;
using UnityEngine.UI;

namespace HotFix
{
    public class UI_Settings : UIBase
    {
        public Button m_CloseBtn;

        public Toggle m_MusicToggle;
        public GameObject m_MusicOn;
        public GameObject m_MusicOff;

        public Toggle m_SoundToggle;
        public GameObject m_SoundOn;
        public GameObject m_SoundOff;

        void Awake()
        {
            m_CloseBtn = transform.Find("CloseBtn").GetComponent<Button>();
            m_CloseBtn.onClick.AddListener(OnCloseBtnClick);

            m_MusicToggle = transform.Find("MusicToggle").GetComponent<Toggle>();
            m_MusicOn = transform.Find("MusicToggle/Background/On").gameObject;
            m_MusicOff = transform.Find("MusicToggle/Background/Off").gameObject;
            m_MusicToggle.onValueChanged.AddListener(OnMusicChanged);

            m_SoundToggle = transform.Find("SoundToggle").GetComponent<Toggle>();
            m_SoundOn = transform.Find("SoundToggle/Background/On").gameObject;
            m_SoundOff = transform.Find("SoundToggle/Background/Off").gameObject;
            m_SoundToggle.onValueChanged.AddListener(OnSoundChanged);
        }

        void Start()
        {
            m_MusicOn.SetActive(m_MusicToggle.isOn);
            m_MusicOff.SetActive(!m_MusicToggle.isOn);
            m_SoundOn.SetActive(m_SoundToggle.isOn);
            m_SoundOff.SetActive(!m_SoundToggle.isOn);
        }

        void OnCloseBtnClick()
        {
            UIManager.Get().Pop(this);
        }

        void OnMusicChanged(bool value)
        {
            m_MusicOn.SetActive(value);
            m_MusicOff.SetActive(!value);
        }

        void OnSoundChanged(bool value)
        {
            m_SoundOn.SetActive(value);
            m_SoundOff.SetActive(!value);
        }
    }
}