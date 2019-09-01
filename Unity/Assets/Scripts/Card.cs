using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] Image mMainSP;
    [SerializeField] Sprite[] cardArray;
    [SerializeField] CardAttribute cardAttribute;

    void Start()
    {

    }

    public void InitData(CardAttribute data)
    {
        cardAttribute = data;

        string combName = (data.cardColor.ToString().ToLower() + "" + (data.cardNum > 0 ? "+" : "") + (int)data.cardNum);
        Debug.Log(combName);

        //var array = cardArray.Where(x => x.name == combName).ToList();
        //Debug.Log(array.Count);
        var sp = cardArray.First(x => x.name == combName);
        mMainSP.sprite = sp;
    }
}
