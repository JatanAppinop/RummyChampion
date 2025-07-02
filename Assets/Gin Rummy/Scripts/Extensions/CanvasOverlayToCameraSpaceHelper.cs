using System;
using UnityEngine;
public static class CanvasOverlayToCameraSpaceHelper
{

    public static Vector3 ConvertOverlayPositonToCameraSpace(this Vector3 pos)
    {
        Vector3 screenPoint = pos;
        screenPoint.z =  Constants.PLANE_DISTACE_OF_CANVAS;
       return Camera.main.ScreenToWorldPoint(screenPoint);
    }

    public static Vector3 ConvertOverlayPositonToCameraSpace(this Vector2 pos)
    {
        Vector3 position = pos;
        return ConvertOverlayPositonToCameraSpace(position);
    }

    public static Vector3 ConvertCameraSpaceToOverlayPos(this Vector3 pos)
    {
        return Camera.main.WorldToScreenPoint(pos);
    }
}
