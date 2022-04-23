using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix
{
    public class Item_Room : UIBase
    {
        public BasePlayerData playerData;
        protected int SeatID;

        public Text m_NameText;
        public Button m_SelfBtn;

        void Awake()
        {
            m_NameText = transform.Find("Text").GetComponent<Text>();
            m_SelfBtn = this.GetComponent<Button>();
            m_SelfBtn.onClick.AddListener(OnSelfClick);
        }

        public void InitUI(BasePlayerData data, int seatId)
        {
            this.playerData = data;
            this.SeatID = seatId;
            m_NameText.text = playerData == null ? "空" : playerData.NickName;
        }

        void OnSelfClick()
        {
            //Debug.Log($"座位上的人是: {(playerData == null ? "空" : playerData.NickName)}");

            if (playerData == null)
            {
                Debug.Log("空座位，加入机器人");
                var ui_dialog = UIManager.Get().Push<UI_Dialog>();
                ui_dialog.Show("是否加入机器人？",
                    () => { ui_dialog.Hide(); }, "取消",
                    () =>
                    {
                        TcpChatClient.SendOperateSeat(SeatID, SeatOperate.ADD_BOT);
                        ui_dialog.Hide();
                    }, "确定");
            }
            else
            {
                Debug.Log($"{playerData.UserName}\n{TcpChatClient.m_PlayerManager.LocalPlayer.UserName}");
                if (playerData.UserName == TcpChatClient.m_PlayerManager.LocalPlayer.UserName)
                {
                    Debug.Log("是自己，没效果");
                }
                else
                {
                    Debug.Log("是别人，踢人");
                    var ui_dialog = UIManager.Get().Push<UI_Dialog>();
                    ui_dialog.Show("是否移除该用户？",
                        () => { ui_dialog.Hide(); }, "取消",
                        () =>
                        {
                            TcpChatClient.SendOperateSeat(SeatID, SeatOperate.KICK_PLAYER);
                            ui_dialog.Hide();
                        }, "确定");
                }
            }
        }
    }
}