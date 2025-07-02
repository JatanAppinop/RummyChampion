using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PaymentAccountCell : MonoBehaviour
{
    public enum PaymentAccountType
    {
        Upi,
        BankAccount
    }
    [SerializeField] TextMeshProUGUI accountLabel;
    [SerializeField] Image icon;
    [SerializeField] Button button;
    [SerializeField] GameObject tickMark;

    public PaymentAccount paymentAccount;
    public PaymentAccountType accountType;

    [HideInInspector]
    public UnityEvent<PaymentAccountCell> onSelected;

    [SerializeField] List<BankAccountIcon> icons = new List<BankAccountIcon>();


    public bool isSelected = false;
    private void Awake()
    {
        button.onClick.AddListener(OnSelect);
        Deselect();
    }

    private void OnSelect()
    {
        onSelected?.Invoke(this);
    }


    public void UpdateData(PaymentAccount data)
    {
        paymentAccount = data;
        switch (data.accountType)
        {
            case "vpa":
                accountType = PaymentAccountType.Upi;
                accountLabel.SetText(MaskUPIId(data.vpa.address));
                icon.sprite = icons.Find(i => i.type == PaymentAccountType.Upi).icon;
                break;
            case "bank_account":
                accountType = PaymentAccountType.BankAccount;
                accountLabel.SetText("xxxxxxx" + GetLastFourDigits(data.bankAccount.account_number));
                icon.sprite = icons.Find(i => i.type == PaymentAccountType.BankAccount).icon;
                break;
        }

    }

    public void Select()
    {
        isSelected = true;
        tickMark.gameObject.SetActive(isSelected);
    }

    public void Deselect()
    {
        isSelected = false;
        tickMark.gameObject.SetActive(isSelected);
    }
    public string GetLastFourDigits(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty; // Handle null or empty string case
        }
        if (input.Length <= 4)
        {
            return input;
        }
        return input.Substring(input.Length - 4);
    }

    public static string MaskUPIId(string upiId)
    {
        int atIndex = upiId.IndexOf('@');

        if (atIndex == -1)
        {
            return upiId; // Invalid UPI ID (no '@' found)
        }

        char[] chars = upiId.ToCharArray();

        // Replace every other character before the '@'
        for (int i = 0; i < atIndex; i += 2)
        {
            chars[i] = 'X';
        }

        return new string(chars);
    }

    private void OnDestroy()
    {
        onSelected.RemoveAllListeners();
    }

    [Serializable]
    internal class BankAccountIcon
    {
        public PaymentAccountType type;
        public Sprite icon;
    }
}