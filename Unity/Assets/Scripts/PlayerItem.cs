using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItem : MonoBehaviour
{
    [SerializeField] Image mIdImage;
    [HideInInspector] public Sprite[] mIdArray;
    public GamePlayer gamePlayer;
    
    // 本地用户
    [Header("----- 本地用户 -----")]
    public bool isLocalPlayer;
    public List<Card> handCards;

    void Start()
    {
        
    }

    public void InitData(int chair_id)
    {
        //isLocalPlayer = (wMainGame.Instance.identify == chair_id);

        int sid = Random.Range(0, 5); //服务器下发身份颜色
        mIdImage.sprite = mIdArray[sid];

        gamePlayer = GameManager.Instance.playerList[chair_id];

        //handCards = wMainGame.Instance.cardList;
    }

    public void OnPlay()
    {
        if (isLocalPlayer)
        {

        }
        else
        {
            int cardid = Random.Range(0, 5);//简单AI，随机出一张手牌
        }
    }
}
