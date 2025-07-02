using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationPauseWatcher : MonoBehaviour {

    private static Action OnAppPauseCB;
    private static Action OnAppUnPauseCB;

    public static void RegisterOnAppPauseCB(Action cb)
    {
        OnAppPauseCB += cb;
    }


    public static void UnregisterOnAppPauseCB(Action cb)
    {
        OnAppPauseCB -= cb;
    }

    public static void RegisterOnAppUnPause(Action cb)
    {
        OnAppUnPauseCB += cb;
    }

    public static void UnregisterOnAppUnPause(Action cb)
    {
        OnAppUnPauseCB -= cb;
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            OnAppPauseCB.RunAction();
        else
            OnAppUnPauseCB.RunAction();
    }

   
}
