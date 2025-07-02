using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Appinop;
using LottiePlugin.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllGameHistoryPopover : PopoverView
{
    internal class GameHistoryData
    {
        public string _id { get; set; }
        public List<UserData> players { get; set; }
        public string tableId { get; set; }
        public List<UserData> winner { get; set; }
        public DateTime gameStartedDate { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public DateTime gameWonDate { get; set; }
    }

    [SerializeField] private bool isLoaded = false;
    private RectTransform rectTransform;
    [SerializeField] AnimatedImage loader;

    [Space]
    [SerializeField] List<ScrollRect> scrollViews;

    [Header("2 Players")]
    [SerializeField] GameObject nothingToShowTwoPlayer;
    [SerializeField] ScrollRect TwoPlayerScroll;
    [SerializeField] List<GameHistoryCell> TwoPlayerCells;

    [Header("4 Players")]
    [SerializeField] GameObject nothingToShowFourPlayer;
    [SerializeField] ScrollRect FourPlayerScroll;
    [SerializeField] List<GameHistoryCell> FourPlayerCells;


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
            loader.gameObject.SetActive(true);
            nothingToShowTwoPlayer.SetActive(false);
            nothingToShowFourPlayer.SetActive(false);

            scrollViews.ForEach(s => s.transform.parent.gameObject.SetActive(false));
        }

        if (!rectTransform)
        {
            rectTransform = this.transform as RectTransform;
        }

        UpdateList();

        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left);
        this.gameObject.SetActive(true);
        rectTransform.MoveToPosition(Vector2.zero, duration: 0.2f);
    }

    private async Task<List<GameHistoryData>> FetchData()
    {

        var response = await APIServices.Instance.GetAsync<BaseModel<List<GameHistoryData>>>(APIEndpoints.getMatches);
        if (response != null && response.success)
        {
            return response.data;
        }
        else
        {
            return new List<GameHistoryData>();
        }
    }

    private async void UpdateList()
    {

        var data = await FetchData();


        List<TableData> tableData = DataContext.Instance.contestsData.FindAll(t => t.isActive);

        List<TableData> TwoPlayerGameTables = tableData.FindAll(t => t.gameType == "2 Player");


        List<TableData> FourPlayerGameTables = tableData.FindAll(t => t.gameType == "4 Player");

        List<GameHistoryData> filteredTwoPlayerGames = data
            .Where(game => game.winner.Any(win => win._id == UserDataContext.Instance.UserData._id) && TwoPlayerGameTables.Any(table => table._id == game.tableId))
            .ToList();

        List<GameHistoryData> filteredFourPlayerGames = data
            .Where(game => game.winner.Any(win => win._id == UserDataContext.Instance.UserData._id) && FourPlayerGameTables.Any(table => table._id == game.tableId))
            .ToList();

        UpdateUI(filteredTwoPlayerGames, filteredFourPlayerGames);
    }

    private void UpdateUI(List<GameHistoryData> TwoPData, List<GameHistoryData> FOurPData)
    {
        loader.Stop();
        loader.gameObject.SetActive(false);


        if (TwoPData.Count > 0)
        {
            int requiredCells = TwoPData.Count;

            if (TwoPlayerCells.Count > requiredCells)
            {
                while (TwoPlayerCells.Count > requiredCells)
                {
                    GameHistoryCell cell = TwoPlayerCells.Last();
                    TwoPlayerCells.Remove(cell);
                    DestroyImmediate(cell.gameObject);
                }
            }
            else if (TwoPlayerCells.Count == requiredCells)
            {

            }
            else
            {
                while (TwoPlayerCells.Count < requiredCells)
                {
                    GameHistoryCell cell = Instantiate(TwoPlayerCells.Last(), TwoPlayerScroll.content);
                    TwoPlayerCells.Add(cell);
                }
            }

            for (int i = 0; i < requiredCells; i++)
            {

                TableData tableData = DataContext.Instance.contestsData.Find(t => t._id == TwoPData[i].tableId);
                TwoPlayerCells[i].UpdateData(2, tableData.wonCoin, tableData.bet, TwoPData[i].gameWonDate, TwoPData[i].winner[0]._id == UserDataContext.Instance.UserData._id);
            }

            TwoPlayerScroll.transform.parent.gameObject.SetActive(true);

        }
        else
        {
            nothingToShowTwoPlayer.SetActive(true);
        }

        //== Four Player
        if (FOurPData.Count > 0)
        {
            int requiredCells = FOurPData.Count;

            if (FourPlayerCells.Count > requiredCells)
            {
                while (FourPlayerCells.Count > requiredCells)
                {
                    GameHistoryCell cell = FourPlayerCells.Last();
                    FourPlayerCells.Remove(cell);
                    DestroyImmediate(cell.gameObject);
                }
            }
            else if (FourPlayerCells.Count == requiredCells)
            {

            }
            else
            {
                while (FourPlayerCells.Count < requiredCells)
                {
                    GameHistoryCell cell = Instantiate(FourPlayerCells.Last(), FourPlayerScroll.content);
                    FourPlayerCells.Add(cell);
                }
            }

            for (int i = 0; i < requiredCells; i++)
            {

                TableData tableData = DataContext.Instance.contestsData.Find(t => t._id == FOurPData[i].tableId);
                FourPlayerCells[i].UpdateData(2, tableData.wonCoin, tableData.bet, FOurPData[i].gameWonDate, FOurPData[i].winner[0]._id == UserDataContext.Instance.UserData._id);
            }

            FourPlayerScroll.transform.parent.gameObject.SetActive(true);

        }
        else
        {
            nothingToShowFourPlayer.SetActive(true);
        }

    }
    private void ResetView()
    {
        scrollViews.ForEach(s => s.content.anchoredPosition = Vector2.zero);
    }
    public override void OnFocus(bool dataUpdated = false)
    {
    }

}
