using System.Collections;
using System.Collections.Generic;
using Appinop;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ChangeLanguagePopoverView : PopoverView
{
    [SerializeField] private bool isLoaded = false;

    private bool isDataUpdated = true;
    private RectTransform rectTransform;

    [SerializeField] TextMeshProUGUI errorLabel;

    [SerializeField] Color errorColor;

    private Sequence errorTween;

    private void Awake()
    {
        this.rectTransform = this.transform as RectTransform;
        errorColor = errorLabel.color;
        errorLabel.gameObject.SetActive(true);
    }

    public override void Hide()
    {
        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left, Animate: true, onComplete: () =>
        {
            this.gameObject.SetActive(false);
        });
    }


    public override void Show()
    {
        if (!isLoaded)
        {
            isLoaded = true;
        }

        if (!rectTransform)
        {
            rectTransform = this.transform as RectTransform;
        }

        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left);
        this.gameObject.SetActive(true);
        rectTransform.MoveToPosition(Vector2.zero, duration: 0.2f);
        errorLabel.color = Color.clear;

    }

    public void onSaveButtonPressed()
    {

        PopoverViewController.Instance.GoBack(isDataUpdated);
    }
    public override void OnFocus(bool dataUpdated = false)
    {
    }

    public void onLanguageTap()
    {

        if (errorTween.IsActive())
        {
            return;

        }
        UnityNativeToastsHelper.ShowShortText("Coming soon");
        errorTween = DOTween.Sequence();
        errorTween.Append(errorLabel.DOColor(errorColor, 0.2f));
        errorTween.AppendInterval(0.5f);
        errorTween.Append(errorLabel.DOColor(Color.clear, 0.2f));
        errorTween.Play();
    }
}
