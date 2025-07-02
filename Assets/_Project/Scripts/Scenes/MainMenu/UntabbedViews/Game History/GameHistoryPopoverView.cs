using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Appinop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHistoryPopoverView : PopoverView
{
    [SerializeField] private bool isLoaded = false;
    private RectTransform rectTransform;

    [SerializeField] TextMeshProUGUI title;
    [SerializeField] ScrollRect scrollView;

    [SerializeField] GameObject ludoNoContent;
    [SerializeField] List<GameHistoryCell> ludoCells;
    [SerializeField] GameHistoryCell prefab;
    [SerializeField] List<PlayerWin> playerWins;

    [SerializeField] GameObject ludoHeader;
    [SerializeField] TextMeshProUGUI ludoTotalGames;
    [SerializeField] GameObject rummyHeader;
    [SerializeField] TextMeshProUGUI rummyTotalGames;


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

        string titleStr = args.GetValueByKey<string>("KTitle");
        // int playerCount = args.GetValueByKey<int>("KPlayers");
        string gameName = args.GetValueByKey<string>("KGame");


        List<PlayerWin> data;
        try
        {
            data = args.GetValueByKey<List<PlayerWin>>("KData");
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            data = new List<PlayerWin>();
        }

        title.SetText(titleStr);

        if (gameName == "ludo")
        {
            ludoHeader.SetActive(true);
            rummyHeader.SetActive(false);
            ludoTotalGames.SetText(data.Count.ToString());
        }
        else if (gameName == "rummy")
        {
            ludoHeader.SetActive(false);
            rummyHeader.SetActive(true);
            rummyTotalGames.SetText(data.Count.ToString());
        }
        // var filteredData = data.Where(x => x.wonCoin > 0).ToList();

        ShowData(data);

        playerWins = data;

    }

    private void ShowData(List<PlayerWin> PlayerWinData)
    {

        if (PlayerWinData.Count > 0)
        {
            Debug.Log("Ttoal 2 player Wins : " + PlayerWinData.Count);


            int requiredCells = PlayerWinData.Count;

            if (ludoCells.Count > requiredCells)
            {
                while (ludoCells.Count > requiredCells)
                {
                    GameHistoryCell cell = ludoCells.Last();
                    ludoCells.Remove(cell);
                    DestroyImmediate(cell.gameObject);
                }
            }
            else if (ludoCells.Count == requiredCells)
            {

            }
            else
            {
                while (ludoCells.Count < requiredCells)
                {
                    GameHistoryCell cell = Instantiate(prefab, scrollView.content);
                    ludoCells.Add(cell);
                }
            }

            for (int i = 0; i < requiredCells; i++)
            {
                ludoCells[i].UpdateData(2, PlayerWinData[i]);
            }

            ludoNoContent.gameObject.SetActive(false);
            scrollView.gameObject.SetActive(true);


        }
        else
        {
            ludoNoContent.gameObject.SetActive(true);
            scrollView.gameObject.SetActive(false);
            // ludoCells.ForEach(cell => cell.gameObject.SetActive(false));
        }
    }
    private void ResetView()
    {
        scrollView.content.anchoredPosition = Vector2.zero;
    }

    public override void OnFocus(bool dataUpdated = false)
    {
    }
}
