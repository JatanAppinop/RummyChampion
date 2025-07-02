using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public static class AnimationExtension {


    public static void AnimateLocalPosition(this Transform t, Vector3 pos, float speed)
    {
        t.DOLocalMove(pos, speed);
    }

    public static void AnimatePosition(this Transform t, Vector3 pos)
    {
        t.DOMove(pos, Constants.QUICK_ANIM_TIME);
    }

    public static void AnimateLocalRotation(this Transform t, Vector3 rot, float speed = -1)
    {
        if (speed == -1)
            speed = Constants.QUICK_ANIM_TIME;
        t.DOLocalRotate(rot, speed);
    }

    public static void AnimateParentScale(this Transform t, float scaleFactor = 1f)
    {
        t.DOScale(t.parent.localScale * scaleFactor, Constants.QUICK_ANIM_TIME);
    }


    public static void ShowCG(this CanvasGroup cg, float animTime = 0f)
    {
        if (cg != null)
        {
            cg.DOFade(1, animTime);
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
    }

    public static void HideCG(this CanvasGroup cg, float animTime = 0f)
    {
        if (cg != null)
        {
            cg.DOFade(0, animTime);
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }
}
