using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItem : MonoBehaviour
{
    public Image mIdImage;
    public Sprite[] mIdArray;

    void Start()
    {
        
    }

    public void InitData()
    {
        int id = Random.Range(0, 5);
        mIdImage.sprite = mIdArray[id];
    }
}
