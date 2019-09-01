using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum RunnerColor
{
    Red = 0,
    Yellow = 1,
    Green = 2,
    Blue = 3,
    Purple = 4
}
public class Runner : MonoBehaviour
{
    [SerializeField] RunnerColor mColor;
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
