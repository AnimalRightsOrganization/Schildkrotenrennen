//+----------------------------------------------------------------------------
// This class uses LeanTween for tweens
// This class is under development it is subject to changes
//+----------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO : Add multiple tweens


[System.Serializable]
public struct Vec3Tween
{

    public float Time;
    public Vector3 To; //public List<Vector3> To = new List<Vector3>();     // Multiple Point Tweens are not supported yet
    public Vector3 From;
}

[System.Serializable]
public class Tweens
{
    public Vec3Tween sPosition;
    public Vec3Tween sRotation;
    public Vec3Tween sScale;

    public Vector3 ePosition;
    public Vector3 eRotation;
    public Vector3 eScale;

    public bool IsPosition;
    public bool IsScale;
    public bool IsRotation;

    public LeanTweenType TweenType = LeanTweenType.easeOutBounce;

    private LTDescr m_handler = null;
//    private LTDescr m_handler2 = null;
    public void DoPositionTween(GameObject obj, System.Action OnTweenEnd = null, System.Action OnTweenStart = null)
    {
        if (!IsPosition) return;

#if UNITY_EDITOR
        string[] dimension = UnityEditor.UnityStats.screenRes.Split('x');
        int _width = System.Int32.Parse(dimension[0]);
        int _height = System.Int32.Parse(dimension[1]);
#endif

#if !UNITY_EDITOR
        int _width = Screen.width;
        int _height = Screen.height;
#endif

        var newPos = sPosition.From;
        newPos.x *= _width;
        newPos.y *= _height;
        obj.transform.position = newPos;

        var pos = sPosition.To;
        pos.x *= _width;
        pos.y *= _height;

        LTDescr id = LeanTween.move(obj, pos, sPosition.Time).setEase(TweenType);
        //   m_handler = id;
        if (OnTweenEnd != null)
            id.setOnComplete(OnTweenEnd);
    }


    public void EndDoPositionTween(GameObject obj, System.Action OnTweenEnd = null, System.Action OnTweenStart = null)
    {
        if (!IsPosition) return;

        var newPos = ePosition;
        newPos.x *= Screen.width;
        newPos.y *= Screen.height;

        LTDescr id = LeanTween.move(obj, newPos, sPosition.Time).setEase(TweenType);
        m_handler = id;
        if (OnTweenEnd != null)
            id.setOnComplete(OnTweenEnd);
    }


    public void DoScaleTween(GameObject obj, System.Action OnTweenEnd = null, System.Action OnTweenStart = null)
    {
        if (!IsScale) return;
        var mainWindow = obj.GetComponent<RectTransform>();
        mainWindow.transform.localScale = sScale.From;
        LTDescr id = LeanTween.scale(mainWindow, sScale.To, sScale.Time).setEase(TweenType);


        //   m_handler = id;
        if (OnTweenEnd != null)
            id.setOnComplete(OnTweenEnd);
    }


    public void EndDoScaleTween(GameObject obj, System.Action OnTweenEnd = null, System.Action OnTweenStart = null)
    {
        if (!IsScale) return;
        var mainWindow = obj.GetComponent<RectTransform>();
        LTDescr id = LeanTween.scale(mainWindow, eScale, sScale.Time).setEase(LeanTweenType.easeInBack);

        m_handler = id;
        if (OnTweenEnd != null)
            id.setOnComplete(OnTweenEnd);
    }




    public void DoRotationTween(GameObject obj, System.Action OnTweenEnd = null, System.Action OnTweenStart = null)
    {
        if (!IsRotation) return;
        obj.transform.rotation = Quaternion.Euler(sRotation.From);
        LTDescr id = LeanTween.rotate(obj, sRotation.To, sRotation.Time).setEase(TweenType);
        //   m_handler = id;
        if (OnTweenEnd != null)
            id.setOnComplete(OnTweenEnd);
    }


    public void EndDoRotationTween(GameObject obj, System.Action OnTweenEnd = null, System.Action OnTweenStart = null)
    {
        if (!IsRotation) return;
        LTDescr id = LeanTween.rotate(obj, eRotation, sRotation.Time).setEase(TweenType);

        m_handler = id;
        if (OnTweenEnd != null)
            id.setOnComplete(OnTweenEnd);
    }



    public LTDescr GetTweenHandler() { return m_handler; }


    public void PerformOnShowTweens(GameObject obj)
    {
        DoPositionTween(obj);
        DoScaleTween(obj);
        DoRotationTween(obj);
    }
    public void PerformOnHideTweens(GameObject obj, System.Action callBack = null)
    {
        EndDoPositionTween(obj, callBack);
        EndDoScaleTween(obj, callBack);
        EndDoRotationTween(obj, callBack);
    }
}