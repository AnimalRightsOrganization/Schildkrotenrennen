using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace HotFix
{
    public class Card : MonoBehaviour
    {
        [HideInInspector] public Sprite[] cardArray;
        [SerializeField] Image mMainSP;
        [SerializeField] CardAttribute cardAttribute;

        public void InitData(CardAttribute data)
        {
            cardAttribute = data;

            string combName = (data.cardColor.ToString().ToLower() + "" + (data.cardNum > 0 ? "+" : "") + (int)data.cardNum);
            //Debug.Log(combName);

            //var array = cardArray.Where(x => x.name == combName).ToList();
            //Debug.Log(array.Count);
            var sp = cardArray.First(x => x.name == combName);
            mMainSP.sprite = sp;
        }

        public void Select()
        {
            //Debug.Log("选中");
            //transform.DOScale(1.1f, 0.2f);

            //wMainGame.playCardEvent.Invoke(cardAttribute); //通知方式执行事件
        }

        public void UnSelect()
        {
            transform.DOScale(1f, 0.2f);
        }
    }
}