using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Appinop;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AddBankAccPopoverView : PopoverView
{
    [SerializeField] private bool isLoaded = false;
    private RectTransform rectTransform;

    [SerializeField] AdvancedInputField nameInputField;
    [SerializeField] AdvancedInputField bankaccountInputField;
    [SerializeField] AdvancedInputField ifscInputField;
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
        bankaccountInputField.text = "";
        ifscInputField.text = "";
        nameInputField.HideError();
        bankaccountInputField.HideError();
        ifscInputField.HideError();
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

            string requestBodyJson = $"{{\"accountNumber\":\"{bankaccountInputField.text}\",\"ifsc\":\"{ifscInputField.text}\",\"accountType\":\"bank_account\",\"fullname\":\"{nameInputField.text}\"}}";
            var responce = await APIServices.Instance.PostAsync<SimpleBaseModel>(APIEndpoints.addBankAccount, requestBodyJson);

            if (responce != null && responce.success)
            {
                UnityNativeToastsHelper.ShowShortText("Bank Account Added Successfully.");
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

        if (string.IsNullOrEmpty(bankaccountInputField.text))
        {
            bankaccountInputField.ShowError("Enter Bank Account Number");
            validInputs = false;
        }
        if (!isValid_Bank_Acc_Number(bankaccountInputField.text))
        {
            bankaccountInputField.ShowError("Enter A Valid Bank Account Number");
            validInputs = false;
        }

        if (string.IsNullOrEmpty(ifscInputField.text))
        {
            ifscInputField.ShowError("Enter IFSC Code");
            validInputs = false;
        }
        if (!isValidIFSCCode(ifscInputField.text))
        {
            ifscInputField.ShowError("Enter Valid IFSC Code");
            validInputs = false;
        }

        return validInputs;
    }

    public static bool isValid_Bank_Acc_Number(string str)
    {
        string strRegex = @"^[0-9]{9,18}$";
        Regex re = new Regex(strRegex);
        if (re.IsMatch(str))
            return (true);
        else
            return (false);
    }

    public static bool isValidIFSCCode(string str)
    {
        string strRegex = @"^[A-Za-z]{4}0[A-Za-z0-9]{6}$";

        Regex re = new Regex(strRegex);
        if (re.IsMatch(str))
            return (true);
        else
            return (false);
    }
}
