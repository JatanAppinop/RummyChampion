using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Appinop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LudoConfirmationPopoverView : PopoverView
{
    [SerializeField] private bool isLoaded = false;

    [Space]
    [SerializeField] TextMeshProUGUI modeLabel;
    [SerializeField] TextMeshProUGUI betAmout;
    [SerializeField] TextMeshProUGUI winAmout;
    [SerializeField] TabGroup mainTabBar;

    private RectTransform rectTransform;


    private string contestID;

    private TableData tableData;

    private void Awake()
    {
        this.rectTransform = this.transform as RectTransform;


    }
    public override void Hide()
    {
        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left, Animate: true, onComplete: () =>
        {
            ResetView();
            this.gameObject.SetActive(false);
        });
    }

    public override void Show()
    {
        if (!isLoaded)
        {
            isLoaded = true;
        }

        this.gameObject.SetActive(true);
        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left);
        this.gameObject.SetActive(true);
        rectTransform.MoveToPosition(Vector2.zero, duration: 0.2f);

    }

    public override void Show(params KeyValuePair<string, object>[] args)
    {
        if (!isLoaded)
        {
            isLoaded = true;
        }

        this.gameObject.SetActive(true);
        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left);
        this.gameObject.SetActive(true);
        rectTransform.MoveToPosition(Vector2.zero, duration: 0.2f);

        tableData = args.GetValueByKey<TableData>(Appinop.Constants.KContestId);

        string mode = args.GetValueByKey<string>(Appinop.Constants.KGameMode);
        Debug.Log("Selected Mode : " + mode);

        UpdateView(tableData, mode);
    }

    private void UpdateView(TableData detail, string mode)
    {
        modeLabel.SetText(mode);
        betAmout.SetText("₹" + detail.bet);
        winAmout.SetText("₹" + detail.wonCoin);

    }
    private void ResetView()
    {
    }


    public override void OnFocus(bool dataUpdated = false)
    {
    }

    public async void OnConfirmationButtonPressed()
    {

        if (tableData.game.ToLower() == "ludo")
        {

            if (WalletDataContext.Instance.WalletData.depositBalance >= tableData.bet || WalletDataContext.Instance.WalletData.winningBalance >= tableData.bet)
            {
                PopoverViewController.Instance.Show(PopoverViewController.Instance.ludoMatchFindingPopover,
                new KeyValuePair<string, object>(Appinop.Constants.KContestId, tableData._id));
            }
            else
            {
                AlertSlider.Instance.Show("Insufficient Balance", "Add Funds", "Cancel")
                .OnPrimaryAction(() =>
                {
                    AlertSlider.Instance.Hide();
                    mainTabBar.NavigateTo("Shop");
                    PopoverViewController.Instance.GoBackTo(100);

                })
                .OnSecondaryAction(() =>
                {
                    AlertSlider.Instance.Hide();
                });
            }
        }
        else if (tableData.game.ToLower() == "rummy")
        {
            double joiningFees = 0;

            if (tableData.gameMode.ToLower() == "points")
                joiningFees = tableData.pointValue * 80;
            else
                joiningFees = tableData.bet;

            if (WalletDataContext.Instance.WalletData.depositBalance >= joiningFees || WalletDataContext.Instance.WalletData.winningBalance >= joiningFees)
            {
                PopoverViewController.Instance.Show(PopoverViewController.Instance.ludoMatchFindingPopover,
                new KeyValuePair<string, object>(Appinop.Constants.KContestId, tableData._id));
            }
            else
            {
                AlertSlider.Instance.Show($"Insufficient Balance\n You need atleast {joiningFees} ₹ to join this content.", "Add Funds", "Cancel")
                .OnPrimaryAction(() =>
                {
                    AlertSlider.Instance.Hide();
                    mainTabBar.NavigateTo("Shop");
                    PopoverViewController.Instance.GoBackTo(100);

                })
                .OnSecondaryAction(() =>
                {
                    AlertSlider.Instance.Hide();
                });
            }

        }
    }
}
