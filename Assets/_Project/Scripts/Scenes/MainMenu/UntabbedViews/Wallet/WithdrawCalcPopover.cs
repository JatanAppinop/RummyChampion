using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Appinop;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WithdrawCalcPopover : PopoverView
{
    [SerializeField] private bool isLoaded = false;
    private RectTransform rectTransform;
    [SerializeField] Image backdrop;

    [Space]
    [Header("Label Reference")]
    [SerializeField] TextMeshProUGUI tdsLabel;
    [SerializeField] TextMeshProUGUI withdrawLabel;

    [Space]
    [Header("Value Reference")]
    [SerializeField] TextMeshProUGUI amountToWithdrawValLabel;
    [SerializeField] TextMeshProUGUI tdsValLabel;
    [SerializeField] TextMeshProUGUI withdrawalChargeValLabel;
    [SerializeField] TextMeshProUGUI recievableValLabel;
    [Header("Other Reference")]
    [SerializeField] PrimaryButton withdrawButton;
    [SerializeField] PrimaryButton CancelButton;
    [Header("Private Reference")]
    [SerializeField] string rs = "₹";
    [SerializeField] double tdsPerc;
    [SerializeField] double withdrawPerc;
    [SerializeField] int amountToWithdraw;
    [SerializeField] PaymentAccount paymentAccount;


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


        amountToWithdraw = args.GetValueByKey<int>(Appinop.Constants.KWithdrawAmount);
        paymentAccount = args.GetValueByKey<PaymentAccount>(Appinop.Constants.KPaymentAccount);

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
        string tdsText = tdsLabel.text;
        tdsText = tdsText.Replace("#", $"({tdsPerc} %)");
        tdsLabel.SetText(tdsText);

        string withdrawText = withdrawLabel.text;
        withdrawText = withdrawText.Replace("#", $"({withdrawPerc} %)");
        withdrawLabel.SetText(withdrawText);

        amountToWithdrawValLabel.SetText(rs + amountToWithdraw.ToTwoDecimalString());

        // Calculate the factor
        double factor = 1 + ((tdsPerc + withdrawPerc) / 100);

        // Calculate the base amount before GST
        double baseAmount = amountToWithdraw / factor;
        recievableValLabel.SetText(rs + baseAmount.ToTwoDecimalString(true));

        // Calculate TDS and Withdrawal amounts
        double tdsAmount = baseAmount * (tdsPerc / 100);
        tdsValLabel.SetText("-" + rs + tdsAmount.ToTwoDecimalString(true));
        double withdrawAmount = baseAmount * (withdrawPerc / 100);
        withdrawalChargeValLabel.SetText("-" + rs + withdrawAmount.ToTwoDecimalString(true));


        double totalAmount = baseAmount + tdsAmount + withdrawAmount;
        totalAmount = Math.Round(totalAmount);
        if (totalAmount != amountToWithdraw)
        {
            Debug.LogError("Amount Mismatch : " + totalAmount + "" + amountToWithdraw);
        }
    }

    private void OnSettingsDataUpdated(PaymentSettingsData data)
    {

        UpdateSettingsData(data);
        UpdateLabels();
    }

    private void UpdateSettingsData(PaymentSettingsData data)
    {
        tdsPerc = data.tds;
        withdrawPerc = data.withdrawCharge;
    }

    public async void OnWithdrawButtonPressed()
    {
        CancelButton.SetInteractable(false);
        withdrawButton.SetInteractable(false);


        AlertSlider.Instance.Show($"Do you want to withdraw <b>₹{amountToWithdraw}</b>?", $"Withdraw <b>₹{amountToWithdraw}</b>", "Cancel")
        .OnPrimaryAction(async () =>
        {
            SimpleBaseModel responce = await RequestWithdrawal(amountToWithdraw, paymentAccount);
            WalletDataContext.Instance.RefreshData();

            ProcessWithdrawalRequest(responce);
            CancelButton.SetInteractable(true);
            withdrawButton.SetInteractable(true);

        })
        .OnSecondaryAction(() =>
        {
            CancelButton.SetInteractable(true);
            withdrawButton.SetInteractable(true);

        });



    }

    private void ProcessWithdrawalRequest(SimpleBaseModel responce)
    {
        if (responce != null && responce.success)
        {
            AlertSlider.Instance.Show("Withdraw request submitted Successfully.\nYour Withdrawal will be processed in 48 Hours", "OK")
            .OnPrimaryAction(() =>
            {
                PopoverViewController.Instance.GoBackTo(2);
                UserDataContext.Instance.RefreshData();
                AlertSlider.Instance.Hide();
            });
        }
        else if (responce != null && !responce.success)
        {
            AlertSlider.Instance.Show($"Withdraw request Failed.\n{responce.message}", "OK")
            .OnPrimaryAction(() =>
            {
                PopoverViewController.Instance.GoBackTo(2);
                AlertSlider.Instance.Hide();
            });

        }
        else
        {
            AlertSlider.Instance.Show($"Withdraw request Failed.\nTry again later.", "OK")
            .OnPrimaryAction(() =>
            {
                PopoverViewController.Instance.GoBackTo(2);
                AlertSlider.Instance.Hide();

            });

        }
    }

    public void OnCancelButtonPressed()
    {
        AlertSlider.Instance.Show("Are You sure ?\nDo You want to cancel the transaction?", "Cancel Transaction", "Withdraw Money")
        .OnPrimaryAction(() => PopoverViewController.Instance.GoBackTo(2));
    }

    private async Task<SimpleBaseModel> RequestWithdrawal(int value, PaymentAccount fundAcc)
    {
        string requestBodyJson = $"{{\"amount\":{value},\"fundAccountId\":\"{fundAcc.fundAccountId}\",\"accountType\":\"{fundAcc.accountType}\"}}";
        var responce = await APIServices.Instance.PostAsync<SimpleBaseModel>(APIEndpoints.requestWithdrawal, requestBodyJson, true);

        if (responce != null)
        {
            return responce;
        }
        else
        {
            return default;
        }
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
