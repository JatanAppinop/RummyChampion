using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Appinop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WithdrawPopoverView : PopoverView
{
    [SerializeField] private bool isLoaded = false;
    private RectTransform rectTransform;

    [SerializeField] ScrollRect scrollRect;
    [SerializeField] TextMeshProUGUI winningsLabel;
    [SerializeField] TextMeshProUGUI minWithdrawLabel;
    [SerializeField] GameObject selectBankAccountParent;
    [SerializeField] PaymentAccountCell paymentAccountPrefeb;
    [SerializeField] PaymentAccountCell selectedCell;
    [SerializeField] List<PaymentAccountCell> paymentAccounts = new List<PaymentAccountCell>();
    [SerializeField] AdvancedInputField amountTextField;
    [SerializeField] PrimaryButton withdrawButton;
    [SerializeField] GameObject addAccountParent;
    [SerializeField] GameObject addBankAccount;
    [SerializeField] GameObject addUPIAccount;

    private void Awake()
    {
        this.rectTransform = this.transform as RectTransform;
    }

    public override void Hide()
    {
        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left, Animate: true, onComplete: () =>
        {
            this.gameObject.SetActive(false);
            ResetView();
        });
    }

    public override void Show()
    {
        if (!isLoaded)
        {
            isLoaded = true;
            winningsLabel.SetText("...");
            WalletDataContext.Instance.WalletDataChanged.AddListener(OnWalletDataChanged);
            UpdateWalletData(WalletDataContext.Instance.WalletData);
            selectBankAccountParent.gameObject.SetActive(false);
        }

        if (!rectTransform)
        {
            rectTransform = this.transform as RectTransform;
        }

        WalletDataContext.Instance.RefreshData();


        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left);
        this.gameObject.SetActive(true);
        rectTransform.MoveToPosition(Vector2.zero, duration: 0.2f, onComplete: () =>
        {
            UpdateView();
        });
    }

    private async void UpdateView()
    {
        Loader.Instance.Show();
        ResetView();
        List<PaymentAccount> accounts = await GetBankAccounts();

        if (accounts.Count > 0)
        {
            foreach (PaymentAccount account in accounts)
            {
                if (account.active)
                {
                    PaymentAccountCell cell = Instantiate(paymentAccountPrefeb, selectBankAccountParent.transform as RectTransform);
                    cell.UpdateData(account);
                    cell.onSelected.AddListener(onBankAccountSelected);
                    paymentAccounts.Add(cell);
                }
            }

            if (paymentAccounts.Count > 0)
            {
                selectBankAccountParent.gameObject.SetActive(true);

                if (paymentAccounts.Find(x => x.paymentAccount.accountType == "vpa") != null)
                    addUPIAccount.SetActive(false);
                else
                    addUPIAccount.SetActive(true);

                if (paymentAccounts.Find(x => x.paymentAccount.accountType == "bank_account") != null)
                    addBankAccount.SetActive(false);
                else
                    addBankAccount.SetActive(true);

                if (!addUPIAccount.activeSelf && !addBankAccount.activeSelf)
                    addAccountParent.SetActive(false);
                else
                    addAccountParent.SetActive(true);

            }
            else
            {
                selectBankAccountParent.gameObject.SetActive(false);

            }
        }
        else
        {
            selectBankAccountParent.gameObject.SetActive(false);
            addBankAccount.SetActive(true);
            addUPIAccount.SetActive(true);
            addAccountParent.SetActive(true);


        }
        Loader.Instance.Hide();

    }

    private void OnWalletDataChanged(WalletData data)
    {
        UpdateWalletData(data);
    }
    private void UpdateWalletData(WalletData data)
    {
        winningsLabel.SetText(data.winningBalance.ToTwoDecimalString());
        minWithdrawLabel.SetText($"Minimum Withdrawal {data.minimumWithdraw}₹");
    }
    private void ResetView()
    {
        scrollRect.content.anchoredPosition = Vector2.zero;
        paymentAccounts.ForEach(x => Destroy(x.gameObject));
        paymentAccounts.Clear();
        amountTextField.text = "";
        amountTextField.HideError();
    }
    public override void OnFocus(bool dataUpdated = false)
    {
        if (dataUpdated)
        {
            UpdateView();
        }
    }

    public async Task<List<PaymentAccount>> GetBankAccounts()
    {

        var response = await APIServices.Instance.GetAsync<PaymentAccounts>(APIEndpoints.getFunAccounts);
        if (response != null && response.success)
        {
            return response.data;
        }
        else
        {
            return new List<PaymentAccount>();
        }
    }

    private void onBankAccountSelected(PaymentAccountCell selectedCell)
    {
        this.selectedCell = selectedCell;
        paymentAccounts.ForEach(x => x.Deselect());
        selectedCell.Select();
        withdrawButton.SetInteractable(true);

    }

    public async void onWithdrawButtonPressed()
    {

        if (UserDataContext.Instance.UserData.kycVerified.ToLower() == "pending")
        {

            AlertSlider.Instance.Show("Your KYC is not Verified yet.\nPlease wait for sometime.", "Ok")
            .OnPrimaryAction(() =>
            {
                AlertSlider.Instance.Hide();

            });

            return;
        }
        if (UserDataContext.Instance.UserData.kycVerified.ToLower() != "approved")
        {

            AlertSlider.Instance.Show("Please Verify KYC First", "Verify KYC", "Cancel")
            .OnPrimaryAction(() =>
            {
                AlertSlider.Instance.Hide();
                PopoverViewController.Instance.GoBack();
                PopoverViewController.Instance.Show(PopoverViewController.Instance.completeKYCPopover);

            })
            .OnSecondaryAction(() =>
            {
                AlertSlider.Instance.Hide();
            });

            return;
        }

        withdrawButton.SetInteractable(false);

        if (paymentAccounts.Count < 1)
        {
            AlertSlider.Instance.Show("Please Add Withdrawal Account.", "OK").OnPrimaryAction(() => AlertSlider.Instance.Hide());
            withdrawButton.SetInteractable(true);

            return;
        }

        if (selectedCell == null)
        {
            AlertSlider.Instance.Show("Please Select Withdrawal Account", "OK").OnPrimaryAction(() => AlertSlider.Instance.Hide());
            withdrawButton.SetInteractable(true);

            return;
        }

        int withdrawAmount = 0;

        int.TryParse(amountTextField.text, out withdrawAmount);

        if (withdrawAmount <= 0)
        {
            amountTextField.ShowError($"Please Enter Amount to withdraw");
            withdrawButton.SetInteractable(true);
            return;
        }

        if (withdrawAmount > WalletDataContext.Instance.WalletData.winningBalance)
        {
            amountTextField.ShowError($"You do not have enough withdrawable balance.");
            withdrawButton.SetInteractable(true);

            return;
        }
        if (withdrawAmount < WalletDataContext.Instance.WalletData.minimumWithdraw)
        {
            amountTextField.ShowError($"You can not withdraw less than {WalletDataContext.Instance.WalletData.minimumWithdraw.ToTwoDecimalString()}");
            withdrawButton.SetInteractable(true);

            return;
        }

        PopoverViewController.Instance.Show(PopoverViewController.Instance.withdrawCalc,
                new KeyValuePair<string, object>(Appinop.Constants.KWithdrawAmount, withdrawAmount),
                new KeyValuePair<string, object>(Appinop.Constants.KPaymentAccount, selectedCell.paymentAccount)
                );

        withdrawButton.SetInteractable(true);


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

}
