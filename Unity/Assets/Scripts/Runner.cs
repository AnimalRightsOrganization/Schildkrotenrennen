using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Runner : MonoBehaviour
{
    public RunnerColor mColor;
    [SerializeField] Sprite[] mColorSp = new Sprite[5];
    [SerializeField] Image mColorImage;
    [SerializeField] int CurrentWayPoint;

    void Awake()
    {
        mColorImage = GetComponent<Image>();
    }

    void Update()
    {
        
    }

    public void InitData(int id)
    {
        CurrentWayPoint = 0;
        mColor = (RunnerColor)id;
        mColorImage.sprite = mColorSp[id];
    }
}
