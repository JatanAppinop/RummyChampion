using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinGameButton : MonoBehaviour
{

    private TextMeshProUGUI gameBtnTxt;
    private Button gameBtn;
    private WinType winType;
    private GameManager gameManager;

    private delegate void FinishMethod();
    FinishMethod finishMethod;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        gameBtn = GetComponent<Button>();
        gameBtnTxt = GetComponentInChildren<TextMeshProUGUI>();
        gameBtn.onClick.AddListener(Click);
    }

    public void ShowKnockBtn(bool btnState)
    {
        gameObject.SetActive(btnState);
    }

    public void SetDesc(string desc)
    {
        gameBtnTxt.SetText("Finish");
    }

    public void SetWinType(WinType winType, int knockScore)
    {
        this.winType = winType;
        switch (winType)
        {
            case WinType.Knock:
                SetDesc("Knock for " + knockScore);
                finishMethod = FinishGameByKnock;
                break;
            case WinType.Gin:
                SetDesc("Gin");
                finishMethod = FinishGameByGin;
                break;
            case WinType.BigGin:
                SetDesc("Big Gin");
                finishMethod = FinishGameByBigGin;
                break;
            default:
                break;
        }
    }

    private void FinishGameByKnock()
    {
        gameManager.OnKnock();
    }

    private void FinishGameByGin()
    {
        gameManager.OnGin();
    }

    private void FinishGameByBigGin()
    {
        gameManager.OnBigGin();
    }

    public void Click()
    {
        finishMethod();
        gameObject.SetActive(false);
    }
}
