using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItem : MonoBehaviour
{
    public Image mIdImage;
    public Sprite[] mIdArray;
    public GamePlayer gamePlayer;
    public bool isLocalPlayer;

    void Start()
    {
        
    }

    public void InitData(int chair_id)
    {
        int sid = Random.Range(0, 5);
        mIdImage.sprite = mIdArray[sid];

        gamePlayer = GameManager.Instance.playerList[chair_id];
    }

    public void OnPlay()
    {
        int cardid = Random.Range(0, 5);//简单AI，随机出一张手牌

    }
}
