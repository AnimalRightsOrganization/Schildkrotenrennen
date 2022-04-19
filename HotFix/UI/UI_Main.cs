using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace HotFix
{
    public class UI_Main : UIBase
    {
        private int playerNum;

        [SerializeField] Button m_ListBtn;
        [SerializeField] Button m_CreateButton;
        [SerializeField] Button m_SettingsButton;
        [SerializeField] Button m_ExitButton;

        [SerializeField] GameObject m_CreatePanel;
        [SerializeField] Button m_CloseCreatePanel;
        [SerializeField] InputField m_NameInput;
        [SerializeField] InputField m_KeyInput;
        [SerializeField] Text m_NumText;
        [SerializeField] Button m_LeftBtn;
        [SerializeField] Button m_RightBtn;
        [SerializeField] Button m_ConfirmBtn;

        [SerializeField] GameObject m_ListPanel;
        [SerializeField] Button m_CloseListPanel;

        void Awake()
        {
            playerNum = 2;

            m_ListBtn = transform.Find("ListBtn").GetComponent<Button>();
            m_CreateButton = transform.Find("CreateBtn").GetComponent<Button>();
            m_SettingsButton = transform.Find("SettingsBtn").GetComponent<Button>();
            m_ExitButton = transform.Find("ExitBtn").GetComponent<Button>();
            m_ListBtn.onClick.AddListener(OnListBtnClick);
            m_CreateButton.onClick.AddListener(OnCreateBtnClick);
            m_SettingsButton.onClick.AddListener(OnSettingsBtnClick);
            m_ExitButton.onClick.AddListener(OnExitBtnClick);

            var createTrans = transform.Find("CreatePanel");
            m_CreatePanel = createTrans.gameObject;
            m_CloseCreatePanel = createTrans.Find("CloseBtn").GetComponent<Button>();
            m_NameInput = createTrans.Find("NamePanel/NameInput").GetComponent<InputField>();
            m_KeyInput = createTrans.Find("KeyPanel/KeyInput").GetComponent<InputField>();
            m_NumText = createTrans.Find("NumPanel/Num").GetComponent<Text>();
            m_LeftBtn = createTrans.Find("NumPanel/LeftBtn").GetComponent<Button>();
            m_RightBtn = createTrans.Find("NumPanel/RightBtn").GetComponent<Button>();
            m_ConfirmBtn = createTrans.Find("ConfirmBtn").GetComponent<Button>();
            m_CloseCreatePanel.onClick.AddListener(() => { m_CreatePanel.SetActive(false); });
            m_LeftBtn.onClick.AddListener(OnLeftBtnClick);
            m_RightBtn.onClick.AddListener(OnRightBtnClick);
            m_ConfirmBtn.onClick.AddListener(OnConfirmBtnClick);
            RefreshPlayerNum();
            m_CreatePanel.SetActive(false);

            m_ListPanel = transform.Find("ListPanel").gameObject;
            m_CloseListPanel = transform.Find("ListPanel/CloseBtn").GetComponent<Button>();
            m_CloseListPanel.onClick.AddListener(() => { m_ListPanel.SetActive(false); });
            m_ListPanel.SetActive(false);
        }

        void OnEnable()
        {
            playerNum = 2;
            RefreshPlayerNum();
            m_CreatePanel.SetActive(false);
            m_ListPanel.SetActive(false);

            //NetPacketManager.RegisterEvent(action);
        }

        void OnDisable()
        {
            //NetPacketManager.UnRegisterEvent(action);
        }

        //UnityAction<PacketType> action;
        //public void OnNetCallback(UnityAction<PacketType> act) { }

        void OnListBtnClick()
        {
            //弹出大厅列表
            m_ListPanel.SetActive(true);
        }
        void OnCreateBtnClick()
        {
            // 弹出创建游戏选项，人数、人机、是否公开
            //var obj = new GameObject("TEMP_Local");
            //var script = obj.AddComponent<TEMP_Local>();
            //UIManager.Get().Push<UI_Game>();
            m_CreatePanel.SetActive(true);
        }
        void OnSettingsBtnClick()
        {
            UIManager.Get().Push<UI_Settings>();
        }
        void OnExitBtnClick()
        {
            UIManager.Get().Pop(this);
        }

        void OnLeftBtnClick()
        {
            playerNum--;
            if (playerNum < 2)
            {
                playerNum = 5;
            }
            RefreshPlayerNum();
        }
        void OnRightBtnClick()
        {
            playerNum++;
            if (playerNum > 5)
            {
                playerNum = 2;
            }
            RefreshPlayerNum();
        }
        void RefreshPlayerNum()
        {
            m_NumText.text = $"{playerNum}";
            //Debug.Log($"人数={playerNum}");
        }
        void OnConfirmBtnClick()
        {
            Debug.Log($"确认");

            C2S_CreateRoom cmd = new C2S_CreateRoom { Num = playerNum };
            TcpChatClient.SendAsync(PacketType.C2S_CreateRoom, cmd);

            m_CreatePanel.SetActive(false);
        }
    }
}