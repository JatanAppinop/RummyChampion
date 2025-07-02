using System.Collections;
using System.Collections.Generic;
using Appinop;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LudoGameRulesPopover : PopoverView
{
    [SerializeField] private bool isLoaded = false;
    private RectTransform rectTransform;
    [SerializeField] Image backdrop;


    private void Awake()
    {
        this.rectTransform = this.transform as RectTransform;
        backdrop.color = Color.clear;

    }

    public override void Show()
    {
        if (!isLoaded)
        {
            isLoaded = true;
        }

        this.gameObject.SetActive(true);
        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Bottom);
        this.gameObject.SetActive(true);
        rectTransform.MoveToPosition(Vector2.zero, duration: 0.2f);
        backdrop.DOFade(0.8f, 0.2f);

    }

    public override void Hide()
    {
        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Bottom, Animate: true, onComplete: () =>
        {
            this.gameObject.SetActive(false);
        });
    }

    public override void OnFocus(bool dataUpdated = false)
    {
    }

}
