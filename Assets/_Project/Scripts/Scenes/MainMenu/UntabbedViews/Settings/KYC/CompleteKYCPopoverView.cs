using System.Collections;
using System.Collections.Generic;
using Appinop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompleteKYCPopoverView : PopoverView
{
    [SerializeField] private bool isLoaded = false;

    private RectTransform rectTransform;

    [SerializeField] Button panButton;
    [SerializeField] Button dlButton;
    [SerializeField] Button aadharButton;

    private enum KYCStatus
    {
        NotApplied,
        Approved,
        Pending,
        Rejected
    }
    private void Awake()
    {
        this.rectTransform = this.transform as RectTransform;
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

            UserDataContext.Instance.UserdataChanged.AddListener(OnUserDataChanged);
            UpdateData(UserDataContext.Instance.UserData);
        }

        if (!rectTransform)
        {
            rectTransform = this.transform as RectTransform;
        }

        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left);
        this.gameObject.SetActive(true);
        rectTransform.MoveToPosition(Vector2.zero, duration: 0.2f);
    }

    public override void OnFocus(bool dataUpdated = false)
    {
        if (dataUpdated)
        {
            UserDataContext.Instance.RefreshData();
        }
    }

    private void OnUserDataChanged(UserData data)
    {
        UpdateData(data);
    }


    private void UpdateData(UserData data)
    {
        switch (data.kycVerified.ToLower())
        {
            case "approved":
                panButton.GetComponentInChildren<TextMeshProUGUI>().text = "Approved";
                panButton.interactable = false;
                dlButton.GetComponentInChildren<TextMeshProUGUI>().text = "Approved";
                dlButton.interactable = false;
                aadharButton.GetComponentInChildren<TextMeshProUGUI>().text = "Approved";
                aadharButton.interactable = false;
                break;
            case "pending":
                panButton.GetComponentInChildren<TextMeshProUGUI>().text = "Pending";
                panButton.interactable = false;
                dlButton.GetComponentInChildren<TextMeshProUGUI>().text = "Pending";
                dlButton.interactable = false;
                aadharButton.GetComponentInChildren<TextMeshProUGUI>().text = "Pending";
                aadharButton.interactable = false;
                break;

            default:
                panButton.GetComponentInChildren<TextMeshProUGUI>().text = "Verify";
                panButton.interactable = true;
                dlButton.GetComponentInChildren<TextMeshProUGUI>().text = "Verify";
                dlButton.interactable = true;
                aadharButton.GetComponentInChildren<TextMeshProUGUI>().text = "Verify";
                aadharButton.interactable = true;
                break;
        }
    }
}