using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Appinop;
using Appinop.PhonePePlugin;
using Appinop.SecureWebView;
using DG.Tweening;
using Newtonsoft.Json;
using SecPlayerPrefs;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DepositCalcPopover : PopoverView
{

    [SerializeField] private bool isLoaded = false;
    private RectTransform rectTransform;
    [SerializeField] Image backdrop;

    [Space]
    [Header("Label Reference")]
    [SerializeField] TextMeshProUGUI sgstLabel;
    [SerializeField] TextMeshProUGUI cgstLabel;
    [SerializeField] TextMeshProUGUI totalGstLabel;
    [SerializeField] GameObject AdditionalBonusGO;

    [Space]
    [Header("Value Reference")]
    [SerializeField] TextMeshProUGUI cashToAddValLabel;
    [SerializeField] TextMeshProUGUI exlTaxValLabel;
    [SerializeField] TextMeshProUGUI sgstValLabel;
    [SerializeField] TextMeshProUGUI cgstValLabel;
    [SerializeField] TextMeshProUGUI totalGstValLabel;
    [SerializeField] TextMeshProUGUI discountBonusValLabel;
    [SerializeField] TextMeshProUGUI AdditionalBonusValLabel;
    [SerializeField] TextMeshProUGUI AddCashValLabel;
    [Header("Other Reference")]
    [SerializeField] PrimaryButton DepositButton;
    [SerializeField] PrimaryButton CancelButton;
    [Header("Private Reference")]
    [SerializeField] string rs = "₹";
    [SerializeField] double sgstPerc;
    [SerializeField] double cgstPerc;
    [SerializeField] double totalGstPerc;
    [SerializeField] double bonusPerc;
    [SerializeField] int amountToDeposit;


    private void Awake()
    {
        this.rectTransform = this.transform as RectTransform;
        backdrop.color = Color.clear;

    }

    public override void Show(params KeyValuePair<string, object>[] args)
    {
        if (!isLoaded)
        {
            isLoaded = true;
            SettingsDataContext.Instance.PaymentSettingsDataChanged.AddListener(OnSettingsDataUpdated);
            UpdateSettingsData(SettingsDataContext.Instance.PaymentSettings);
        }


        amountToDeposit = args.GetValueByKey<int>(Appinop.Constants.KDepositAmount);

        this.gameObject.SetActive(true);
        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Bottom);
        this.gameObject.SetActive(true);
        rectTransform.MoveToPosition(Vector2.zero, duration: 0.2f);
        backdrop.DOFade(0.8f, 0.2f);

        UpdateLabels();

    }

    private void UpdateLabels()
    {

        //Update Labels
        string sgstText = sgstLabel.text;
        sgstText = sgstText.Replace("#", $"({sgstPerc} %)");
        sgstLabel.SetText(sgstText);

        string cgstText = cgstLabel.text;
        cgstText = cgstText.Replace("#", $"({cgstPerc} %)");
        cgstLabel.SetText(cgstText);

        string totalGstText = totalGstLabel.text;
        totalGstText = totalGstText.Replace("#", $"({cgstPerc + sgstPerc} %)");
        totalGstLabel.SetText(totalGstText);

        //Update Calculations
        cashToAddValLabel.SetText(rs + amountToDeposit.ToTwoDecimalString());

        // Calculate the factor
        double factor = 1 + (totalGstPerc / 100);

        // Calculate the base amount before GST
        double baseAmount = amountToDeposit / factor;
        exlTaxValLabel.SetText(rs + baseAmount.ToTwoDecimalString(true));

        // Calculate SGST and CGST amounts
        double sgstAmount = baseAmount * (sgstPerc / 100);
        sgstValLabel.SetText("-" + rs + sgstAmount.ToTwoDecimalString(true));
        double cgstAmount = baseAmount * (cgstPerc / 100);
        cgstValLabel.SetText("-" + rs + cgstAmount.ToTwoDecimalString(true));

        double totalGST = sgstAmount + cgstAmount;
        totalGstValLabel.SetText("-" + rs + totalGST.ToTwoDecimalString(true));

        double totalAmount = baseAmount + sgstAmount + cgstAmount;
        totalAmount = Math.Round(totalAmount);
        if (totalAmount != amountToDeposit)
        {
            Debug.LogError("Amount Mismatch : " + totalAmount + "" + amountToDeposit);
        }

        discountBonusValLabel.SetText("+" + rs + totalGST.ToTwoDecimalString(true));

        double additionalBonus = amountToDeposit * (bonusPerc / 100);

        AdditionalBonusValLabel.SetText("+" + rs + additionalBonus.ToTwoDecimalString(true));

        double totalDepositAmount = baseAmount + totalGST + additionalBonus;

        AddCashValLabel.SetText(rs + totalDepositAmount.ToTwoDecimalString(true));

        //Hide GO
        if (bonusPerc <= 0)
        {
            AdditionalBonusGO.SetActive(false);
        }

        CancelButton.SetInteractable(true);
        DepositButton.SetInteractable(true);

    }

    private void OnSettingsDataUpdated(PaymentSettingsData data)
    {

        UpdateSettingsData(data);
        UpdateLabels();
    }

    private void UpdateSettingsData(PaymentSettingsData data)
    {
        sgstPerc = data.sgst;
        cgstPerc = data.cgst;
        totalGstPerc = sgstPerc + cgstPerc;
        bonusPerc = data.bonus;
        Debug.LogWarning(data);
    }

    public async void OnDepositButtonPressed()
    {
        CancelButton.SetInteractable(false);
        DepositButton.SetInteractable(false);

        OpenPG(amountToDeposit);

    }
    public async void OnDepositButtonPressed_CashFree()
    {
        CancelButton.SetInteractable(false);
        DepositButton.SetInteractable(false);

        OpenPGCashFree(amountToDeposit);

    }
    private async void OpenPGCashFree(int value)
    {
        var encodedData = await GetPaymentDataCashFree(value);

        ////Save Transaction ID
        SecurePlayerPrefs.SetString(Appinop.Constants.KLastTID, encodedData.order_id);
        Application.OpenURL(encodedData.payment_link);
#if UNITY_ANDROID && !UNITY_EDITOR
       

    //    SecureWebViewHelper.Instance.OpenPaymentWebView(encodedData.payment_link).OnResponseReceived(OnPaymentResponse);
        SecureWebViewHelper.Instance.OpenPaymentWebView(encodedData.payment_link).OnResponseReceived((responce) =>
        {
            Debug.Log("Responce : " + responce);
            OnPaymentResponse(responce);
        });
#endif
        //SecureWebViewHelper.Instance.OpenPaymentWebView(encodedData.payment_link).OnResponseReceived((responce) =>
        //{
        //    Debug.Log("Responce : " + responce);
        //    OnPaymentResponse(responce);
        //});

        //PhonePeHelper.Instance.StartTransaction(encodedData.base64Payload, encodedData.checksum, ProcessCallBack);
        //var transaction = await GetTransactionDetailCashFree(encodedData.order_id);

    }
    //1. Discard Issue Fixed.
    //2 Joker implement
    //3.Card ReShuffle implemented
    //4. Application Leader Board data show 
    //5 CashFree test implemented
    //6. Automatic Kyc implement

    private async void OnPaymentResponse(string status)
    {
        Debug.Log("Payment Response: " + status);

        Debug.LogError("Payment Status : " + status);
        CancelButton.SetInteractable(true);
        DepositButton.SetInteractable(true);
        this.gameObject.SetActive(false);

        Loader.Instance.Show("Payment Status : " + status);

        string tID = SecurePlayerPrefs.GetString(Appinop.Constants.KLastTID);

        if (string.IsNullOrEmpty(tID))
        {
            Loader.Instance.Hide();
            AlertSlider.Instance.Show("Something went wrong.\nTry again later", "OK");
            PopoverViewController.Instance.GoBack();

            return;
        }

        Loader.Instance.Show("Transaction ID : " + tID);
        var transaction = await GetTransactionDetailCashFree(tID);
        Loader.Instance.Show("Status : " + transaction.message);

        if (transaction.success && transaction.message.ToLower().Equals("Success".ToLower()))
        {
            await UserDataContext.Instance.RefreshData();
            await WalletDataContext.Instance.RefreshData();

            AlertSlider.Instance.Show($"Payment Successful.\n<b>₹ {transaction.data.amount}</b> added to your account.", "OK")
                .OnPrimaryAction(() =>
                {
                    AlertSlider.Instance.Hide();
                    PopoverViewController.Instance.GoBack();
                });
        }
        else if (!transaction.success)
        {
            await UserDataContext.Instance.RefreshData();
            await WalletDataContext.Instance.RefreshData();

            AlertSlider.Instance.Show($"Transaction Pending.\nPlease Check back Later.", "OK")
                .OnPrimaryAction(() =>
                {
                    AlertSlider.Instance.Hide();
                    PopoverViewController.Instance.GoBack();
                });
        }
        else
        {
            AlertSlider.Instance.Show($"Payment Failed.\nPlease Try again!", "OK");
        }
        Loader.Instance.Hide();
    }

    public class PaymentRequest
    {
        public string amount { get; set; }
    }
    private async Task<Transaction> GetTransactionDetailCashFree(string order_id)
    {

        string requestUrl = $"{APIEndpoints.checkTransactionCashFree}?order_id={order_id}";
        var responce = await APIServices.Instance.GetAsync<Transaction>(requestUrl);
        return responce;
    }
    // Usage:


    private async Task<PaymentData> GetPaymentDataCashFree(int value)
    {
        PaymentRequest request = new PaymentRequest { amount = value.ToString() };
        string requestBodyJson = JsonConvert.SerializeObject(request);

        // value = 1;

        var responce = await APIServices.Instance.PostAsync<CashfreePaymentResponse>(APIEndpoints.requestCashFree, requestBodyJson, true);

        if (responce != null && responce.success)
        {
            return responce.data;
        }
        else
        {
            return null;
        }
    }


    public void OnCancelButtonPressed()
    {
        AlertSlider.Instance.Show("Are You sure ?\nDo You want to cancel the transaction?", "Cancel Transaction", "Deposit Money")
        .OnPrimaryAction(() => PopoverViewController.Instance.GoBack());
    }


    private async void OpenPG(int value)
    {
        var encodedData = await GetPaymentData(value);

        //Save Transaction ID
        SecurePlayerPrefs.SetString(Appinop.Constants.KLastTID, encodedData.transactionId);

        PhonePeHelper.Instance.StartTransaction(encodedData.base64Payload, encodedData.checksum, ProcessCallBack);
    }


    private async void ProcessCallBack(string status)
    {

        Debug.LogError("Payment Status : " + status);
        CancelButton.SetInteractable(true);
        DepositButton.SetInteractable(true);

        Loader.Instance.Show("Payment Status : " + status);

        string tID = SecurePlayerPrefs.GetString(Appinop.Constants.KLastTID);

        if (string.IsNullOrEmpty(tID))
        {
            Loader.Instance.Hide();
            AlertSlider.Instance.Show("Something went wrong.\nTry again later", "OK");
            PopoverViewController.Instance.GoBack();

            return;
        }

        Loader.Instance.Show("Transaction ID : " + tID);
        var transaction = await GetTransactionDetail(tID);
        Loader.Instance.Show("Status : " + transaction.message);

        if (transaction.success && transaction.message.ToLower().Equals("Success".ToLower()))
        {
           await UserDataContext.Instance.RefreshData();
            await WalletDataContext.Instance.RefreshData();

            AlertSlider.Instance.Show($"Payment Successful.\n<b>₹ {transaction.data.amount}</b> added to your account.", "OK")
                .OnPrimaryAction(() =>
                {
                    AlertSlider.Instance.Hide();
                    PopoverViewController.Instance.GoBack();
                });
        }
        else if (!transaction.success)
        {
            await UserDataContext.Instance.RefreshData();
            await WalletDataContext.Instance.RefreshData();

            AlertSlider.Instance.Show($"Transaction Pending.\nPlease Check back Later.", "OK")
                .OnPrimaryAction(() =>
                {
                    AlertSlider.Instance.Hide();
                    PopoverViewController.Instance.GoBack();
                });
        }
        else
        {
            AlertSlider.Instance.Show($"Payment Failed.\nPlease Try again!", "OK");
        }
        Loader.Instance.Hide();

    }

    private async Task<PayPageData2> GetPaymentData(int value)
    {
        // value = 1;
        string requestBodyJson = "{\"amount\":" + value + "}";
        var responce = await APIServices.Instance.PostAsync<PayPage2>(APIEndpoints.requestDeposit, requestBodyJson, true);

        if (responce != null && responce.success)
        {
            return responce.data;
        }
        else
        {
            return null;
        }
    }
    private async Task<Transaction> GetTransactionDetail(string transactionID)
    {
        var responce = await APIServices.Instance.GetAsync<Transaction>(APIEndpoints.checkTransaction + transactionID);
        return responce;
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




