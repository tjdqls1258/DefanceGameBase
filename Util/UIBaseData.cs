using System;
using UnityEngine;

[Serializable]
public class UIBaseData
{
    public enum UIType
    {
        Command,
        MainUI,
        InGameUI,
    }

    public string dataName;
    public UIType uiType;
    public float anchorPosX;
    public float anchorPosY;
    public float sizeDetailX;
    public float sizeDetailY;
    public float anchorminX;
    public float anchorminY;
    public float anchorsMinX;
    public float anchorsMinY;
    public float anchorsMaxX;
    public float anchorsMaxY;
    public float pivotX;
    public float pivotY;

    public void SettingAnchorPos(Vector2 anchor)
    {
        anchorPosX = anchor.x; 
        anchorPosY = anchor.y;
    }

    public Vector2 GetAnchorPos()
    {
        return new Vector2(anchorPosX, anchorPosY);
    }

    public void SettingSizeDetail(Vector2 sizeDetail)
    {
        sizeDetailX = sizeDetail.x;
        sizeDetailY = sizeDetail.y;
    }

    public Vector2 GetSizeDetail()
    {
        return new Vector2(sizeDetailX, sizeDetailY);
    }

    public void SettingAnchorMinMax(Vector2 anchorsMin, Vector2 anchorsMax)
    {
        anchorsMinX = anchorsMin.x;
        anchorsMinY = anchorsMin.y;

        anchorsMaxX = anchorsMax.x;
        anchorsMaxY = anchorsMax.y;
    }

    public (Vector2 min, Vector2 max) GetAchorMinMax()
    {
        return (new Vector2(anchorsMinX, anchorsMinY), new Vector2(anchorsMaxX, anchorsMaxY));
    }

    public void SettingPivot(Vector2 pivot)
    {
        pivotX = pivot.x;
        pivotY = pivot.y;
    }

    public Vector2 GetPivot()
    {
        return new Vector2(pivotX, pivotY);
    }
}
