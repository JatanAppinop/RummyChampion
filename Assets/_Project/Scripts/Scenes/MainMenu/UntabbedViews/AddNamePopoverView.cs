using System.Collections;
using System.Collections.Generic;
using Appinop;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AddNamePopoverView : PopoverView
{
    [SerializeField] private bool isLoaded = false;
    private RectTransform rectTransform;

    [SerializeField] AdvancedInputField textInputField;
    [SerializeField] PrimaryButton saveButton;
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

    public async void OnSaveButtonPressed()
    {
        saveButton.SetInteractable(false);

        if (textInputField.text == "")
        {
            textInputField.ShowError("Enter your Name");
            saveButton.SetInteractable(true);
            return;
        }

        string requestBodyJson = "{\"username\":\"" + textInputField.text + "\"}";

        var responce = await APIServices.Instance.PutAsync<SimpleBaseModel>(APIEndpoints.editProfile, requestBodyJson);

        if (responce != null && responce.success)
        {
            UnityNativeToastsHelper.ShowShortText(responce.message);
            await UserDataContext.Instance.RefreshData();
            PopoverViewController.Instance.GoBack();
        }
        else
        {
            UnityNativeToastsHelper.ShowShortText(responce.message);
            Debug.LogError("Update Profile Unsuccessfull. Error : " + responce.message);
        }
    }
}
