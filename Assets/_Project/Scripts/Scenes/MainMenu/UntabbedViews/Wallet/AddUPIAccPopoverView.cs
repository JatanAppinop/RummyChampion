using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Appinop;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AddUPIAccPopoverView : PopoverView
{
    [SerializeField] private bool isLoaded = false;
    private RectTransform rectTransform;

    [SerializeField] AdvancedInputField nameInputField;
    [SerializeField] AdvancedInputField upiInputField;
    [SerializeField] PrimaryButton addAccountButton;
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

        ResetView();
    }

    private void ResetView()
    {
        addAccountButton.SetInteractable(true);
        nameInputField.text = "";
        upiInputField.text = "";
        nameInputField.HideError();
        upiInputField.HideError();
    }

    public override void OnFocus(bool dataUpdated = false)
    {
    }

    public async void OnAddButtonPressed()
    {

        bool validInputs = ValidateInputs();

        if (validInputs)
        {
            addAccountButton.SetInteractable(false);

            string requestBodyJson = $"{{\"vpa\":\"{upiInputField.text}\",\"accountType\":\"vpa\",\"fullname\":\"{nameInputField.text}\"}}";
            var responce = await APIServices.Instance.PostAsync<SimpleBaseModel>(APIEndpoints.addBankAccount, requestBodyJson);

            if (responce != null && responce.success)
            {
                UnityNativeToastsHelper.ShowShortText("UPI ID Added Successfully.");
                PopoverViewController.Instance.GoBack(true);
            }
            else
            {
                Debug.LogWarning("Error : " + responce.message);
                UnityNativeToastsHelper.ShowShortText(responce.message);
            }

            addAccountButton.SetInteractable(true);

        }
    }

    private bool ValidateInputs()
    {
        bool validInputs = true;
        if (string.IsNullOrEmpty(nameInputField.text))
        {
            nameInputField.ShowError("Enter your Name");
            validInputs = false;
        }

        if (string.IsNullOrEmpty(upiInputField.text))
        {
            upiInputField.ShowError("Enter UPI Id");
            validInputs = false;
        }
        if (!isValidUPIId(upiInputField.text))
        {
            upiInputField.ShowError("Enter A UPI Id");
            validInputs = false;
        }

        return validInputs;
    }

    public static bool isValidUPIId(string str)
    {
        string strRegex = @"^[a-zA-Z0-9.\-_]{2,256}@[a-zA-Z]{2,64}$";

        Regex re = new Regex(strRegex);
        if (re.IsMatch(str))
            return (true);
        else
            return (false);
    }
}
